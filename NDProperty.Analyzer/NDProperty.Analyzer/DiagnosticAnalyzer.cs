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
using NDProperty.Generator;

namespace NDP.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NDPPropertyAnalyzer : NDPBaseAnalyzer
    {
        public override NDPGenerator Generator { get; } = new NDPGeneratorProperty();
    }
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NDPAttachedAnalyzer : NDPBaseAnalyzer
    {
        public override NDPGenerator Generator { get; } = new NDPGeneratorAttachedProperty();
    }
    public abstract class NDPBaseAnalyzer : DiagnosticAnalyzer
    {
        //// You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        //// See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        //private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        //private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        //private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        public abstract NDProperty.Generator.NDPGenerator Generator { get; }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            Generator.ClassNotFound,
            Generator.ClassNotPartial,
            Generator.MethodNameConvention,
            Generator.WrongParameter);


        public override void Initialize(AnalysisContext context)
        {
#if false
            System.Diagnostics.Debugger.Launch();
#endif
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            //context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method);
            context.RegisterSyntaxNodeAction(AnalyzeMethods, SyntaxKind.Attribute);
        }

        private void AnalyzeMethods(SyntaxNodeAnalysisContext context)
        {
            var attribute = context.Node as AttributeSyntax;

            var attributeType = context.SemanticModel.GetTypeInfo(attribute);
            if (!NDProperty.Generator.NDPGenerator.TypeSymbolMatchesType(attributeType.ConvertedType, Generator.AttributeType, context.SemanticModel))
                return; // Not our Attribute.

            var method = attribute.Parent.Parent as MethodDeclarationSyntax;
            var diagnostics = Generator.GenerateDiagnostics(method, context.SemanticModel);

            foreach (var d in diagnostics)
                context.ReportDiagnostic(d);

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
