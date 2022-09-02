using System;

namespace mvdata.foodjet.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ContentEqualsAttribute : Attribute
    {
        public string OtherView { get; set; }   
    }
}