using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace RequiredPropertyInitAnalyzer.Utils
{
    internal static class TypeSymbolUtils
    {
        public static IReadOnlyList<IPropertySymbol> GetProperties(ITypeSymbol type)
        {
            var properties = new List<IPropertySymbol>();

            var currentType = type;
            while (currentType != null)
            {
                properties.AddRange(currentType.GetMembers().OfType<IPropertySymbol>());

                currentType = currentType.BaseType;
            }


            return properties;
        }
    }
}
