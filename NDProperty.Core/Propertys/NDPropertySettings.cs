using System;

namespace NDProperty.Propertys
{
    [Flags]
    public enum NDPropertySettings
    {
        None = 0,
        /// <summary>
        /// Normaly the OnPropertyChanged handler will only be called if old and new value are not equal. Using this setting it will even called if new and old value are the same.
        /// </summary>
        CallOnChangedHandlerOnEquals = 1 << 1,
        /// <summary>
        /// Will define that this Property defines the Parent - Child relationship.
        /// </summary>
        ParentReference = 1 << 2,
        /// <summary>
        /// Will define that this Property should be read only. 
        /// </summary>
        /// <remarks>
        /// This property will be used for code generation in the first place. <para/>
        /// It will not automaticly result in a readonly behavior. You need to restrict
        /// access to the <see cref="NDPropertyKey{TValue, TType}"/> and only hand out the <see cref="NDReadOnlyPropertyKey{TValue, TType}"/>.
        /// </remarks>
        ReadOnly = 1 << 3,
        /// <summary>
        /// Defines that this Property inherits its value fromits parent if no local value was set.
        /// </summary>
        /// <remarks>
        /// This requires that a Parent Reference exists on this object. Otherwise it does nothing.
        /// </remarks>
        Inherited = 1 << 4,
        /// <summary>
        /// Using this setting allows setting the local value to <c>null</c>.
        /// </summary>
        /// <remarks>
        /// If this setting is not set, assigning <c>null</c> to an Property will remove its value, setting an inherited value or the specified default value.
        /// If this setting is set assigning <c>null</c> to this Property will assigne this value to the property overiting iheritance and specified default value.
        /// </remarks>
        SetLocalExplicityNull = 1 << 5,


    }


}
