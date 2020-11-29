using LoquatTech;

using Microsoft.CodeAnalysis;

using RequiredPropertyInitAnalyzer.Utils;

namespace RequiredPropertyInitAnalyzer
{
    internal static class RequiredAttributeUtils
    {
        private static readonly string[] RequiredInitAttributeSegments = {
            nameof(LoquatTech),
            nameof(RequiredInitAttribute)
        };

        public static bool PropertyIsRequired(IPropertySymbol propertySymbol)
        {
            var attributes = propertySymbol.GetAttributes();

            foreach (var attributeData in attributes)
            {
                if (attributeData.AttributeClass is not null
                 && TypeNameUtils.HasFullName(attributeData.AttributeClass, RequiredInitAttributeSegments))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool TypeIsRequired(ITypeSymbol initializationType)
        {
            var attributes = initializationType.GetAttributes();

            foreach (var attributeData in attributes)
            {
                if (attributeData.AttributeClass is not null
                    && TypeNameUtils.HasFullName(attributeData.AttributeClass, RequiredInitAttributeSegments))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
