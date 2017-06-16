using System;
using System.ComponentModel;
using System.Diagnostics;
using CodeGeneration.Roslyn;

namespace NDProperty
{ 
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class NDPAttributeBase : Attribute
    {
        internal NDPAttributeBase() { }

        public bool Inherited { get; set; } = false;
        public bool IsReadOnly { get; set; } = false;
        public bool IsParentReference { get; set; } = false;
        public NullTreatment NullTreatment { get; set; } = NullTreatment.RemoveLocalValue;
    }

    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    [CodeGenerationAttribute(typeof(Generator.NDPGeneratorProperty))]
    [Conditional("CodeGeneration")]
    public sealed class NDPAttribute : NDPAttributeBase { }

    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    [CodeGenerationAttribute(typeof(Generator.NDPGeneratorAttachedProperty))]
    [Conditional("CodeGeneration")]
    public sealed class NDPAttachAttribute : NDPAttributeBase { }


}
