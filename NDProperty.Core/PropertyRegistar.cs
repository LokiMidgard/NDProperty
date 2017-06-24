using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NDProperty
{
    public static partial class PropertyRegistar
    {

        private static readonly Dictionary<Type, List<IInternalNDReadOnlyProperty>> inheritedPropertys = new Dictionary<Type, List<IInternalNDReadOnlyProperty>>();

        public static NDProperty<TValue, TType> Register<TValue, TType>(Func<TType, OnChanged<TValue>> changedMethod, TValue defaultValue, NullTreatment nullTreatment, NDPropertySettings settigns)
            where TType : class
        {
            var p = new NDProperty<TValue, TType>(changedMethod, nullTreatment, defaultValue, settigns);
            if (p.Inherited)
            {
                if (!inheritedPropertys.ContainsKey(typeof(TType)))
                    inheritedPropertys.Add(typeof(TType), new List<IInternalNDReadOnlyProperty>());
                inheritedPropertys[typeof(TType)].Add(p);
            }
            if (settigns.HasFlag(NDPropertySettings.ParentReference))
                AddParentHandler(p);

            return p;
        }
        public static NDAttachedProperty<TValue, TType> RegisterAttached<TValue, TType>(OnChanged<TValue, TType> changedMethod, TValue defaultValue,  NullTreatment nullTreatment, NDPropertySettings settigns)
            where TType : class
        {
            var p = new NDAttachedProperty<TValue, TType>(changedMethod, nullTreatment, defaultValue, settigns);
            if (p.Inherited)
            {
                if (!inheritedPropertys.ContainsKey(typeof(TType)))
                    inheritedPropertys.Add(typeof(TType), new List<IInternalNDReadOnlyProperty>());
                inheritedPropertys[typeof(TType)].Add(p);
            }
            if (settigns.HasFlag(NDPropertySettings.ParentReference))
                    AddParentHandler(p);

            return p;
        }

        private static void AddParentHandler<TValue, TType>(NDReadOnlyProperty<TValue, TType> p) where TType : class
        {
            AddEventHandler(p, (sender, e) =>
            {
                var tree = Tree.GetTree(e.ChangedObject);
                var affectedItems = new List<(object affectedObject, object oldValue, IInternalNDReadOnlyProperty affectedProperty)>();
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
        }

        public static void SetValue<TValue, TType>(NDProperty<TValue, TType> property, TType changedObject, TValue value) where TType : class
        {
            var oldValue = GetValue(property, changedObject);
            if (!property.Settigns.HasFlag(NDPropertySettings.CallOnChangedHandlerOnEquals) && Object.Equals(oldValue, value))
                return;
            var onChangedArg = OnChangedArg.Create(oldValue, value);
            property.changedMethod(changedObject)(onChangedArg);
            SetValueInternal(property, changedObject, onChangedArg);
        }
        public static void SetValue<TValue, TType>(NDAttachedProperty<TValue, TType> property, TType changedObject, TValue value) where TType : class
        {
            var oldValue = GetValue(property, changedObject);
            if (!property.Settigns.HasFlag(NDPropertySettings.CallOnChangedHandlerOnEquals) && Object.Equals(oldValue, value))
                return;
            var onChangedArg = OnChangedArg.Create(changedObject, oldValue, value);
            property.changedMethod(onChangedArg);
            SetValueInternal(property, changedObject, onChangedArg);
        }

        private static void SetValueInternal<TValue, TType>(NDReadOnlyProperty<TValue, TType> property, TType obj, OnChangedArg<TValue> onChangedArg) where TType : class
        {
            var value = onChangedArg.MutatedValue;
            if (!onChangedArg.Reject)
            {
                if (value == null && property.NullTreatment == NullTreatment.RemoveLocalValue)
                    Lookup<TValue, TType>.Property.Remove((obj, property));
                else
                    Lookup<TValue, TType>.Property[(obj, property)] = value;

                if (!Equals(onChangedArg.OldValue, onChangedArg.NewValue))
                {
                    FireValueChanged(property, obj, obj, onChangedArg.OldValue, value);
                    if (property.Inherited)
                    {
                        var tree = Tree.GetTree(obj);
                        var queue = new Queue<Tree>();
                        queue.Enqueue(tree);
                        while (queue.Count != 0)
                        {
                            tree = queue.Dequeue();
                            if (tree.Current is TType t)
                                FireValueChanged(property, t, obj, onChangedArg.OldValue, value);
                            foreach (var child in tree.Childrean)
                                queue.Enqueue(child);
                        }
                    }
                }
                onChangedArg.FireExecuteAfterChange();
            }
        }

        internal static void FireValueChanged<TValue, TType>(NDReadOnlyProperty<TValue, TType> property, TType objectOfValueChange, object sender, TValue oldValue, TValue newValue) where TType : class
        {
            if (Lookup<TValue, TType>.Handler.ContainsKey((objectOfValueChange, property)))
            {
                var list = Lookup<TValue, TType>.Handler[(objectOfValueChange, property)];
                foreach (var handler in list)
                    handler(sender, ChangedEventArgs.Create(objectOfValueChange, oldValue, newValue));
            }
            FireValueForAllChanged(property, objectOfValueChange, sender, oldValue, newValue);
        }
        private static void FireValueForAllChanged<TValue, TType>(NDReadOnlyProperty<TValue, TType> property, TType objectOfValueChange, object sender, TValue oldValue, TValue newValue) where TType : class
        {
            if (!Lookup<TValue, TType>.PropertyHandler.ContainsKey(property))
                return;
            var list = Lookup<TValue, TType>.PropertyHandler[property];
            foreach (var handler in list)
                handler(sender, ChangedEventArgs.Create(objectOfValueChange, oldValue, newValue));
        }

        public static TValue GetLocalValue<TValue, TType>(NDReadOnlyProperty<TValue, TType> property, TType obj) where TType : class
        {
            if (HasLocalValue(property, obj))
                return Lookup<TValue, TType>.Property[(obj, property)];
            return property.DefaultValue;
        }
        public static bool HasLocalValue<TValue, TType>(NDReadOnlyProperty<TValue, TType> property, TType obj) where TType : class
        {
            return Lookup<TValue, TType>.Property.ContainsKey((obj, property));
        }
        public static TValue GetValue<TValue, TType>(NDReadOnlyProperty<TValue, TType> property, TType obj) where TType : class
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

            return property.DefaultValue;
        }

        public static void AddEventHandler<TValue, TType>(NDReadOnlyProperty<TValue, TType> property, TType obj, EventHandler<ChangedEventArgs<TValue, TType>> handler) where TType : class
        {
            if (!Lookup<TValue, TType>.Handler.ContainsKey((obj, property)))
                Lookup<TValue, TType>.Handler.Add((obj, property), new List<EventHandler<ChangedEventArgs<TValue, TType>>>());
            var list = Lookup<TValue, TType>.Handler[(obj, property)];
            list.Add(handler);
        }
        public static void RemoveEventHandler<TValue, TType>(NDReadOnlyProperty<TValue, TType> property, TType obj, EventHandler<ChangedEventArgs<TValue, TType>> handler) where TType : class
        {
            if (!Lookup<TValue, TType>.Handler.ContainsKey((obj, property)))
                return;
            var list = Lookup<TValue, TType>.Handler[(obj, property)];
            list.Remove(handler);
            if (list.Count == 0)
                Lookup<TValue, TType>.Handler.Remove((obj, property));
        }
        public static void AddEventHandler<TValue, TType>(NDReadOnlyProperty<TValue, TType> property, EventHandler<ChangedEventArgs<TValue, TType>> handler) where TType : class
        {
            if (!Lookup<TValue, TType>.PropertyHandler.ContainsKey(property))
                Lookup<TValue, TType>.PropertyHandler.Add(property, new List<EventHandler<ChangedEventArgs<TValue, TType>>>());
            var list = Lookup<TValue, TType>.PropertyHandler[property];
            list.Add(handler);
        }
        public static void RemoveEventHandler<TValue, TType>(NDReadOnlyProperty<TValue, TType> property, EventHandler<ChangedEventArgs<TValue, TType>> handler) where TType : class
        {
            if (!Lookup<TValue, TType>.PropertyHandler.ContainsKey(property))
                return;
            var list = Lookup<TValue, TType>.PropertyHandler[property];
            list.Remove(handler);
            if (list.Count == 0)
                Lookup<TValue, TType>.PropertyHandler.Remove(property);
        }

        private static class Lookup<TValue, TType> where TType : class
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

    public delegate void OnChanged<TValue>(OnChangedArg<TValue> arg);
    public delegate void OnChanged<TValue, TType>(OnChangedArg<TValue, TType> arg) where TType : class;


}
