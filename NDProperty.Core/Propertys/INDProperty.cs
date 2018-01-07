[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("NDProperty.Generator")]
namespace NDProperty.Propertys
{
    public interface INDProperty<TKey, TType, TValue> where TType : class
    {
        /// <summary>
        /// Access the readonly property of this Property.
        /// </summary>
        /// <remarks>
        /// This Proeprty can be used to allow read but not write to an Property.
        /// </remarks>
        NDReadOnlyPropertyKey<TKey, TType, TValue> ReadOnlyProperty { get; }
    }
}