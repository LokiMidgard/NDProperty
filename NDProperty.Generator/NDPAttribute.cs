using System;
using System.Diagnostics;
using CodeGeneration.Roslyn;

namespace NDProperty
{
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    [CodeGenerationAttribute(typeof(Generator.Class2))]
    [Conditional("CodeGeneration")]
    public sealed class NDPAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236

        public NDPAttribute()
        {

        }

        public bool Inherited { get; set; } = false;
        public NullTreatment NullTreatment { get; set; } = NullTreatment.RemoveLocalValue;
    }


}
