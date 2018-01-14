using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NDProperty.Propertys;
using NDProperty.Providers;

namespace NDProperty
{
    public static partial class PropertyRegistar<TKey>
    {
        public static bool IsInitilized { get; private set; }
        public static IReadOnlyList<ValueProvider<TKey>> ValueProviders
        {
            get
            {
                if (manager == null)
                    Initilize();
                return manager;
            }
        }

        public static Dictionary<ValueProvider<TKey>, int> ProviderOrder
        {
            get
            {
                if (managerOrder == null)
                    Initilize();
                return managerOrder;
            }
        }


        private static void Initilize()
        {
            if (IsInitilized)
                return;
            IsInitilized = true;

            IEnumerable<Providers.ValueProvider<TKey>> valueProvider;

            if (typeof(IInitilizer<TKey>).GetTypeInfo().IsAssignableFrom(typeof(TKey).GetTypeInfo()))
            {

                if (typeof(TKey).GetConstructor(Type.EmptyTypes) == null)
                    throw new ArgumentException($"If TypeParameter is of type {nameof(IInitilizer<TKey>)}, then it must have a default constructor", nameof(TKey));

                var initilizer = typeof(TKey).GetConstructor(Type.EmptyTypes).Invoke(new object[0]) as IInitilizer<TKey>;
                valueProvider = initilizer.ValueProvider;
            }
            else
                valueProvider = new ValueProvider<TKey>[] { LocalValueProvider<TKey>.Instance, InheritenceValueProvider<TKey>.Instance, DefaultValueProvider<TKey>.Instance };



            manager = valueProvider.ToList();
            managerOrder = valueProvider.Select((manager, index) => new { Key = manager, Value = index }).ToDictionary(element => element.Key, element => element.Value);


        }

        private static readonly Dictionary<Type, List<IInternalNDProperty<TKey>>> inheritedPropertys = new Dictionary<Type, List<IInternalNDProperty<TKey>>>();
        private static IReadOnlyList<ValueProvider<TKey>> manager;
        private static Dictionary<ValueProvider<TKey>, int> managerOrder;

        /// <summary>
        /// Registers a Property on the specific class
        /// </summary>
        /// <typeparam name="TValue">The type of the Property</typeparam>
        /// <typeparam name="TType">The Type of the Object that defines the Property</typeparam>
        /// <param name="changingMethod">The Method that will be called if the property will be set.</param>
        /// <param name="defaultValue">The default Value that the Proeprty will have if no value is set.</param>
        /// <param name="nullTreatment">Defines how to handle Null values.</param>
        /// <param name="settigns">Additional Settings.</param>
        /// <returns>The Property key</returns>
        public static NDPropertyKey<TKey, TType, TValue> Register<TType, TValue>(Func<TType, OnChanging<TKey, TValue>> changingMethod, TValue defaultValue, NDPropertySettings settigns)
            where TType : class
        {
            var p = new NDPropertyKey<TKey, TType, TValue>(changingMethod, defaultValue, settigns);
            if (p.Inherited)
            {
                if (!inheritedPropertys.ContainsKey(typeof(TType)))
                    inheritedPropertys.Add(typeof(TType), new List<IInternalNDProperty<TKey>>());
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
        /// <param name="changingMethod">The Method that will be called if the property will be set.</param>
        /// <param name="defaultValue">The default Value that the Proeprty will have if no value is set.</param>
        /// <param name="nullTreatment">Defines how to handle Null values.</param>
        /// <param name="settigns">Additional Settings.</param>
        /// <returns>The Property key</returns>
        public static NDAttachedPropertyKey<TKey, TType, TValue> RegisterAttached<TType, TValue>(OnChanging<TKey, TType, TValue> changingMethod, TValue defaultValue, NDPropertySettings settigns)
            where TType : class
        {
            var p = new NDAttachedPropertyKey<TKey, TType, TValue>(changingMethod, defaultValue, settigns);
            if (p.Inherited)
            {
                if (!inheritedPropertys.ContainsKey(typeof(TType)))
                    inheritedPropertys.Add(typeof(TType), new List<IInternalNDProperty<TKey>>());
                inheritedPropertys[typeof(TType)].Add(p);
            }
            if (settigns.HasFlag(NDPropertySettings.ParentReference))
                AddParentHandler(p);

            return p;
        }

        //public static void SetProvider<TType, TValue>(Providers.ValueProvider<TValue> provider, TType targetObject, NDPropertyKey<TType, TValue> property)
        //    where TType : class
        //{
        //    if (Lookup<TType, TValue>.ProviderListener.ContainsKey((provider.Manager, property, targetObject)))
        //    {
        //        var oldListener = Lookup<TType, TValue>.ProviderListener[(provider.Manager, property, targetObject)];
        //        provider.Manager.GetProvider(targetObject, property).ValueWillChange -= (EventHandler<IValueWillChangeEventArgs<TValue>>)(object)oldListener;
        //    }
        //    EventHandler<Providers.IValueWillChangeEventArgs<TValue>> listener = (sender, e) =>
        //    {
        //        var (beforeLocalProviders, providersAfterLocal) = GetProviders(property, targetObject);
        //        bool fondInBefore = false;
        //        for (int i = 0; i < beforeLocalProviders.Count; i++)
        //        {
        //            if (beforeLocalProviders[i] == beforeLocalProviders)
        //            {
        //                fondInBefore = true;
        //                break; // No provider with higher precedence has a value
        //            }
        //            if (beforeLocalProviders[i].HasValue)
        //                return; // a Provider with higher precedence has a value
        //        }

        //        if (!fondInBefore)
        //        {
        //            if (HasLocalValue(property, targetObject))
        //                return; // We have a local set value. That has higher precedence. 
        //            for (int i = 0; i < providersAfterLocal.Count; i++)
        //            {
        //                if (providersAfterLocal[i] == beforeLocalProviders)
        //                {
        //                    fondInBefore = true;
        //                    break; // No provider with higher precedence has a value
        //                }
        //                if (providersAfterLocal[i].HasValue)
        //                    return; // a Provider with higher precedence has a value
        //            }
        //        }
        //        var oldValue = GetValue(property, targetObject);

        //        if (Equals(oldValue, e.NewValue))
        //            return; // No need to inform someone

        //        e.AfterChange += () =>
        //        {
        //            FireValueChanged(property, targetObject, provider, oldValue, provider.CurrentValue);
        //        };
        //    };
        //    Lookup<TType, TValue>.ProviderListener[(provider.Manager, property, targetObject)] = listener;

        //    provider.ValueWillChange += listener;
        //    provider.Manager.SetProvider(provider, targetObject, property);
        //}


        private static void AddParentHandler<TType, TValue>(NDReadOnlyPropertyKey<TKey, TType, TValue> p) where TType : class
        {
            AddEventHandler(p, (sender, e) =>
            {

                var tree = Tree.GetTree(e.ChangedObject);
                var removedParent = tree.Parent?.Current;
                var affectedItems = new List<(object affectedObject, object oldValue, bool hasOldValue, IInternalNDProperty<TKey> affectedProperty, Type propertyDefinedOn, ValueProvider<TKey> currentProvider, object currentValue)>();
                if (inheritedPropertys.Count != 0)
                {
                    // Get all affacted decendents
                    Queue<Tree> queue = new Queue<Tree>();
                    queue.Enqueue(tree);

                    while (queue.Count != 0)
                    {
                        tree = queue.Dequeue();

                        {
                            foreach (var affectedProperty in inheritedPropertys.Where(x => x.Key.IsAssignableFrom(tree.Current.GetType())).SelectMany(x => x.Value.Select(y => new { Value = y, Key = x.Key })))
                                if (InheritenceValueProvider<TKey>.Instance.IsParantChangeInteresting(tree.Current, affectedProperty.Value, affectedProperty.Key, removedParent))
                                {
                                    var (currentValue, currentProvider) = affectedProperty.Value.GetValueAndProvider(tree.Current);
                                    var (oldValue, hasOldValue) = affectedProperty.Value.GetProviderValue(tree.Current, InheritenceValueProvider<TKey>.Instance);


                                    affectedItems.Add((tree.Current, oldValue, hasOldValue, affectedProperty.Value, affectedProperty.Key, currentProvider, currentValue));
                                }
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
                    var searchedNewValue = InheritenceValueProvider<TKey>.Instance.SearchNewValue(item.affectedObject, item.affectedProperty, item.propertyDefinedOn);
                    if (searchedNewValue.source != null)
                    {
                        (item.affectedProperty).CallSetOmInHeritanceProvider(item.affectedObject, searchedNewValue.source, searchedNewValue.value, true, item.oldValue, item.hasOldValue, item.currentProvider, item.currentValue);
                        //InheritenceValueProvider<TKey>.Instance.SetValue(item.affectedObject, item.affectedProperty, searchedNewValue.source, searchedNewValue.value, true, item.oldValue, item.hasOldValue, item.currentProvider, item.currentValue);
                        //var newValue = item.affectedProperty.GetValue(item.affectedObject);
                        //if (newValue != item.oldValue)
                        //    item.affectedProperty.CallChangeHandler(item.affectedObject, sender, item.oldValue, newValue);
                    }
                }
            });
        }

        ///// <summary>
        ///// Sets a value on the Property
        ///// </summary>
        ///// <typeparam name="TValue">The Type of the Property</typeparam>
        ///// <typeparam name="TType">The Type on which the Property is defined</typeparam>
        ///// <param name="property">The Property that will be changed</param>
        ///// <param name="changingObject">The Object on which the property will be changed</param>
        ///// <param name="value">The new Value of the Property.</param>
        ///// <returns><c>false</c> if the operation was rejected</returns>
        ///// <remarks>
        ///// if <see cref="NDPropertySettings.CallOnChangedHandlerOnEquals"/> is not set and the <paramref name="value"/> equals the current value, this method returns <c>true</c>.
        ///// </remarks>
        //public static bool SetValue<TType, TValue>(NDPropertyKey<TKey, TType, TValue> property, TType changingObject, TValue value) where TType : class
        //{
        //    return LocalValueProvider<TKey>.Instance.SetValue(property, changingObject, value);
        //}

        /// <summary>
        /// Sets a value on the Property
        /// </summary>
        /// <typeparam name="TValue">The Type of the Property</typeparam>
        /// <typeparam name="TType">The Type on which the Property is defined</typeparam>
        /// <param name="property">The Property that will be changed</param>
        /// <param name="changingObject">The Object on which the property will be changed</param>
        /// <param name="value">The new Value of the Property.</param>
        /// <returns><c>false</c> if the operation was rejected</returns>
        /// <remarks>
        /// if <see cref="NDPropertySettings.CallOnChangedHandlerOnEquals"/> is not set and the <paramref name="value"/> equals the current value, this method returns <c>true</c>.
        /// </remarks>
        public static bool SetValue<TType, TValue, TPropertyType>(TPropertyType property, TType changingObject, TValue value)
            where TType : class
            where TPropertyType : NDReadOnlyPropertyKey<TKey, TType, TValue>, INDProperty<TKey, TType, TValue>
        {
            return LocalValueProvider<TKey>.Instance.SetValue(property, changingObject, value);
        }

        /// <summary>
        /// This method is called, when a value of a property should change
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TType"></typeparam>
        /// <param name="property"></param>
        /// <param name="obj"></param>
        /// <param name="onChangedArg"></param>
        /// <returns>true if <paramref name="onChangedArg"/> did not had reject set and the update function returns true.</returns>
        internal static bool ChangeValue<TType, TValue, TPropertyType>(object sender, TPropertyType property, TType obj, OnChangingArg<TKey, TValue> onChangedArg, Func<bool> updateCode)
            where TType : class
            where TPropertyType : NDReadOnlyPropertyKey<TKey, TType, TValue>, INDProperty<TKey, TType, TValue>
        {
            var value = onChangedArg.Provider.MutatedValue;
            if (!onChangedArg.Provider.Reject)
            {

                bool updateSuccsessfull;
                if (property.Inherited)
                {
                    var inheritanceProviderIndex = ProviderOrder[InheritenceValueProvider<TKey>.Instance];

                    var oldValueList = new List<(TType targetObject, ValueProvider<TKey> currentProvider, TValue currentValue, TValue oldValue, bool hasOldValue)>();

                    var tree = Tree.GetTree(obj);
                    var queue = new Queue<Tree>();
                    foreach (var child in tree.Childrean)
                        queue.Enqueue(child);

                    while (queue.Count != 0)
                    {
                        tree = queue.Dequeue();
                        if (tree.Current is TType t) // the childrean of this child will be notified when updating InheritanceValueProvider.
                        {
                            var (currentValue, currentManger) = GetValueAndProvider(property, t);
                            var (oldValue, hasOldValue) = InheritenceValueProvider<TKey>.Instance.GetValue(t, property);
                            oldValueList.Add((t, currentManger, currentValue, oldValue, hasOldValue));
                        }
                        else
                            foreach (var child in tree.Childrean)
                                queue.Enqueue(child);
                    }
                    updateSuccsessfull = updateCode();
                    if (!updateSuccsessfull)
                        return false;
                    foreach (var item in oldValueList)
                        InheritenceValueProvider<TKey>.Instance.SetValue(item.targetObject, property, obj, value, onChangedArg.Provider.HasNewValue, item.oldValue, item.hasOldValue, item.currentProvider, item.currentValue, sender);
                }
                else
                    updateSuccsessfull = updateCode();

                if (!updateSuccsessfull)
                    return false;

                //TValue newActualValue = default;
                //if (onChangedArg.ChangingProvider == onChangedArg.CurrentProvider && !onChangedArg.HasNewValue)
                //{
                //    // the current value was provided by the changing provider but now it will no longer have a value
                //    // we need to find out what the new value will be.
                //    bool found = false;
                //    foreach (var item in PropertyRegistar<TKey>.ValueProviders)
                //    {
                //        var (providerValue, hasValue) = item.GetValue(obj, property);
                //        if (hasValue)
                //        {
                //            found = true;
                //            newActualValue = providerValue;
                //            break;
                //        }
                //    }
                //    if (!found)
                //        throw new InvalidOperationException("No Value Found");
                //}
                //else
                //    newActualValue = value;

                if (onChangedArg.Property.IsObjectValueChanging)
                    FireValueChanged(property, obj, sender, onChangedArg.Property.OldValue, onChangedArg.Property.NewValue);

                onChangedArg.FireExecuteAfterChange(sender);

                var currentProviderIndex = ProviderOrder[onChangedArg.Provider.ChangingProvider];

                for (int i = 0; i < ValueProviders.Count; i++)
                {
                    var providerToNotify = ValueProviders[i];
                    if (i < currentProviderIndex)
                        providerToNotify.LowerProviderUpdated(sender, obj, property, value, onChangedArg.Provider.ChangingProvider);
                    else if (i > currentProviderIndex)
                        providerToNotify.HigherProviderUpdated(sender, obj, property, value, onChangedArg.Provider.ChangingProvider);
                }

                return true;
            }
            else
                return false;
        }


        internal static void FireValueChanged<TType, TValue>(NDReadOnlyPropertyKey<TKey, TType, TValue> property, TType objectOfValueChange, object sender, TValue oldValue, TValue newValue) where TType : class
        {
            if (Lookup<TType, TValue>.Handler.ContainsKey((objectOfValueChange, property)))
            {
                var list = Lookup<TType, TValue>.Handler[(objectOfValueChange, property)];
                foreach (var handler in list)
                    handler(sender, ChangedEventArgs.Create(objectOfValueChange, property, oldValue, newValue));
            }
            FireValueForAllChanged(property, objectOfValueChange, sender, oldValue, newValue);
        }
        private static void FireValueForAllChanged<TType, TValue>(NDReadOnlyPropertyKey<TKey, TType, TValue> property, TType objectOfValueChange, object sender, TValue oldValue, TValue newValue) where TType : class
        {
            if (!Lookup<TType, TValue>.PropertyHandler.ContainsKey(property))
                return;
            var list = Lookup<TType, TValue>.PropertyHandler[property];
            foreach (var handler in list)
                handler(sender, ChangedEventArgs.Create(objectOfValueChange, property, oldValue, newValue));
        }

        /// <summary>
        /// Getting the localy set Value on the Object ignoring inheritance and everything else.
        /// </summary>
        /// <typeparam name="TValue">The type of the Property</typeparam>
        /// <typeparam name="TType">The Type of the Object that defines the Property</typeparam>
        /// <param name="property">The property which value to retrive</param>
        /// <param name="obj">The object that holds the value</param>
        /// <returns>The Local value or the in the Property defined default value if no value was set on this Object.</returns>
        public static TValue GetLocalValue<TType, TValue>(NDReadOnlyPropertyKey<TKey, TType, TValue> property, TType obj) where TType : class
        {
            return LocalValueProvider<TKey>.Instance.GetValue(obj, property).value;
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
        public static bool RemoveLocalValue<TType, TValue, TPropertyType>(TPropertyType property, TType obj) where TType : class
            where TPropertyType : NDReadOnlyPropertyKey<TKey, TType, TValue>, INDProperty<TKey, TType, TValue>
        {
            return LocalValueProvider<TKey>.Instance.RemoveValue<TType, TValue, TPropertyType>(property, obj);
        }

        /// <summary>
        /// Returns if a local Value is set on this Property.
        /// </summary>
        /// <typeparam name="TValue">The type of the Property</typeparam>
        /// <typeparam name="TType">The Type of the Object that defines the Property</typeparam>
        /// <param name="property">The Property to check</param>
        /// <param name="obj">The Object to check</param>
        /// <returns><c>true</c> if the value is set on the object and is not calculated from another source.</returns>
        public static bool HasLocalValue<TType, TValue>(NDReadOnlyPropertyKey<TKey, TType, TValue> property, TType obj) where TType : class
        {
            return LocalValueProvider<TKey>.Instance.GetValue(obj, property).hasValue;
        }

        /// <summary>
        /// Returns the Value of the Property
        /// </summary>
        /// <typeparam name="TValue">The type of the Property</typeparam>
        /// <typeparam name="TType">The Type of the Object that defines the Property</typeparam>
        /// <param name="property">The Property to obtaine the value from</param>
        /// <param name="obj">The Object to obtaine the value from</param>
        /// <returns>The value</returns>
        public static TValue GetValue<TType, TValue>(NDReadOnlyPropertyKey<TKey, TType, TValue> property, TType obj) where TType : class
        {
            return GetValueAndProvider(property, obj).value;
        }
        public static (TValue value, Providers.ValueProvider<TKey> provider) GetValueAndProvider<TType, TValue>(NDReadOnlyPropertyKey<TKey, TType, TValue> property, TType obj) where TType : class
        {

            foreach (var provider in ValueProviders)
            {
                var (providerValue, hasValue) = provider.GetValue(obj, property);
                if (hasValue)
                    return (providerValue, provider);
            }

            throw new NotImplementedException("No Provider provided a Value");
        }

        /// <summary>
        /// Adds an event handler to the specific Property on the Specific object.
        /// </summary>
        /// <typeparam name="TValue">The type of the Property</typeparam>
        /// <typeparam name="TType">The Type of the Object that defines the Property</typeparam>
        /// <param name="property">The Property whichthe Eventhandler will be watch.</param>
        /// <param name="obj">The object the Eventhandler will be watch.</param>
        /// <param name="handler">The handler that will be called if the Property changes.</param>
        public static void AddEventHandler<TType, TValue>(NDReadOnlyPropertyKey<TKey, TType, TValue> property, TType obj, EventHandler<ChangedEventArgs<TKey, TType, TValue>> handler) where TType : class
        {
            if (!Lookup<TType, TValue>.Handler.ContainsKey((obj, property)))
                Lookup<TType, TValue>.Handler.Add((obj, property), new List<EventHandler<ChangedEventArgs<TKey, TType, TValue>>>());
            var list = Lookup<TType, TValue>.Handler[(obj, property)];
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
        public static void RemoveEventHandler<TType, TValue>(NDReadOnlyPropertyKey<TKey, TType, TValue> property, TType obj, EventHandler<ChangedEventArgs<TKey, TType, TValue>> handler) where TType : class
        {
            if (!Lookup<TType, TValue>.Handler.ContainsKey((obj, property)))
                return;
            var list = Lookup<TType, TValue>.Handler[(obj, property)];
            list.Remove(handler);
            if (list.Count == 0)
                Lookup<TType, TValue>.Handler.Remove((obj, property));
        }

        /// <summary>
        /// Adds an event handler that will be called if the specific Property on any Object will be changed.
        /// </summary>
        /// <typeparam name="TValue">The type of the Property</typeparam>
        /// <typeparam name="TType">The Type of the Object that defines the Property</typeparam>
        /// <param name="property">The Property that will be watched.</param>
        /// <param name="handler">The handler that will be called if a Property Changes.</param>
        public static void AddEventHandler<TType, TValue>(NDReadOnlyPropertyKey<TKey, TType, TValue> property, EventHandler<ChangedEventArgs<TKey, TType, TValue>> handler) where TType : class
        {
            if (!Lookup<TType, TValue>.PropertyHandler.ContainsKey(property))
                Lookup<TType, TValue>.PropertyHandler.Add(property, new List<EventHandler<ChangedEventArgs<TKey, TType, TValue>>>());
            var list = Lookup<TType, TValue>.PropertyHandler[property];
            list.Add(handler);
        }

        /// <summary>
        /// Removes an event handler that was called if the specific Property on any Object will be changed.
        /// </summary>
        /// <typeparam name="TValue">The type of the Property</typeparam>
        /// <typeparam name="TType">The Type of the Object that defines the Property</typeparam>
        /// <param name="property">The Property that will no longer be watched.</param>
        /// <param name="handler">The handler that will no longer be called if a Property Changes.</param>
        public static void RemoveEventHandler<TType, TValue>(NDReadOnlyPropertyKey<TKey, TType, TValue> property, EventHandler<ChangedEventArgs<TKey, TType, TValue>> handler)
            where TType : class
        {
            if (!Lookup<TType, TValue>.PropertyHandler.ContainsKey(property))
                return;
            var list = Lookup<TType, TValue>.PropertyHandler[property];
            list.Remove(handler);
            if (list.Count == 0)
                Lookup<TType, TValue>.PropertyHandler.Remove(property);
        }

        internal static class Lookup<TType, TValue> where TType : class
        {
            public readonly static Dictionary<(TType, NDReadOnlyPropertyKey<TKey, TType, TValue>), TValue> Property = new Dictionary<(TType, NDReadOnlyPropertyKey<TKey, TType, TValue>), TValue>();
            public readonly static Dictionary<(TType, NDReadOnlyPropertyKey<TKey, TType, TValue>), List<EventHandler<ChangedEventArgs<TKey, TType, TValue>>>> Handler = new Dictionary<(TType, NDReadOnlyPropertyKey<TKey, TType, TValue>), List<EventHandler<ChangedEventArgs<TKey, TType, TValue>>>>();
            public readonly static Dictionary<NDReadOnlyPropertyKey<TKey, TType, TValue>, List<EventHandler<ChangedEventArgs<TKey, TType, TValue>>>> PropertyHandler = new Dictionary<NDReadOnlyPropertyKey<TKey, TType, TValue>, List<EventHandler<ChangedEventArgs<TKey, TType, TValue>>>>();
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

    public delegate void OnChanging<TKey, TValue>(OnChangingArg<TKey, TValue> arg);
    public delegate void OnChanging<TKey, TType, TValue>(OnChangingArg<TKey, TType, TValue> arg) where TType : class;


}
