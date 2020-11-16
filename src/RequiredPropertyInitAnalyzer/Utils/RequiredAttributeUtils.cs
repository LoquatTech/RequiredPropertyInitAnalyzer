using Microsoft.CodeAnalysis;

namespace RequiredPropertyInitAnalyzer
{
    internal static class RequiredAttributeUtils
    {
        public static bool PropertyIsRequired(IPropertySymbol propertySymbol, INamedTypeSymbol requiredType)
        {
            var attributes = propertySymbol.GetAttributes();

            foreach (var attributeData in attributes)
            {
                if (SymbolEqualityComparer.Default.Equals(requiredType, attributeData.AttributeClass))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool TypeIsRequired(ITypeSymbol initializationType, INamedTypeSymbol requiredType)
        {
            var attributes = initializationType.GetAttributes();

            foreach (var attributeData in attributes)
            {
                if (SymbolEqualityComparer.Default.Equals(requiredType, attributeData.AttributeClass))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
