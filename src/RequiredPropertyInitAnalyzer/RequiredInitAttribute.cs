using System;

namespace LoquatTech.RequiredPropertyInit
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class RequiredInitAttribute : Attribute
    {
    }
}
