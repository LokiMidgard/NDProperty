using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CodeGeneration.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Validation;

namespace NDProperty.Generator
{
    public class Class2 : ICodeGenerator
    {
        private readonly bool inherited;
        private readonly NullTreatment nullTreatment;

        public Class2(AttributeData attributeData)
        {
            Requires.NotNull(attributeData, nameof(attributeData));


            //this.suffix = (string)attributeData.ConstructorArguments[0].Value;
            this.inherited = (bool)attributeData.NamedArguments.First(x => x.Key == nameof(NDPAttribute.Inherited)).Value.Value;
            this.nullTreatment = (NullTreatment)attributeData.NamedArguments.First(x => x.Key == nameof(NDPAttribute.NullTreatment)).Value.Value;
        }

        public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(MemberDeclarationSyntax applyTo, CSharpCompilation compilation, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {

            var results = SyntaxFactory.List<MemberDeclarationSyntax>();

            //if (!System.Diagnostics.Debugger.IsAttached)
            //    System.Diagnostics.Debugger.Launch();
            //else
            //    System.Diagnostics.Debugger.Break();

            var nameRegex = new Regex(@"On(?<name>\S+)Changed");

            var m = applyTo as MethodDeclarationSyntax;

            var originalClassDeclaration = applyTo.Parent as ClassDeclarationSyntax;

            if (originalClassDeclaration == null)
                return Task.FromResult(results);

            // check if class is partial
            if (!originalClassDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
                return Task.FromResult(results);


            if (m.ParameterList.Parameters.Count != 1)
                return Task.FromResult(results);

            var changedMethodParameter = m.ParameterList.Parameters[0];
            var parameterType = changedMethodParameter.Type as GenericNameSyntax;
            if (parameterType?.Identifier.Text != "OnChangedArg")
                return Task.FromResult(results);

            var valueType = parameterType.TypeArgumentList.Arguments[0];


            var nameMatch = nameRegex.Match(m.Identifier.Text);
            if (!nameMatch.Success)
                return Task.FromResult(results);

            var propertyName = nameMatch.Groups["name"].Value;

            return Task.FromResult(GenerateProperty(propertyName, originalClassDeclaration.Identifier.Text));
        }

        private SyntaxList<MemberDeclarationSyntax> GenerateProperty(string propertyName, string className)
        {
            var propertyKey = propertyName + "Property";
            var propertyEvent = propertyName + "Changed";
            var propertyChangedMethod = $"On{propertyName}Changed";

            return SyntaxFactory.List<MemberDeclarationSyntax>(
                    new MemberDeclarationSyntax[]{
                    SyntaxFactory.FieldDeclaration(
                        SyntaxFactory.VariableDeclaration(
                            SyntaxFactory.QualifiedName(
                                SyntaxFactory.AliasQualifiedName(
                                    SyntaxFactory.IdentifierName(
                                        SyntaxFactory.Token(SyntaxKind.GlobalKeyword)),
                                    SyntaxFactory.IdentifierName("NDProperty")),
                                SyntaxFactory.GenericName(
                                    SyntaxFactory.Identifier("NDProperty"))
                                .WithTypeArgumentList(
                                    SyntaxFactory.TypeArgumentList(
                                        SyntaxFactory.SeparatedList<TypeSyntax>(
                                            new SyntaxNodeOrToken[]{
                                                SyntaxFactory.PredefinedType(
                                                    SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                                                SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                SyntaxFactory.IdentifierName(className)})))))
                        .WithVariables(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.VariableDeclarator(
                                    SyntaxFactory.Identifier(propertyKey))
                                .WithInitializer(
                                    SyntaxFactory.EqualsValueClause(
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.AliasQualifiedName(
                                                        SyntaxFactory.IdentifierName(
                                                            SyntaxFactory.Token(SyntaxKind.GlobalKeyword)),
                                                        SyntaxFactory.IdentifierName("NDProperty")),
                                                    SyntaxFactory.IdentifierName(nameof( PropertyRegistar))),
                                                SyntaxFactory.GenericName(
                                                    SyntaxFactory.Identifier(nameof(PropertyRegistar.Register)))
                                                .WithTypeArgumentList(
                                                    SyntaxFactory.TypeArgumentList(
                                                        SyntaxFactory.SeparatedList<TypeSyntax>(
                                                            new SyntaxNodeOrToken[]{
                                                                SyntaxFactory.PredefinedType(
                                                                    SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                                                                SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                                SyntaxFactory.IdentifierName(className)})))))
                                        .WithArgumentList(
                                            SyntaxFactory.ArgumentList(
                                                SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                                    new SyntaxNodeOrToken[]{
                                                        SyntaxFactory.Argument(
                                                            SyntaxFactory.SimpleLambdaExpression(
                                                                SyntaxFactory.Parameter(
                                                                    SyntaxFactory.Identifier("t")),
                                                                SyntaxFactory.MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    SyntaxFactory.IdentifierName("t"),
                                                                    SyntaxFactory.IdentifierName(propertyChangedMethod)))),
                                                        SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                        SyntaxFactory.Argument(
                                                            SyntaxFactory.LiteralExpression(
                                                                this.inherited ?SyntaxKind.TrueLiteralExpression :SyntaxKind.FalseLiteralExpression)),
                                                        SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                        SyntaxFactory.Argument(
                                                            SyntaxFactory.MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                SyntaxFactory.MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    SyntaxFactory.AliasQualifiedName(
                                                                        SyntaxFactory.IdentifierName(
                                                                            SyntaxFactory.Token(SyntaxKind.GlobalKeyword)),
                                                                        SyntaxFactory.IdentifierName("NDProperty")),
                                                                    SyntaxFactory.IdentifierName(nameof(NullTreatment))),
                                                                SyntaxFactory.IdentifierName(this.nullTreatment.ToString())))}))))))))
                    .WithModifiers(
                        SyntaxFactory.TokenList(
                            new []{
                                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                                SyntaxFactory.Token(SyntaxKind.StaticKeyword),
                                SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)})),
                    SyntaxFactory.PropertyDeclaration(
                        SyntaxFactory.PredefinedType(
                            SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                        SyntaxFactory.Identifier(propertyName))
                    .WithModifiers(
                        SyntaxFactory.TokenList(
                            SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                    .WithAccessorList(
                        SyntaxFactory.AccessorList(
                            SyntaxFactory.List<AccessorDeclarationSyntax>(
                                new AccessorDeclarationSyntax[]{
                                    SyntaxFactory.AccessorDeclaration(
                                        SyntaxKind.GetAccessorDeclaration)
                                    .WithBody(
                                        SyntaxFactory.Block(
                                            SyntaxFactory.SingletonList<StatementSyntax>(
                                                SyntaxFactory.ReturnStatement(
                                                    SyntaxFactory.InvocationExpression(
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxFactory.MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                SyntaxFactory.AliasQualifiedName(
                                                                    SyntaxFactory.IdentifierName(
                                                                        SyntaxFactory.Token(SyntaxKind.GlobalKeyword)),
                                                                    SyntaxFactory.IdentifierName("NDProperty")),
                                                                SyntaxFactory.IdentifierName(nameof(PropertyRegistar))),
                                                            SyntaxFactory.IdentifierName(nameof(PropertyRegistar.GetValue))))
                                                    .WithArgumentList(
                                                        SyntaxFactory.ArgumentList(
                                                            SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                                                new SyntaxNodeOrToken[]{
                                                                    SyntaxFactory.Argument(
                                                                        SyntaxFactory.IdentifierName(propertyKey)),
                                                                    SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                                    SyntaxFactory.Argument(
                                                                        SyntaxFactory.ThisExpression())}))))))),
                                    SyntaxFactory.AccessorDeclaration(
                                        SyntaxKind.SetAccessorDeclaration)
                                    .WithBody(
                                        SyntaxFactory.Block(
                                            SyntaxFactory.SingletonList<StatementSyntax>(
                                                SyntaxFactory.ExpressionStatement(
                                                    SyntaxFactory.InvocationExpression(
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxFactory.MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                SyntaxFactory.AliasQualifiedName(
                                                                    SyntaxFactory.IdentifierName(
                                                                        SyntaxFactory.Token(SyntaxKind.GlobalKeyword)),
                                                                    SyntaxFactory.IdentifierName("NDProperty")),
                                                                SyntaxFactory.IdentifierName(nameof(PropertyRegistar))),
                                                            SyntaxFactory.IdentifierName(nameof(PropertyRegistar.SetValue))))
                                                    .WithArgumentList(
                                                        SyntaxFactory.ArgumentList(
                                                            SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                                                new SyntaxNodeOrToken[]{
                                                                    SyntaxFactory.Argument(
                                                                        SyntaxFactory.IdentifierName(propertyKey)),
                                                                    SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                                    SyntaxFactory.Argument(
                                                                        SyntaxFactory.ThisExpression()),
                                                                    SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                                    SyntaxFactory.Argument(
                                                                        SyntaxFactory.IdentifierName("value"))})))))))}))),
                    SyntaxFactory.EventDeclaration(
                        SyntaxFactory.GenericName(
                            SyntaxFactory.Identifier(nameof(EventHandler)))
                        .WithTypeArgumentList(
                            SyntaxFactory.TypeArgumentList(
                                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                    SyntaxFactory.QualifiedName(
                                        SyntaxFactory.AliasQualifiedName(
                                            SyntaxFactory.IdentifierName(
                                                SyntaxFactory.Token(SyntaxKind.GlobalKeyword)),
                                            SyntaxFactory.IdentifierName("NDProperty")),
                                        SyntaxFactory.GenericName(
                                            SyntaxFactory.Identifier(nameof(ChangedEventArgs)))
                                        .WithTypeArgumentList(
                                            SyntaxFactory.TypeArgumentList(
                                                SyntaxFactory.SeparatedList<TypeSyntax>(
                                                    new SyntaxNodeOrToken[]{
                                                        SyntaxFactory.PredefinedType(
                                                            SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                                                        SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                        SyntaxFactory.IdentifierName(className)}))))))),
                        SyntaxFactory.Identifier(propertyEvent))
                    .WithModifiers(
                        SyntaxFactory.TokenList(
                            SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                    .WithAccessorList(
                        SyntaxFactory.AccessorList(
                            SyntaxFactory.List<AccessorDeclarationSyntax>(
                                new AccessorDeclarationSyntax[]{
                                    SyntaxFactory.AccessorDeclaration(
                                        SyntaxKind.AddAccessorDeclaration)
                                    .WithBody(
                                        SyntaxFactory.Block(
                                            SyntaxFactory.SingletonList<StatementSyntax>(
                                                SyntaxFactory.ExpressionStatement(
                                                    SyntaxFactory.InvocationExpression(
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxFactory.MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                SyntaxFactory.AliasQualifiedName(
                                                                    SyntaxFactory.IdentifierName(
                                                                        SyntaxFactory.Token(SyntaxKind.GlobalKeyword)),
                                                                    SyntaxFactory.IdentifierName("NDProperty")),
                                                                SyntaxFactory.IdentifierName(nameof(PropertyRegistar))),
                                                            SyntaxFactory.IdentifierName(nameof(PropertyRegistar.AddEventHandler))))
                                                    .WithArgumentList(
                                                        SyntaxFactory.ArgumentList(
                                                            SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                                                new SyntaxNodeOrToken[]{
                                                                    SyntaxFactory.Argument(
                                                                        SyntaxFactory.IdentifierName(propertyKey)),
                                                                    SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                                    SyntaxFactory.Argument(
                                                                        SyntaxFactory.ThisExpression()),
                                                                    SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                                    SyntaxFactory.Argument(
                                                                        SyntaxFactory.IdentifierName("value"))}))))))),
                                    SyntaxFactory.AccessorDeclaration(
                                        SyntaxKind.RemoveAccessorDeclaration)
                                    .WithBody(
                                        SyntaxFactory.Block(
                                            SyntaxFactory.SingletonList<StatementSyntax>(
                                                SyntaxFactory.ExpressionStatement(
                                                    SyntaxFactory.InvocationExpression(
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxFactory.MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                SyntaxFactory.AliasQualifiedName(
                                                                    SyntaxFactory.IdentifierName(
                                                                        SyntaxFactory.Token(SyntaxKind.GlobalKeyword)),
                                                                    SyntaxFactory.IdentifierName("NDProperty")),
                                                                SyntaxFactory.IdentifierName(nameof(PropertyRegistar))),
                                                            SyntaxFactory.IdentifierName(nameof(PropertyRegistar.RemoveEventHandler))))
                                                    .WithArgumentList(
                                                        SyntaxFactory.ArgumentList(
                                                            SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                                                new SyntaxNodeOrToken[]{
                                                                    SyntaxFactory.Argument(
                                                                        SyntaxFactory.IdentifierName(propertyKey)),
                                                                    SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                                    SyntaxFactory.Argument(
                                                                        SyntaxFactory.ThisExpression()),
                                                                    SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                                    SyntaxFactory.Argument(
                                                                        SyntaxFactory.IdentifierName("value"))})))))))})))});



        }

        //private void OnStrChanged(OnChangedArg<string> arg)
        //{

        //}

    }
}
