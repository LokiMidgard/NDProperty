using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;

namespace NDProperty
{
    public static class PropertyRegistar
    {

        private static readonly Dictionary<Type, List<INDReadOnlyProperty>> inheritedPropertys = new Dictionary<Type, List<INDReadOnlyProperty>>();
        private static readonly HashSet<(Type, INDReadOnlyProperty)> inheritedPropertysSet = new HashSet<(Type, INDReadOnlyProperty)>();

        public static NDProperty<TValue, TType> Register<TValue, TType>(Func<TType, OnChanged<TValue>> changedMethod, bool inherited, NullTreatment nullTreatment)
        {
            var p = new NDProperty<TValue, TType>(changedMethod, inherited, nullTreatment);
            if (p.Inherited)
            {
                if (!inheritedPropertys.ContainsKey(typeof(TType)))
                    inheritedPropertys.Add(typeof(TType), new List<INDReadOnlyProperty>());
                inheritedPropertys[typeof(TType)].Add(p);
                inheritedPropertysSet.Add((typeof(TType), p));
            }
            return p;
        }

        public static NDProperty<TValue, TType> RegisterParent<TValue, TType>(Func<TType, OnChanged<TValue>> changedMethod)
        {
            var property = Register(changedMethod, false, NullTreatment.RemoveLocalValue);

            AddEventHandler(property, (sender, e) =>
            {
                // TODO: Call ChangedHandler for Inherited Stuff
                var tree = Tree.GetTree(sender);
                System.Diagnostics.Debug.Assert(Equals(tree.Parent.Current, e.OldValue));

                if (tree.Parent != null)
                    tree.Parent.Childrean.Remove(tree);

                tree.Parent = Tree.GetTree(e.NewValue);

                if (tree.Parent != null)
                    tree.Parent.Childrean.Add(tree);
            });
            return property;
        }


        public static void SetValue<TValue, TType>(NDProperty<TValue, TType> property, TType obj, TValue value)
        {
            var oldValue = GetValue(property, obj);
            var onChangedArg = OnChangedArg.Create(oldValue, value);
            property.changedMethod(obj)(onChangedArg);
            if (!onChangedArg.Reject)
            {
                if (value == null && property.NullTreatment == NullTreatment.RemoveLocalValue)
                    Lookup<TValue, TType>.Property.Remove((obj, property));
                else
                    Lookup<TValue, TType>.Property[(obj, property)] = value;

                if (!Equals(onChangedArg.OldValue, onChangedArg.NewValue))
                {
                    FireValueChanged(property, obj, onChangedArg.OldValue, onChangedArg.NewValue);
                    if (property.Inherited)
                    {
                        var tree = Tree.GetTree(obj);
                        var queue = new Queue<Tree>();
                        queue.Enqueue(tree);
                        while (queue.Count != 0)
                        {
                            tree = queue.Dequeue();
                            if (inheritedPropertysSet.Contains((tree.Current.GetType(), property)))
                                FireValueChanged(property, (TType)tree.Current, onChangedArg.OldValue, onChangedArg.NewValue);
                            foreach (var child in tree.Childrean)
                                queue.Enqueue(child);
                        }
                    }
                }
            }
        }

        private static void FireValueChanged<TValue, TType>(NDProperty<TValue, TType> property, TType obj, TValue oldValue, TValue newValue)
        {
            if (!Lookup<TValue, TType>.Handler.ContainsKey((obj, property)))
                return;
            var list = Lookup<TValue, TType>.Handler[(obj, property)];
            foreach (var handler in list)
                handler(obj, ChangedEventArgs.Create(oldValue, newValue));
            FireValueForAllChanged(property, obj, oldValue, newValue);
        }
        private static void FireValueForAllChanged<TValue, TType>(NDProperty<TValue, TType> property, TType obj, TValue oldValue, TValue newValue)
        {
            if (!Lookup<TValue, TType>.PropertyHandler.ContainsKey(property))
                return;
            var list = Lookup<TValue, TType>.PropertyHandler[property];
            foreach (var handler in list)
                handler(obj, ChangedEventArgs.Create(oldValue, newValue));
        }

        public static TValue GetValue<TValue, TType>(NDReadOnlyProperty<TValue, TType> property, TType obj)
        {
            if (Lookup<TValue, TType>.Property.ContainsKey((obj, property)))
                return Lookup<TValue, TType>.Property[(obj, property)];

            if (property.Inherited)
                throw new NotImplementedException();            // TODO: Implement TreeLookup

            return default(TValue);
        }

        public static void AddEventHandler<TValue, TType>(NDReadOnlyProperty<TValue, TType> property, TType obj, EventHandler<ChangedEventArgs<TValue>> handler)
        {
            if (!Lookup<TValue, TType>.Handler.ContainsKey((obj, property)))
                Lookup<TValue, TType>.Handler.Add((obj, property), new List<EventHandler<ChangedEventArgs<TValue>>>());
            var list = Lookup<TValue, TType>.Handler[(obj, property)];
            list.Add(handler);
        }
        public static void RemoveEventHandler<TValue, TType>(NDReadOnlyProperty<TValue, TType> property, TType obj, EventHandler<ChangedEventArgs<TValue>> handler)
        {
            if (!Lookup<TValue, TType>.Handler.ContainsKey((obj, property)))
                return;
            var list = Lookup<TValue, TType>.Handler[(obj, property)];
            list.Remove(handler);
            if (list.Count == 0)
                Lookup<TValue, TType>.Handler.Remove((obj, property));
        }
        public static void AddEventHandler<TValue, TType>(NDReadOnlyProperty<TValue, TType> property, EventHandler<ChangedEventArgs<TValue>> handler)
        {
            if (!Lookup<TValue, TType>.PropertyHandler.ContainsKey(property))
                Lookup<TValue, TType>.PropertyHandler.Add(property, new List<EventHandler<ChangedEventArgs<TValue>>>());
            var list = Lookup<TValue, TType>.PropertyHandler[property];
            list.Add(handler);
        }
        public static void RemoveEventHandler<TValue, TType>(NDReadOnlyProperty<TValue, TType> property, EventHandler<ChangedEventArgs<TValue>> handler)
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
            public readonly static System.Collections.Generic.Dictionary<(TType, NDReadOnlyProperty<TValue, TType>), List<EventHandler<ChangedEventArgs<TValue>>>> Handler = new Dictionary<(TType, NDReadOnlyProperty<TValue, TType>), List<EventHandler<ChangedEventArgs<TValue>>>>();
            public readonly static System.Collections.Generic.Dictionary<NDReadOnlyProperty<TValue, TType>, List<EventHandler<ChangedEventArgs<TValue>>>> PropertyHandler = new Dictionary<NDReadOnlyProperty<TValue, TType>, List<EventHandler<ChangedEventArgs<TValue>>>>();
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
        public static ChangedEventArgs<T> Create<T>(T oldValue, T newValue) => new ChangedEventArgs<T>(oldValue, newValue);
    }
    public class ChangedEventArgs<T> : EventArgs
    {
        public ChangedEventArgs(T oldValue, T newValue)
        {
            this.OldValue = oldValue;
            this.NewValue = newValue;
        }

        public T OldValue { get; }
        public T NewValue { get; }

    }

    public delegate void OnChanged<T>(OnChangedArg<T> arg);

    public static class OnChangedArg
    {
        public static OnChangedArg<T> Create<T>(T oldValue, T newValue) => new OnChangedArg<T>(oldValue, newValue);
    }
    public class OnChangedArg<T>
    {
        public OnChangedArg(T oldValue, T newValue)
        {
            this.OldValue = oldValue;
            this.NewValue = newValue;
        }

        public T OldValue { get; }
        public T NewValue { get; }

        public bool Reject { get; set; }

        public StringResource Error { get; set; }

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

    [System.AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    sealed class NDPAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        readonly string positionalString;

        // This is a positional argument
        public NDPAttribute(string positionalString)
        {
            this.positionalString = positionalString;

            // TODO: Implement code here

            throw new NotImplementedException();
        }

        public string PositionalString
        {
            get { return positionalString; }
        }

        // This is a named argument
        public int NamedInt { get; set; }
    }

    public enum NullTreatment
    {
        RemoveLocalValue,
        SetLocalExplicityNull
    }

    public class NDProperty<TValue, TType> : NDReadOnlyProperty<TValue, TType>
    {
        internal readonly Func<TType, OnChanged<TValue>> changedMethod;


        internal NDProperty(Func<TType, OnChanged<TValue>> changedMethod, bool inherited, NullTreatment nullTreatment) : base(inherited, nullTreatment)
        {
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
    }

    public interface INDReadOnlyProperty { }
}
