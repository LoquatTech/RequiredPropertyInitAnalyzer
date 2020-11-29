using System;

namespace LoquatTech
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    internal class RequiredInitAttribute : Attribute
    {
    }
}
