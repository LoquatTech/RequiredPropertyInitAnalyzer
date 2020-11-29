using System.Collections.Generic;
using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

using RequiredPropertyInitAnalyzer.Utils;

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

        private static readonly DiagnosticDescriptor Rule = new(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Error,
            true,
            Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private static bool CheckPropertyHasInit(IPropertySymbol propertySymbol)
        {
            return propertySymbol.SetMethod?.IsInitOnly == true;
        }

        private static HashSet<string> GetRequiredProperties(ITypeSymbol initializationType)
        {
            bool typeHasRequiredAttribute = RequiredAttributeUtils.TypeIsRequired(initializationType);

            var requiredProperties = new HashSet<string>();

            foreach (var propertySymbol in TypeSymbolUtils.GetProperties(initializationType))
            {
                bool isRequired = typeHasRequiredAttribute
                               || RequiredAttributeUtils.PropertyIsRequired(propertySymbol);

                if (isRequired && CheckPropertyHasInit(propertySymbol))
                {
                    requiredProperties.Add(propertySymbol.Name);
                }
            }

            return requiredProperties;
        }

        private static void GetUninitializedProperties(IObjectOrCollectionInitializerOperation initializer, HashSet<string> requiredProperties)
        {
            if (initializer?.Initializers is null)
            {
                return;
            }

            foreach (var operation in initializer.Initializers)
            {
                if (operation is ISimpleAssignmentOperation { Target: IPropertyReferenceOperation propertyReference })
                {
                    string initPropName = propertyReference.Property.Name;

                    requiredProperties.Remove(initPropName);
                }
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterOperationAction(this.AnalyzeOperation, OperationKind.ObjectCreation);
        }

        private void AnalyzeOperation(OperationAnalysisContext context)
        {
            if (context.Operation is not IObjectCreationOperation creationOperation)
            {
                return;
            }

            var returnType = creationOperation.Type;

            var requiredProperties = GetRequiredProperties(returnType);

            GetUninitializedProperties(creationOperation.Initializer, requiredProperties);

            if (requiredProperties.Count > 0)
            {
                string uninitializedPropertiesList = string.Join(", ", requiredProperties);
                context.ReportDiagnostic(Diagnostic.Create(Rule, creationOperation.Syntax.GetLocation(), uninitializedPropertiesList));
            }
        }
    }
}
