using System.Collections.Generic;
using System.Collections.Immutable;

using LoquatTech.RequiredPropertyInit;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RequiredPropertyInitAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RequiredPropertyInitAnalyzer : DiagnosticAnalyzer
    {
        private const string Category = "Usage";
        private const string DiagnosticId = "RPI1001";

        private static readonly LocalizableString Description = new LocalizableResourceString(
            nameof(Resources.RequiredPropertiesDescription),
            Resources.ResourceManager,
            typeof(Resources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(Resources.RequiredPropertiesFormat),
            Resources.ResourceManager,
            typeof(Resources));

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(Resources.RequiredPropertiesTitle),
            Resources.ResourceManager,
            typeof(Resources));

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Error,
            true,
            Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(this.AnalyzeSyntaxNode, SyntaxKind.ObjectInitializerExpression);
        }

        private static bool CheckPropertyHasInit(IPropertySymbol propertySymbol)
        {
            return propertySymbol.SetMethod?.IsInitOnly == true;
        }

        private HashSet<string> GetRequiredProperties(ITypeSymbol initializationType, INamedTypeSymbol requiredType)
        {
            bool typeHasRequiredAttribute = RequiredAttributeUtils.TypeIsRequired(initializationType, requiredType);

            var requiredProperties = new HashSet<string>();

            foreach (var propertySymbol in initializationType.GetMembers().OfType<IPropertySymbol>())
            {
                bool isRequired = typeHasRequiredAttribute
                               || RequiredAttributeUtils.PropertyIsRequired(
                                      propertySymbol,
                                      requiredType);

                if (isRequired && CheckPropertyHasInit(propertySymbol))
                {
                    requiredProperties.Add(propertySymbol.Name);
                }
            }

            return requiredProperties;
        }

        private static HashSet<string> GetUninitializedProperties(
            InitializerExpressionSyntax initializer,
            HashSet<string> requiredProperties)
        {
            foreach (var expressionSyntax in initializer.Expressions)
            {
                if (expressionSyntax is not AssignmentExpressionSyntax assignment
                 || (assignment.Kind() != SyntaxKind.SimpleAssignmentExpression))
                {
                    continue;
                }

                if (assignment.Left is IdentifierNameSyntax identifier)
                {
                    string initPropName = identifier.Identifier.ValueText;

                    requiredProperties.Remove(initPropName);
                }
            }

            return requiredProperties;
        }

        private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var requiredType = context.Compilation.GetTypeByMetadataName(typeof(RequiredInitAttribute).FullName);

            if (requiredType == null)
            {
                return;
            }

            if (context.Node is not InitializerExpressionSyntax initializer)
            {
                return;
            }

            var initializerParent = initializer.Parent;
            if (initializerParent == null)
            {
                return;
            }

            switch (initializerParent.Kind())
            {
                case SyntaxKind.ObjectCreationExpression:
                case SyntaxKind.SimpleAssignmentExpression:
                    break;
                default:
                    return;
            }

            var initializationType = context.SemanticModel.GetTypeInfo(initializerParent).Type;
            if (initializationType == null)
            {
                return;
            }

            var requiredProperties = this.GetRequiredProperties(initializationType, requiredType);

            var uninitializedProperties = GetUninitializedProperties(initializer, requiredProperties);

            if (uninitializedProperties.Count > 0)
            {
                string uninitializedPropertiesList = string.Join(", ", uninitializedProperties);
                context.ReportDiagnostic(Diagnostic.Create(Rule, initializerParent.GetLocation(), uninitializedPropertiesList));
            }
        }
    }
}
