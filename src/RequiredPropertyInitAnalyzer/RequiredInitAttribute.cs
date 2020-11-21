using System;

namespace LoquatTech
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class RequiredInitAttribute : Attribute
    {
    }
}
