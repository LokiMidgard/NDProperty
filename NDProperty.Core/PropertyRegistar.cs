using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NDProperty
{
    public static partial class PropertyRegistar
    {

        private static readonly Dictionary<Type, List<IInternalNDReadOnlyProperty>> inheritedPropertys = new Dictionary<Type, List<IInternalNDReadOnlyProperty>>();

        /// <summary>
        /// Registers a Property on the specific class
        /// </summary>
        /// <typeparam name="TValue">The type of the Property</typeparam>
        /// <typeparam name="TType">The Type of the Object that defines the Property</typeparam>
        /// <param name="changedMethod">The Method that will be called if the property will be set.</param>
        /// <param name="defaultValue">The default Value that the Proeprty will have if no value is set.</param>
        /// <param name="nullTreatment">Defines how to handle Null values.</param>
        /// <param name="settigns">Additional Settings.</param>
        /// <returns>The Property key</returns>
        public static NDPropertyKey<TValue, TType> Register<TValue, TType>(Func<TType, OnChanged<TValue>> changedMethod, TValue defaultValue, NullTreatment nullTreatment, NDPropertySettings settigns)
            where TType : class
        {
            var p = new NDPropertyKey<TValue, TType>(changedMethod, nullTreatment, defaultValue, settigns);
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

        /// <summary>
        /// Registers an attached Proeprty on the specific class.
        /// </summary>
        /// <typeparam name="TValue">The type of the Property</typeparam>
        /// <typeparam name="TType">The Type of the Object that defines the Property</typeparam>
        /// <param name="changedMethod">The Method that will be called if the property will be set.</param>
        /// <param name="defaultValue">The default Value that the Proeprty will have if no value is set.</param>
        /// <param name="nullTreatment">Defines how to handle Null values.</param>
        /// <param name="settigns">Additional Settings.</param>
        /// <returns>The Property key</returns>
        public static NDAttachedPropertyKey<TValue, TType> RegisterAttached<TValue, TType>(OnChanged<TValue, TType> changedMethod, TValue defaultValue, NullTreatment nullTreatment, NDPropertySettings settigns)
            where TType : class
        {
            var p = new NDAttachedPropertyKey<TValue, TType>(changedMethod, nullTreatment, defaultValue, settigns);
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

        private static void AddParentHandler<TValue, TType>(NDReadOnlyPropertyKey<TValue, TType> p) where TType : class
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

        /// <summary>
        /// Sets a value on the Property
        /// </summary>
        /// <typeparam name="TValue">The Type of the Property</typeparam>
        /// <typeparam name="TType">The Type on which the Property is defined</typeparam>
        /// <param name="property">The Property that will be changed</param>
        /// <param name="changedObject">The Object on which the property will be changed</param>
        /// <param name="value">The new Value of the Property.</param>
        /// <returns><c>false</c> if the operation was rejected</returns>
        /// <remarks>
        /// if <see cref="NDPropertySettings.CallOnChangedHandlerOnEquals"/> is not set and the <paramref name="value"/> equals the current value, this method returns <c>true</c>.
        /// </remarks>
        public static bool SetValue<TValue, TType>(NDPropertyKey<TValue, TType> property, TType changedObject, TValue value) where TType : class
        {
            var oldValue = GetValue(property, changedObject);
            if (!property.Settigns.HasFlag(NDPropertySettings.CallOnChangedHandlerOnEquals) && Object.Equals(oldValue, value))
                return true;
            var onChangedArg = OnChangedArg.Create(oldValue, value);
            property.changedMethod(changedObject)(onChangedArg);
            return SetValueInternal(property, changedObject, onChangedArg);
        }

        /// <summary>
        /// Sets a value on the Property
        /// </summary>
        /// <typeparam name="TValue">The Type of the Property</typeparam>
        /// <typeparam name="TType">The Type on which the Property is defined</typeparam>
        /// <param name="property">The Property that will be changed</param>
        /// <param name="changedObject">The Object on which the property will be changed</param>
        /// <param name="value">The new Value of the Property.</param>
        /// <returns><c>false</c> if the operation was rejected</returns>
        /// <remarks>
        /// if <see cref="NDPropertySettings.CallOnChangedHandlerOnEquals"/> is not set and the <paramref name="value"/> equals the current value, this method returns <c>true</c>.
        /// </remarks>
        public static bool SetValue<TValue, TType>(NDAttachedPropertyKey<TValue, TType> property, TType changedObject, TValue value) where TType : class
        {
            var oldValue = GetValue(property, changedObject);
            if (!property.Settigns.HasFlag(NDPropertySettings.CallOnChangedHandlerOnEquals) && Object.Equals(oldValue, value))
                return true;
            var onChangedArg = OnChangedArg.Create(changedObject, oldValue, value);
            property.changedMethod(onChangedArg);
            return SetValueInternal(property, changedObject, onChangedArg);

        }

        private static bool SetValueInternal<TValue, TType>(NDReadOnlyPropertyKey<TValue, TType> property, TType obj, OnChangedArg<TValue> onChangedArg) where TType : class
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
                return true;
            }
            else
                return false;
        }

        internal static void FireValueChanged<TValue, TType>(NDReadOnlyPropertyKey<TValue, TType> property, TType objectOfValueChange, object sender, TValue oldValue, TValue newValue) where TType : class
        {
            if (Lookup<TValue, TType>.Handler.ContainsKey((objectOfValueChange, property)))
            {
                var list = Lookup<TValue, TType>.Handler[(objectOfValueChange, property)];
                foreach (var handler in list)
                    handler(sender, ChangedEventArgs.Create(objectOfValueChange, oldValue, newValue));
            }
            FireValueForAllChanged(property, objectOfValueChange, sender, oldValue, newValue);
        }
        private static void FireValueForAllChanged<TValue, TType>(NDReadOnlyPropertyKey<TValue, TType> property, TType objectOfValueChange, object sender, TValue oldValue, TValue newValue) where TType : class
        {
            if (!Lookup<TValue, TType>.PropertyHandler.ContainsKey(property))
                return;
            var list = Lookup<TValue, TType>.PropertyHandler[property];
            foreach (var handler in list)
                handler(sender, ChangedEventArgs.Create(objectOfValueChange, oldValue, newValue));
        }

        /// <summary>
        /// Getting the localy set Value on the Object ignoring inheritance and everything else.
        /// </summary>
        /// <typeparam name="TValue">The type of the Property</typeparam>
        /// <typeparam name="TType">The Type of the Object that defines the Property</typeparam>
        /// <param name="property">The property which value to retrive</param>
        /// <param name="obj">The object that holds the value</param>
        /// <returns>The Local value or the in the Property defined default value if no value was set on this Object.</returns>
        public static TValue GetLocalValue<TValue, TType>(NDReadOnlyPropertyKey<TValue, TType> property, TType obj) where TType : class
        {
            if (HasLocalValue(property, obj))
                return Lookup<TValue, TType>.Property[(obj, property)];
            return property.DefaultValue;
        }

        /// <summary>
        /// Removes the local Value on the Object.
        /// </summary>
        /// <typeparam name="TValue">The type of the Property</typeparam>
        /// <typeparam name="TType">The Type of the Object that defines the Property</typeparam>
        /// <param name="property">The Property whiches value will be removed.</param>
        /// <param name="obj">The Object on which the value will be removed.</param>
        /// <returns><c>true</c> if there was a value to remove.</returns>
        /// <remarks>
        /// If this property supports inheritence it will now inherre the value from it parent againe. <para/>
        /// If the <see cref="NullTreatment.RemoveLocalValue"/> is set, assigneing null to the property will resolve in the same as calling this Method.
        /// </remarks>
        public static bool RemoveLocalValue<TValue, TType>(NDReadOnlyPropertyKey<TValue, TType> property, TType obj) where TType : class
        {
            if (HasLocalValue(property, obj))
                return Lookup<TValue, TType>.Property.Remove((obj, property));
            return false;
        }

        /// <summary>
        /// Returns if a local Value is set on this Property.
        /// </summary>
        /// <typeparam name="TValue">The type of the Property</typeparam>
        /// <typeparam name="TType">The Type of the Object that defines the Property</typeparam>
        /// <param name="property">The Property to check</param>
        /// <param name="obj">The Object to check</param>
        /// <returns><c>true</c> if the value is set on the object and is not calculated from another source.</returns>
        public static bool HasLocalValue<TValue, TType>(NDReadOnlyPropertyKey<TValue, TType> property, TType obj) where TType : class
        {
            return Lookup<TValue, TType>.Property.ContainsKey((obj, property));
        }

        /// <summary>
        /// Returns the Value of the Property
        /// </summary>
        /// <typeparam name="TValue">The type of the Property</typeparam>
        /// <typeparam name="TType">The Type of the Object that defines the Property</typeparam>
        /// <param name="property">The Property to obtaine the value from</param>
        /// <param name="obj">The Object to obtaine the value from</param>
        /// <returns>The value</returns>
        public static TValue GetValue<TValue, TType>(NDReadOnlyPropertyKey<TValue, TType> property, TType obj) where TType : class
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

        /// <summary>
        /// Adds an event handler to the specific Property on the Specific object.
        /// </summary>
        /// <typeparam name="TValue">The type of the Property</typeparam>
        /// <typeparam name="TType">The Type of the Object that defines the Property</typeparam>
        /// <param name="property">The Property whichthe Eventhandler will be watch.</param>
        /// <param name="obj">The object the Eventhandler will be watch.</param>
        /// <param name="handler">The handler that will be called if the Property changes.</param>
        public static void AddEventHandler<TValue, TType>(NDReadOnlyPropertyKey<TValue, TType> property, TType obj, EventHandler<ChangedEventArgs<TValue, TType>> handler) where TType : class
        {
            if (!Lookup<TValue, TType>.Handler.ContainsKey((obj, property)))
                Lookup<TValue, TType>.Handler.Add((obj, property), new List<EventHandler<ChangedEventArgs<TValue, TType>>>());
            var list = Lookup<TValue, TType>.Handler[(obj, property)];
            list.Add(handler);
        }
        /// <summary>
        /// Removes an event handler from the specific Property on the Specific object.
        /// </summary>
        /// <typeparam name="TValue">The type of the Property</typeparam>
        /// <typeparam name="TType">The Type of the Object that defines the Property</typeparam>
        /// <param name="property">The Property whichthe Eventhandler will be removed.</param>
        /// <param name="obj">The object the Eventhandler will be removed.</param>
        /// <param name="handler">The handler that should no longer be called if the Property changes.</param>
        public static void RemoveEventHandler<TValue, TType>(NDReadOnlyPropertyKey<TValue, TType> property, TType obj, EventHandler<ChangedEventArgs<TValue, TType>> handler) where TType : class
        {
            if (!Lookup<TValue, TType>.Handler.ContainsKey((obj, property)))
                return;
            var list = Lookup<TValue, TType>.Handler[(obj, property)];
            list.Remove(handler);
            if (list.Count == 0)
                Lookup<TValue, TType>.Handler.Remove((obj, property));
        }

        /// <summary>
        /// Adds an event handler that will be called if the specific Property on any Object will be changed.
        /// </summary>
        /// <typeparam name="TValue">The type of the Property</typeparam>
        /// <typeparam name="TType">The Type of the Object that defines the Property</typeparam>
        /// <param name="property">The Property that will be watched.</param>
        /// <param name="handler">The handler that will be called if a Property Changes.</param>
        public static void AddEventHandler<TValue, TType>(NDReadOnlyPropertyKey<TValue, TType> property, EventHandler<ChangedEventArgs<TValue, TType>> handler) where TType : class
        {
            if (!Lookup<TValue, TType>.PropertyHandler.ContainsKey(property))
                Lookup<TValue, TType>.PropertyHandler.Add(property, new List<EventHandler<ChangedEventArgs<TValue, TType>>>());
            var list = Lookup<TValue, TType>.PropertyHandler[property];
            list.Add(handler);
        }

        /// <summary>
        /// Removes an event handler that was called if the specific Property on any Object will be changed.
        /// </summary>
        /// <typeparam name="TValue">The type of the Property</typeparam>
        /// <typeparam name="TType">The Type of the Object that defines the Property</typeparam>
        /// <param name="property">The Property that will no longer be watched.</param>
        /// <param name="handler">The handler that will no longer be called if a Property Changes.</param>
        public static void RemoveEventHandler<TValue, TType>(NDReadOnlyPropertyKey<TValue, TType> property, EventHandler<ChangedEventArgs<TValue, TType>> handler) where TType : class
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
            public readonly static System.Collections.Generic.Dictionary<(TType, NDReadOnlyPropertyKey<TValue, TType>), TValue> Property = new Dictionary<(TType, NDReadOnlyPropertyKey<TValue, TType>), TValue>();
            public readonly static System.Collections.Generic.Dictionary<(TType, NDReadOnlyPropertyKey<TValue, TType>), List<EventHandler<ChangedEventArgs<TValue, TType>>>> Handler = new Dictionary<(TType, NDReadOnlyPropertyKey<TValue, TType>), List<EventHandler<ChangedEventArgs<TValue, TType>>>>();
            public readonly static System.Collections.Generic.Dictionary<NDReadOnlyPropertyKey<TValue, TType>, List<EventHandler<ChangedEventArgs<TValue, TType>>>> PropertyHandler = new Dictionary<NDReadOnlyPropertyKey<TValue, TType>, List<EventHandler<ChangedEventArgs<TValue, TType>>>>();
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
