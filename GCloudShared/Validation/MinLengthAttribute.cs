using System;

namespace mvdata.foodjet.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class MinLengthAttribute : Attribute
    {
        public int MinLength { get; set; }
    }
}