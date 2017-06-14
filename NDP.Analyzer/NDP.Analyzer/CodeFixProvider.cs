using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace NDP.Analyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NDPAnalyzerCodeFixProvider)), Shared]
    public class NDPAnalyzerCodeFixProvider : CodeFixProvider
    {
        private const string title = "Make uppercase";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(
            NDProperty.Generator.Class2.NDP0001,
            NDProperty.Generator.Class2.NDP0002,
            NDProperty.Generator.Class2.NDP0003,
            NDProperty.Generator.Class2.NDP0003);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {

            foreach (var diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case NDProperty.Generator.Class2.NDP0001:
                        context.RegisterCodeFix(
                        CodeAction.Create(
                            title: "The Method was not named after Convention",
                            createChangedSolution: c => RenameMethod(diagnostic, context, c),
                            equivalenceKey: diagnostic.Id),
                        diagnostic);
                        break;
                    case NDProperty.Generator.Class2.NDP0002:


                        context.RegisterCodeFix(
                        CodeAction.Create(
                            title: "Wrong parameters",
                            createChangedSolution: c => ReplaceParameter(diagnostic, context, c),
                            equivalenceKey: diagnostic.Id),
                        diagnostic);
                        break;
                    case NDProperty.Generator.Class2.NDP0003:
                        context.RegisterCodeFix(
                        CodeAction.Create(
                            title: "The class is not partial",
                            createChangedSolution: c => AddPartial(diagnostic, context, c),
                            equivalenceKey: diagnostic.Id),
                        diagnostic);
                        break;
                    default:
                        continue;
                }


            }


            // Register a code action that will invoke the fix.
            return Task.FromResult<object>(null);
        }

        private async Task<Solution> AddPartial(Diagnostic diagnostic, CodeFixContext context, CancellationToken cancellationToken)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            // Find the type declaration identified by the diagnostic.
            var classDeclaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().First();
            var newClassDeclaretaion = classDeclaration.WithModifiers(classDeclaration.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.PartialKeyword)));


            root = root.ReplaceNode(classDeclaration, newClassDeclaretaion);

            // Produce a new solution that has all references to that type renamed, including the declaration.
            var originalSolution = context.Document.Project.Solution;
            return originalSolution.WithDocumentSyntaxRoot(context.Document.Id, root);
        }

        private async Task<Solution> ReplaceParameter(Diagnostic diagnostic, CodeFixContext context, CancellationToken cancellationToken)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            // Find the type declaration identified by the diagnostic.
            var methodDeclaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().First();
            var parameterList = methodDeclaration.ParameterList;

            TypeSyntax genericType;
            if (parameterList.Parameters.Count > 0)
                genericType = parameterList.Parameters[0].Type;
            else
                genericType = SyntaxFactory.PredefinedType(
                                        SyntaxFactory.Token(SyntaxKind.ObjectKeyword));

            var newParameterList = SyntaxFactory.ParameterList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Parameter(
                        SyntaxFactory.Identifier("arg"))
                    .WithType(
                        SyntaxFactory.GenericName(
                            SyntaxFactory.Identifier(nameof(NDProperty.OnChangedArg)))
                        .WithTypeArgumentList(
                            SyntaxFactory.TypeArgumentList(
                                SyntaxFactory.SingletonSeparatedList(
                                    genericType))))));

            root = root.ReplaceNode(parameterList, newParameterList);

            // Produce a new solution that has all references to that type renamed, including the declaration.
            var originalSolution = context.Document.Project.Solution;
            return originalSolution.WithDocumentSyntaxRoot(context.Document.Id, root);
        }

        private async Task<Solution> RenameMethod(Diagnostic diagnostic, CodeFixContext context, CancellationToken cancellationToken)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var document = context.Document;

            var typeDecl = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().First();


            // Compute new uppercase name.
            var identifierToken = typeDecl.Identifier;
            var oldName = identifierToken.Text;
            var newName = (oldName.StartsWith("On") ? "" : "On") + oldName + (oldName.EndsWith("Changed") ? "" : "Changed");

            // Get the symbol representing the type to be renamed.
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var typeSymbol = semanticModel.GetDeclaredSymbol(typeDecl, cancellationToken);

            // Produce a new solution that has all references to that type renamed, including the declaration.
            var originalSolution = document.Project.Solution;
            var optionSet = originalSolution.Workspace.Options;
            var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, typeSymbol, newName, optionSet, cancellationToken).ConfigureAwait(false);

            // Return the new solution with the now-uppercase type name.
            return newSolution;
        }
    }
}