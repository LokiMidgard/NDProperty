using System;
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
        public Class2(AttributeData attributeData)
        {
            Requires.NotNull(attributeData, nameof(attributeData));


            //  this.suffix = (string)attributeData.ConstructorArguments[0].Value;
        }
        //public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(MemberDeclarationSyntax applyTo, CSharpCompilation compilation, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        //{
        //    var results = SyntaxFactory.List<MemberDeclarationSyntax>();

        //    MemberDeclarationSyntax copy = null;
        //    var applyToClass = applyTo as ClassDeclarationSyntax;
        //    if (applyToClass != null)
        //    {
        //        copy = applyToClass
        //            .WithIdentifier(SyntaxFactory.Identifier(applyToClass.Identifier.ValueText + this.suffix));
        //    }

        //    if (copy != null)
        //    {
        //        results = results.Add(copy);
        //    }

        //    return Task.FromResult(results);
        //}
        public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(MemberDeclarationSyntax applyTo, CSharpCompilation compilation, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {

            //var results = SyntaxFactory.List<MemberDeclarationSyntax>();

            //MemberDeclarationSyntax copy = null;
            //var applyToClass = applyTo as ClassDeclarationSyntax;
            //if (applyToClass != null)
            //{
            //    copy = applyToClass
            //        .WithIdentifier(SyntaxFactory.Identifier(applyToClass.Identifier.ValueText + "Test"));
            //}

            //if (copy != null)
            //{
            //    results = results.Add(copy);
            //}

            //return Task.FromResult(results);

            var results = SyntaxFactory.List<MemberDeclarationSyntax>();
            //return Task.FromResult(results);

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


            //var property = SyntaxFactory.PropertyDeclaration(valueType, propertyName)
            //    .WithAccessorList(SyntaxFactory.List(
            //        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, )

            //        ));


            //var newClass = SyntaxFactory.ClassDeclaration(originalClassDeclaration.Identifier)
            //    .WithModifiers(originalClassDeclaration.Modifiers)
            //    .WithMembers(GenerateProperty(propertyName, originalClassDeclaration.Identifier.Text));

            //var trailingTrivia = newClass.GetTrailingTrivia();
            //originalClassDeclaration.WithMembers()


            //return Task.FromResult(results);

            //return Task.FromResult(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(newClass));
            return Task.FromResult(GenerateProperty(propertyName, originalClassDeclaration.Identifier.Text));
        }

        private static SyntaxList<MemberDeclarationSyntax> GenerateProperty(string propertyName, string className)
        {
            var propertyKey = propertyName + "Property";
            var propertyEvent = propertyName + "Changed";
            var propertyChangedMethod = $"On{propertyName}Changed";


            //return SyntaxFactory.List<MemberDeclarationSyntax>(
            //        new MemberDeclarationSyntax[0]);
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
                            SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
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
                                                    SyntaxFactory.IdentifierName("PropertyRegistar")),
                                                SyntaxFactory.GenericName(
                                                    SyntaxFactory.Identifier("Register"))
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
                                                                SyntaxKind.FalseLiteralExpression)),
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
                                                                    SyntaxFactory.IdentifierName("NullTreatment")),
                                                                SyntaxFactory.IdentifierName("RemoveLocalValue")))}))))))))
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
                                                                SyntaxFactory.IdentifierName("PropertyRegistar")),
                                                            SyntaxFactory.IdentifierName("GetValue")))
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
                                                                SyntaxFactory.IdentifierName("PropertyRegistar")),
                                                            SyntaxFactory.IdentifierName("SetValue")))
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
                            SyntaxFactory.Identifier("EventHandler"))
                        .WithTypeArgumentList(
                            SyntaxFactory.TypeArgumentList(
                                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                    SyntaxFactory.QualifiedName(
                                        SyntaxFactory.AliasQualifiedName(
                                            SyntaxFactory.IdentifierName(
                                                SyntaxFactory.Token(SyntaxKind.GlobalKeyword)),
                                            SyntaxFactory.IdentifierName("NDProperty")),
                                        SyntaxFactory.GenericName(
                                            SyntaxFactory.Identifier("ChangedEventArgs"))
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
                                                                SyntaxFactory.IdentifierName("PropertyRegistar")),
                                                            SyntaxFactory.IdentifierName("AddEventHandler")))
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
                                                                SyntaxFactory.IdentifierName("PropertyRegistar")),
                                                            SyntaxFactory.IdentifierName("RemoveEventHandler")))
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
