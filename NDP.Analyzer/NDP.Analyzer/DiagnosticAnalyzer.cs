using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NDProperty;

namespace NDP.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NDPAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "NDPAnalyzer";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);



        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            NDProperty.Generator.Class2.RuleNDP0001,
            NDProperty.Generator.Class2.RuleNDP0002,
            NDProperty.Generator.Class2.RuleNDP0003,
            NDProperty.Generator.Class2.RuleNDP0003,
            NDProperty.Generator.Class2.RuleNDP0004);


        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            //context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method);
            context.RegisterSyntaxNodeAction(AnalyzeMethods, SyntaxKind.Attribute);
        }

        private void AnalyzeMethods(SyntaxNodeAnalysisContext context)
        {
            var attribute = context.Node as AttributeSyntax;

            var attributeType = context.SemanticModel.GetTypeInfo(attribute);
            if (!TypeSymbolMatchesType(attributeType.ConvertedType, typeof(NDPAttribute), context.SemanticModel))
                return; // Not our Attribute.


            var method = attribute.Parent.Parent as MethodDeclarationSyntax;

            var diagnostics = NDProperty.Generator.Class2.GenerateDiagnostics(method);

            foreach (var d in diagnostics)
                context.ReportDiagnostic(d);
            
        }


        static bool TypeSymbolMatchesType(ITypeSymbol typeSymbol, Type type, SemanticModel semanticModel)
        {
            return GetTypeSymbolForType(type, semanticModel).Equals(typeSymbol);
        }

        static INamedTypeSymbol GetTypeSymbolForType(Type type, SemanticModel semanticModel)
        {

            if (!type.IsConstructedGenericType)
            {
                return semanticModel.Compilation.GetTypeByMetadataName(type.FullName);
            }

            // get all typeInfo's for the Type arguments 
            var typeArgumentsTypeInfos = type.GenericTypeArguments.Select(a => GetTypeSymbolForType(a, semanticModel));

            var openType = type.GetGenericTypeDefinition();
            var typeSymbol = semanticModel.Compilation.GetTypeByMetadataName(openType.FullName);
            return typeSymbol.Construct(typeArgumentsTypeInfos.ToArray<ITypeSymbol>());
        }
        //private static void AnalyzeSymbol(SymbolAnalysisContext context)
        //{
        //    // TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find
        //    var namedTypeSymbol = (IMethodSymbol)context.Symbol;
        //    namedTypeSymbol.GetAttributes().FirstOrDefault(x=>x.AttributeClass.ty)
        //    // Find just those named type symbols with names containing lowercase letters.
        //    if (namedTypeSymbol.Name.ToCharArray().Any(char.IsLower))
        //    {
        //        // For all such symbols, produce a diagnostic.
        //        var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);

        //        context.ReportDiagnostic(diagnostic);
        //    }
        //}
    }
}
