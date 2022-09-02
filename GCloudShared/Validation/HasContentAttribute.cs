using System;

namespace mvdata.foodjet.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class HasContentAttribute : Attribute
    {
    }
}