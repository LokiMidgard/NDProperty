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
using NDProperty.Generator;

namespace NDP.Analyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NDPPropertyAnalyzerCodeFixProvider)), Shared]
    public class NDPPropertyAnalyzerCodeFixProvider : NDPAnalyzerCodeFixProvider
    {
        public override NDPGenerator Generator { get; } = new NDPGeneratorProperty();

        internal override GenericNameSyntax GetTypeArgumentList(GenericNameSyntax qualifiedNameSyntax, TypeSyntax type)
        {


            return qualifiedNameSyntax.WithTypeArgumentList(
                SyntaxFactory.TypeArgumentList(
                    SyntaxFactory.SingletonSeparatedList(type)));
        }
    }
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NDPAttachedAnalyzerCodeFixProvider)), Shared]
    public class NDPAttachedAnalyzerCodeFixProvider : NDPAnalyzerCodeFixProvider
    {
        public override NDPGenerator Generator { get; } = new NDPGeneratorAttachedProperty();

        internal override GenericNameSyntax GetTypeArgumentList(GenericNameSyntax qualifiedNameSyntax, TypeSyntax type)
        {
            return qualifiedNameSyntax.WithTypeArgumentList(
            SyntaxFactory.TypeArgumentList(
                SyntaxFactory.SeparatedList<TypeSyntax>(
                    new SyntaxNodeOrToken[]{
                            type,
                            SyntaxFactory.Token(SyntaxKind.CommaToken),
                            SyntaxFactory.PredefinedType(
                                SyntaxFactory.Token(SyntaxKind.ObjectKeyword))})));
        }
    }
    public abstract class NDPAnalyzerCodeFixProvider : CodeFixProvider
    {
        private const string title = "Make uppercase";

        public abstract NDProperty.Generator.NDPGenerator Generator { get; }


        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(
            Generator.WrongParameter.Id,
            Generator.MethodNameConvention.Id,
            Generator.ClassNotPartial.Id);

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
                    case NDProperty.Generator.NDPGenerator.NDP0001:
                    case NDProperty.Generator.NDPGenerator.NDP0005:
                        context.RegisterCodeFix(
                        CodeAction.Create(
                            title: "Rename Method",
                            createChangedSolution: c => RenameMethod(diagnostic, context, c),
                            equivalenceKey: diagnostic.Id),
                        diagnostic);
                        break;

                    case NDProperty.Generator.NDPGenerator.NDP0002:
                    case NDProperty.Generator.NDPGenerator.NDP0006:
                        if (!context.Document.SupportsSemanticModel)
                            continue;
                        context.RegisterCodeFix(
                        CodeAction.Create(
                            title: "Convert to correct aparameters",
                            createChangedSolution: c => ReplaceParameter(diagnostic, context, c),
                            equivalenceKey: diagnostic.Id),
                        diagnostic);
                        break;

                    case NDProperty.Generator.NDPGenerator.NDP0003:
                    case NDProperty.Generator.NDPGenerator.NDP0007:
                        context.RegisterCodeFix(
                        CodeAction.Create(
                            title: "Make class partial",
                            createChangedSolution: c => AddPartial(diagnostic, context, c),
                            equivalenceKey: diagnostic.Id),
                        diagnostic);
                        break;

                    default:
                        continue;
                }


            }


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
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnosticSpan = diagnostic.Location.SourceSpan;
            // Find the type declaration identified by the diagnostic.
            var methodDeclaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().First();
            var parameterList = methodDeclaration.ParameterList;

            TypeSyntax parameterTypeSyntax;
            if (parameterList.Parameters.Count > 0)
            {
                var firstParameter = parameterList.Parameters.First();
                var typeInfo = semanticModel.GetTypeInfo(firstParameter.Type);


                if (NDPGenerator.TypeSymbolMatchesType(typeInfo.ConvertedType, Generator.OnChangedArgs, semanticModel, false))
                    parameterTypeSyntax = firstParameter.Type; // if we had the correct type then its ok.
                else
                {
                    var newType = firstParameter.Type;
                    if (NDPGenerator.TypeSymbolMatchesType(typeInfo.ConvertedType, typeof(NDProperty.OnChangedArg<,>), semanticModel, false)
                        || NDPGenerator.TypeSymbolMatchesType(typeInfo.ConvertedType, typeof(NDProperty.OnChangedArg<>), semanticModel, false))
                        newType = firstParameter.Type.DescendantNodesAndSelf().OfType<GenericNameSyntax>().First().TypeArgumentList.Arguments.First();
                    parameterTypeSyntax = SyntaxFactory.QualifiedName(
                            SyntaxFactory.AliasQualifiedName(
                                SyntaxFactory.IdentifierName(
                                    SyntaxFactory.Token(SyntaxKind.GlobalKeyword)),
                                SyntaxFactory.IdentifierName(nameof(NDProperty))),
                            GetTypeArgumentList(SyntaxFactory.GenericName(
                            SyntaxFactory.Identifier(nameof(NDProperty.OnChangedArg))), newType));
                }
            }
            else
            {
                parameterTypeSyntax = SyntaxFactory.QualifiedName(
                            SyntaxFactory.AliasQualifiedName(
                                SyntaxFactory.IdentifierName(
                                    SyntaxFactory.Token(SyntaxKind.GlobalKeyword)),
                                SyntaxFactory.IdentifierName(nameof(NDProperty))),
                            GetTypeArgumentList(SyntaxFactory.GenericName(
                            SyntaxFactory.Identifier(nameof(NDProperty.OnChangedArg))), SyntaxFactory.PredefinedType(
                                        SyntaxFactory.Token(SyntaxKind.ObjectKeyword))));
            }

            var newParameterList = SyntaxFactory.ParameterList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Parameter(
                        SyntaxFactory.Identifier("arg"))
                    .WithType(parameterTypeSyntax)));

            root = root.ReplaceNode(parameterList, newParameterList);

            // Produce a new solution that has all references to that type renamed, including the declaration.
            var originalSolution = context.Document.Project.Solution;
            return originalSolution.WithDocumentSyntaxRoot(context.Document.Id, root);
        }

        internal abstract GenericNameSyntax GetTypeArgumentList(GenericNameSyntax qualifiedNameSyntax, TypeSyntax type);

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