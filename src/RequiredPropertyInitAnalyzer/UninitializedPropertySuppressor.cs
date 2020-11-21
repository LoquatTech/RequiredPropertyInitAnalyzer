using System.Collections.Immutable;
using System.Linq;

using LoquatTech;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RequiredPropertyInitAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UninitializedPropertySuppressor : DiagnosticSuppressor
    {
        private const string SuppressionId = "SPR1001";

        private const string UninitializedPropertyDiagnosticId = "CS8618";

        private static readonly LocalizableString Justification = new LocalizableResourceString(
            nameof(Resources.UninitializedPropertySuppressionJustification),
            Resources.ResourceManager,
            typeof(Resources));

        private static readonly SuppressionDescriptor SuppressUninitializedProperty = new SuppressionDescriptor(
            SuppressionId,
            UninitializedPropertyDiagnosticId,
            Justification);

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } =
            ImmutableArray.Create(SuppressUninitializedProperty);

        private static void HandleDiagnostic(
            SuppressionAnalysisContext context,
            Diagnostic diagnostic,
            INamedTypeSymbol requiredType)
        {
            var sourceTree = diagnostic.Location.SourceTree;
            if (sourceTree is null)
            {
                return;
            }

            var node = sourceTree.GetRoot(context.CancellationToken).FindNode(diagnostic.Location.SourceSpan);

            if (node is not PropertyDeclarationSyntax propertyDeclaration)
            {
                return;
            }

            if (propertyDeclaration.Parent is not TypeDeclarationSyntax typeDeclaration)
            {
                return;
            }

            var model = context.GetSemanticModel(sourceTree);

            var declaredType = model.GetDeclaredSymbol(typeDeclaration, context.CancellationToken);

            if (declaredType is null)
            {
                return;
            }

            bool typeHasRequired = RequiredAttributeUtils.TypeIsRequired(declaredType, requiredType);

            var property = declaredType.GetMembers()
                                       .OfType<IPropertySymbol>()
                                       .FirstOrDefault(p => p.Name == propertyDeclaration.Identifier.ValueText);

            if ((property?.SetMethod == null)
             || property.SetMethod.IsInitOnly == false)
            {
                return;
            }

            if (typeHasRequired || RequiredAttributeUtils.PropertyIsRequired(property, requiredType))
            {
                context.ReportSuppression(Suppression.Create(SuppressUninitializedProperty, diagnostic));
            }
        }

        public override void ReportSuppressions(SuppressionAnalysisContext context)
        {
            var requiredType = context.Compilation.GetTypeByMetadataName(typeof(RequiredInitAttribute).FullName);

            if (requiredType == null)
            {
                return;
            }

            foreach (var diagnostic in context.ReportedDiagnostics)
            {
                HandleDiagnostic(context, diagnostic, requiredType);
            }
        }
    }
}
