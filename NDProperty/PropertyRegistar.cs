using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;

namespace NDProperty
{
    public static class PropertyRegistar
    {

        private static readonly Dictionary<Type, List<INDReadOnlyProperty>> inheritedPropertys = new Dictionary<Type, List<INDReadOnlyProperty>>();

        public static NDProperty<TValue, TType> Register<TValue, TType>(Func<TType, OnChanged<TValue>> changedMethod, bool inherited, NullTreatment nullTreatment)
        {
            var p = new NDProperty<TValue, TType>(changedMethod, inherited, nullTreatment);
            if (p.Inherited)
            {
                if (!inheritedPropertys.ContainsKey(typeof(TType)))
                    inheritedPropertys.Add(typeof(TType), new List<INDReadOnlyProperty>());
                inheritedPropertys[typeof(TType)].Add(p);
            }
            return p;
        }
        public static NDAttachedProperty<TValue, TType> RegisterAttached<TValue, TType, THost>(OnChanged<TValue, TType> changedMethod, bool inherited, NullTreatment nullTreatment)
        {
            var p = new NDAttachedProperty<TValue, TType>(changedMethod, inherited, nullTreatment);
            if (p.Inherited)
            {
                if (!inheritedPropertys.ContainsKey(typeof(TType)))
                    inheritedPropertys.Add(typeof(TType), new List<INDReadOnlyProperty>());
                inheritedPropertys[typeof(TType)].Add(p);
            }
            return p;
        }

        public static NDProperty<TValue, TType> RegisterParent<TValue, TType>(Func<TType, OnChanged<TValue>> changedMethod)
        {
            var property = Register(changedMethod, false, NullTreatment.RemoveLocalValue);

            AddEventHandler(property, (sender, e) =>
            {
                var tree = Tree.GetTree(e.ChangedObject);
                var affectedItems = new List<(object affectedObject, object oldValue, INDReadOnlyProperty affectedProperty)>();
                if (inheritedPropertys.Count != 0)
                {
                    // Get all affacted decendents
                    Queue<Tree> queue = new Queue<Tree>();
                    queue.Enqueue(tree);

                    while (queue.Count != 0)
                    {
                        tree = queue.Dequeue();
                        if (inheritedPropertys.ContainsKey(tree.Current.GetType()))
                        {
                            foreach (var affectedProperty in inheritedPropertys[tree.Current.GetType()])
                                affectedItems.Add((tree.Current, affectedProperty.GetValue(tree.Current), affectedProperty));
                        }
                        foreach (var child in tree.Childrean)
                            queue.Enqueue(child);
                    }

                    // reset tree to its original value
                    tree = Tree.GetTree(e.ChangedObject);
                }
                System.Diagnostics.Debug.Assert(Equals(tree.Parent?.Current, e.OldValue));

                if (tree.Parent != null)
                    tree.Parent.Childrean.Remove(tree);

                tree.Parent = Tree.GetTree(e.NewValue);

                if (tree.Parent != null)
                    tree.Parent.Childrean.Add(tree);

                // Notify AffectedItems
                foreach (var item in affectedItems)
                {
                    var newValue = item.affectedProperty.GetValue(item.affectedObject);
                    if (newValue != item.oldValue)
                        item.affectedProperty.CallChangeHandler(item.affectedObject, sender, item.oldValue, newValue);
                }
            });
            return property;
        }


        public static void SetValue<TValue, TType>(NDProperty<TValue, TType> property, TType changedObject, TValue value)
        {
            var oldValue = GetValue(property, changedObject);
            var onChangedArg = OnChangedArg.Create(oldValue, value);
            property.changedMethod(changedObject)(onChangedArg);
            SetValueInternal(property, changedObject, value, onChangedArg);
        }
        public static void SetValue<TValue, TType>(NDAttachedProperty<TValue, TType> property, TType changedObject, TValue value)
        {
            var oldValue = GetValue(property, changedObject);
            var onChangedArg = OnChangedArg.Create(changedObject, oldValue, value);
            property.changedMethod(onChangedArg);
            SetValueInternal(property, changedObject, value, onChangedArg);
        }

        private static void SetValueInternal<TValue, TType>(NDReadOnlyProperty<TValue, TType> property, TType obj, TValue value, OnChangedArg<TValue> onChangedArg)
        {
            if (!onChangedArg.Reject)
            {
                if (value == null && property.NullTreatment == NullTreatment.RemoveLocalValue)
                    Lookup<TValue, TType>.Property.Remove((obj, property));
                else
                    Lookup<TValue, TType>.Property[(obj, property)] = value;

                if (!Equals(onChangedArg.OldValue, onChangedArg.NewValue))
                {
                    FireValueChanged(property, obj, obj, onChangedArg.OldValue, onChangedArg.NewValue);
                    if (property.Inherited)
                    {
                        var tree = Tree.GetTree(obj);
                        var queue = new Queue<Tree>();
                        queue.Enqueue(tree);
                        while (queue.Count != 0)
                        {
                            tree = queue.Dequeue();
                            if (tree.Current is TType t)
                                FireValueChanged(property, t, obj, onChangedArg.OldValue, onChangedArg.NewValue);
                            foreach (var child in tree.Childrean)
                                queue.Enqueue(child);
                        }
                    }
                }
            }
        }

        internal static void FireValueChanged<TValue, TType>(NDReadOnlyProperty<TValue, TType> property, TType objectOfValueChange, object sender, TValue oldValue, TValue newValue)
        {
            if (Lookup<TValue, TType>.Handler.ContainsKey((objectOfValueChange, property)))
            {
                var list = Lookup<TValue, TType>.Handler[(objectOfValueChange, property)];
                foreach (var handler in list)
                    handler(sender, ChangedEventArgs.Create(objectOfValueChange, oldValue, newValue));
            }
            FireValueForAllChanged(property, objectOfValueChange, sender, oldValue, newValue);
        }
        private static void FireValueForAllChanged<TValue, TType>(NDReadOnlyProperty<TValue, TType> property, TType objectOfValueChange, object sender, TValue oldValue, TValue newValue)
        {
            if (!Lookup<TValue, TType>.PropertyHandler.ContainsKey(property))
                return;
            var list = Lookup<TValue, TType>.PropertyHandler[property];
            foreach (var handler in list)
                handler(sender, ChangedEventArgs.Create(objectOfValueChange, oldValue, newValue));
        }

        public static TValue GetLocalValue<TValue, TType>(NDReadOnlyProperty<TValue, TType> property, TType obj)
        {
            if (HasLocalValue(property, obj))
                return Lookup<TValue, TType>.Property[(obj, property)];
            return default(TValue);
        }
        public static bool HasLocalValue<TValue, TType>(NDReadOnlyProperty<TValue, TType> property, TType obj)
        {
            return Lookup<TValue, TType>.Property.ContainsKey((obj, property));
        }
        public static TValue GetValue<TValue, TType>(NDReadOnlyProperty<TValue, TType> property, TType obj)
        {
            if (HasLocalValue(property, obj))
                return GetLocalValue(property, obj);

            if (property.Inherited)
            {
                // go up the tree
                var tree = Tree.GetTree(obj);
                while (tree.Parent != null)
                {
                    tree = tree.Parent;
                    if (tree.Current is TType instance && HasLocalValue(property, instance))
                        return GetLocalValue(property, instance);
                }
            }

            return default(TValue);
        }

        public static void AddEventHandler<TValue, TType>(NDReadOnlyProperty<TValue, TType> property, TType obj, EventHandler<ChangedEventArgs<TValue, TType>> handler)
        {
            if (!Lookup<TValue, TType>.Handler.ContainsKey((obj, property)))
                Lookup<TValue, TType>.Handler.Add((obj, property), new List<EventHandler<ChangedEventArgs<TValue, TType>>>());
            var list = Lookup<TValue, TType>.Handler[(obj, property)];
            list.Add(handler);
        }
        public static void RemoveEventHandler<TValue, TType>(NDReadOnlyProperty<TValue, TType> property, TType obj, EventHandler<ChangedEventArgs<TValue, TType>> handler)
        {
            if (!Lookup<TValue, TType>.Handler.ContainsKey((obj, property)))
                return;
            var list = Lookup<TValue, TType>.Handler[(obj, property)];
            list.Remove(handler);
            if (list.Count == 0)
                Lookup<TValue, TType>.Handler.Remove((obj, property));
        }
        public static void AddEventHandler<TValue, TType>(NDReadOnlyProperty<TValue, TType> property, EventHandler<ChangedEventArgs<TValue, TType>> handler)
        {
            if (!Lookup<TValue, TType>.PropertyHandler.ContainsKey(property))
                Lookup<TValue, TType>.PropertyHandler.Add(property, new List<EventHandler<ChangedEventArgs<TValue, TType>>>());
            var list = Lookup<TValue, TType>.PropertyHandler[property];
            list.Add(handler);
        }
        public static void RemoveEventHandler<TValue, TType>(NDReadOnlyProperty<TValue, TType> property, EventHandler<ChangedEventArgs<TValue, TType>> handler)
        {
            if (!Lookup<TValue, TType>.PropertyHandler.ContainsKey(property))
                return;
            var list = Lookup<TValue, TType>.PropertyHandler[property];
            list.Remove(handler);
            if (list.Count == 0)
                Lookup<TValue, TType>.PropertyHandler.Remove(property);
        }

        private static class Lookup<TValue, TType>
        {
            public readonly static System.Collections.Generic.Dictionary<(TType, NDReadOnlyProperty<TValue, TType>), TValue> Property = new Dictionary<(TType, NDReadOnlyProperty<TValue, TType>), TValue>();
            public readonly static System.Collections.Generic.Dictionary<(TType, NDReadOnlyProperty<TValue, TType>), List<EventHandler<ChangedEventArgs<TValue, TType>>>> Handler = new Dictionary<(TType, NDReadOnlyProperty<TValue, TType>), List<EventHandler<ChangedEventArgs<TValue, TType>>>>();
            public readonly static System.Collections.Generic.Dictionary<NDReadOnlyProperty<TValue, TType>, List<EventHandler<ChangedEventArgs<TValue, TType>>>> PropertyHandler = new Dictionary<NDReadOnlyProperty<TValue, TType>, List<EventHandler<ChangedEventArgs<TValue, TType>>>>();
        }


        internal class Tree
        {
            private static readonly Dictionary<object, Tree> treeLookup = new Dictionary<object, Tree>();

            public static Tree GetTree(object sender)
            {
                if (sender == null)
                    return null;
                if (!treeLookup.ContainsKey(sender))
                    treeLookup.Add(sender, new Tree(sender));
                var tree = treeLookup[sender];
                return tree;
            }
            private Tree(object sender)
            {
                this.Current = sender;
            }

            public Tree Parent { get; set; }
            public object Current { get; }
            public List<Tree> Childrean { get; } = new List<Tree>();
        }
    }


    public static class ChangedEventArgs
    {
        public static ChangedEventArgs<TValue, TType> Create<TValue, TType>(TType objectThatChanged, TValue oldValue, TValue newValue) => new ChangedEventArgs<TValue, TType>(objectThatChanged, oldValue, newValue);
    }
    public class ChangedEventArgs<TValue, TType> : EventArgs
    {
        public ChangedEventArgs(TType objectThatChanged, TValue oldValue, TValue newValue)
        {
            ChangedObject = objectThatChanged;
            this.OldValue = oldValue;
            this.NewValue = newValue;
        }

        public TValue OldValue { get; }
        public TValue NewValue { get; }
        public TType ChangedObject { get; }
    }

    public delegate void OnChanged<TValue>(OnChangedArg<TValue> arg);
    public delegate void OnChanged<TValue, TType>(OnChangedArg<TValue, TType> arg);

    public static class OnChangedArg
    {
        public static OnChangedArg<TValue> Create<TValue>(TValue oldValue, TValue newValue) => new OnChangedArg<TValue>(oldValue, newValue);
        public static OnChangedArg<TValue, TType> Create<TValue, TType>(TType changedObject, TValue oldValue, TValue newValue) => new OnChangedArg<TValue, TType>(changedObject, oldValue, newValue);
    }
    public class OnChangedArg<TValue>
    {
        public OnChangedArg(TValue oldValue, TValue newValue)
        {
            this.OldValue = oldValue;
            this.NewValue = newValue;
        }

        public TValue OldValue { get; }
        public TValue NewValue { get; }

        public bool Reject { get; set; }

        public StringResource Error { get; set; }

    }
    public class OnChangedArg<TValue, TType> : OnChangedArg<TValue>
    {
        public OnChangedArg(TType changedObject, TValue oldValue, TValue newValue) : base(oldValue, newValue)
        {
            this.ChangedObject = changedObject;
        }

        public TType ChangedObject { get; }

    }

    public abstract class StringResource
    {
        public static implicit operator string(StringResource resource) => resource.ToString(System.Globalization.CultureInfo.CurrentUICulture);
        public static implicit operator StringResource(string resource) => new SimpleResource(resource);

        protected abstract string ToString(CultureInfo currentUICulture);

        private class SimpleResource : StringResource
        {
            private readonly string str;

            public SimpleResource(string str)
            {
                this.str = str;
            }
            protected override string ToString(CultureInfo currentUICulture) => str;
        }
    }

    public enum NullTreatment
    {
        RemoveLocalValue,
        SetLocalExplicityNull
    }

    public class NDProperty<TValue, TType> : NDReadOnlyProperty<TValue, TType>, INDProperty<TValue, TType>
    {
        internal readonly Func<TType, OnChanged<TValue>> changedMethod;

        public NDReadOnlyProperty<TValue, TType> ReadOnlyProperty { get; }
        NDReadOnlyProperty<TValue, TType> INDProperty<TValue, TType>.ReadOnlyProperty => ReadOnlyProperty;

        internal NDProperty(Func<TType, OnChanged<TValue>> changedMethod, bool inherited, NullTreatment nullTreatment) : base(inherited, nullTreatment)
        {
            ReadOnlyProperty = new NDReadOnlyProperty<TValue, TType>(inherited, nullTreatment);
            this.changedMethod = changedMethod;
        }
    }
    public class NDAttachedProperty<TValue, TType> : NDReadOnlyProperty<TValue, TType>, INDProperty<TValue, TType>
    {
        internal readonly OnChanged<TValue, TType> changedMethod;

        internal NDReadOnlyProperty<TValue, TType> ReadOnlyProperty { get; }
        NDReadOnlyProperty<TValue, TType> INDProperty<TValue, TType>.ReadOnlyProperty => ReadOnlyProperty;

        internal NDAttachedProperty(OnChanged<TValue, TType> changedMethod, bool inherited, NullTreatment nullTreatment) : base(inherited, nullTreatment)
        {
            ReadOnlyProperty = new NDReadOnlyProperty<TValue, TType>(inherited, nullTreatment);
            this.changedMethod = changedMethod;
        }

    }

    public class NDReadOnlyProperty<TValue, TType> : INDReadOnlyProperty
    {
        public bool Inherited { get; }

        public NullTreatment NullTreatment { get; }

        internal NDReadOnlyProperty(bool inherited, NullTreatment nullTreatment)
        {
            Inherited = inherited;
            NullTreatment = nullTreatment;
        }

        public bool Equals(NDReadOnlyProperty<TValue, TType> obj)
        {
            var other = GetReadonly(obj);
            var me = GetReadonly(this);
            if (ReferenceEquals(me, this) && ReferenceEquals(obj, other))
                return ReferenceEquals(me, other);
            return me.Equals(other);
        }
        public override bool Equals(object obj)
        {
            if (obj is NDReadOnlyProperty<TValue, TType> p)
                return Equals(p);
            return false;
        }

        public override int GetHashCode()
        {
            var me = GetReadonly(this);
            if (ReferenceEquals(me, this))
                return base.GetHashCode();
            return me.GetHashCode();
        }

        private static NDReadOnlyProperty<TValue, TType> GetReadonly(NDReadOnlyProperty<TValue, TType> r)
        {
            if (r is INDProperty<TValue, TType> p)
                return p.ReadOnlyProperty;
            return r;
        }

        object INDReadOnlyProperty.GetValue(object obj)
        {
            if (obj is TType t)
                return PropertyRegistar.GetValue(this, t);
            throw new ArgumentException($"Parameter was not of Type {typeof(TType).FullName}");
        }
        object INDReadOnlyProperty.GetLocalValue(object obj)
        {
            if (obj is TType t)
                return PropertyRegistar.GetValue(this, t);
            throw new ArgumentException($"Parameter was not of Type {typeof(TType).FullName}");
        }

        bool INDReadOnlyProperty.HasLocalValue(object obj)
        {
            if (obj is TType t)
                return PropertyRegistar.HasLocalValue(this, t);
            throw new ArgumentException($"Parameter was not of Type {typeof(TType).FullName}");
        }

        void INDReadOnlyProperty.CallChangeHandler(object objectToChange, object sender, object oldValue, object newValue)
        {
            if (!(objectToChange is TType))
                throw new ArgumentException($"Parameter was not of Type {typeof(TType).FullName}", nameof(objectToChange));
            TValue nv;
            TValue ov;
            if (newValue != null)
            {
                if (newValue is TValue)
                    nv = (TValue)newValue;
                else
                    throw new ArgumentException($"Parameter was not of Type {typeof(TValue).FullName}", nameof(newValue));
            }
            else
                nv = (TValue)newValue;

            if (oldValue != null)
            {
                if (oldValue is TValue)
                    ov = (TValue)oldValue;
                else
                    throw new ArgumentException($"Parameter was not of Type {typeof(TValue).FullName}", nameof(oldValue));
            }
            else
                ov = (TValue)oldValue;


            if (objectToChange is TType t)
                PropertyRegistar.FireValueChanged(this, t, sender, ov, nv);


        }
    }

    internal interface INDReadOnlyProperty
    {

        object GetValue(object obj);
        object GetLocalValue(object obj);
        bool HasLocalValue(object obj);
        void CallChangeHandler(object obj, object sender, object oldValue, object newValue);
    }


}
