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
        private readonly bool isReadOnly;
        private readonly bool isParentReference;

        public Class2(AttributeData attributeData)
        {
            Requires.NotNull(attributeData, nameof(attributeData));

            var d = attributeData.NamedArguments.ToDictionary(x => x.Key, x => x.Value);

            this.isReadOnly = d.ContainsKey(nameof(NDPAttribute.IsReadOnly)) ? (bool)d[nameof(NDPAttribute.IsReadOnly)].Value : false;
            this.inherited = d.ContainsKey(nameof(NDPAttribute.Inherited)) ? (bool)d[nameof(NDPAttribute.Inherited)].Value : false;
            this.isParentReference = d.ContainsKey(nameof(NDPAttribute.IsParentReference)) ? (bool)d[nameof(NDPAttribute.IsParentReference)].Value : false;
            this.nullTreatment = d.ContainsKey(nameof(NDPAttribute.NullTreatment)) ? (NullTreatment)d[nameof(NDPAttribute.Inherited)].Value : NullTreatment.RemoveLocalValue;

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

            return Task.FromResult(GenerateProperty(propertyName, originalClassDeclaration.Identifier.Text, isReadOnly));
        }

        private SyntaxList<MemberDeclarationSyntax> GenerateProperty(string propertyName, string className, bool isReadOnly)
        {
            var propertyKey = propertyName + "Property";
            var propertyReadOnlyKey = propertyName + "ReadOnlyProperty";
            var propertyEvent = propertyName + "Changed";
            var propertyChangedMethod = $"On{propertyName}Changed";


            var list = new System.Collections.Generic.List<MemberDeclarationSyntax>();
            if (isReadOnly)
            {
                list.Add(GeneratePropertyKey(propertyName, className, propertyKey, propertyChangedMethod, Accessibility.Private));
                list.Add(GenerateReadOnlyPropertyKey(propertyName, propertyReadOnlyKey, className, propertyKey, propertyChangedMethod));
            }
            else
            {
                list.Add(GeneratePropertyKey(propertyName, className, propertyKey, propertyChangedMethod, Accessibility.Public));
            }

            list.Add(GeneratePropertyProperty(propertyName, propertyKey, isReadOnly? Accessibility.Private: Accessibility.NotApplicable));
            list.Add(GenerateEvent(propertyKey, propertyEvent, className));
            return SyntaxFactory.List<MemberDeclarationSyntax>(list);
        }

        private MemberDeclarationSyntax GenerateEvent(string propertyKey, string propertyEvent, string className)
        {
            return SyntaxFactory.EventDeclaration(
                           SyntaxFactory.GenericName(
                               SyntaxFactory.Identifier(nameof(EventHandler)))
                           .WithTypeArgumentList(
                               SyntaxFactory.TypeArgumentList(
                                   SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                       SyntaxFactory.QualifiedName(
                                           SyntaxFactory.AliasQualifiedName(
                                               SyntaxFactory.IdentifierName(
                                                   SyntaxFactory.Token(SyntaxKind.GlobalKeyword)),
                                               SyntaxFactory.IdentifierName(nameof(NDProperty))),
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
                                                                    SyntaxFactory.IdentifierName(nameof(NDProperty))),
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
                                                                    SyntaxFactory.IdentifierName(nameof(NDProperty))),
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
                                                                        SyntaxFactory.IdentifierName("value"))})))))))})));
        }

        private MemberDeclarationSyntax GeneratePropertyProperty(string propertyName, string propertyKey, Accessibility setterAccessibility = Accessibility.NotApplicable)
        {
            var setterModifier = SyntaxFactory.TokenList();
            if (setterAccessibility != Accessibility.NotApplicable)
                setterModifier = setterModifier.Add(SyntaxFactory.Token(ToAccessibilitySyntax(setterAccessibility)));
            return SyntaxFactory.PropertyDeclaration(
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
                                                                    SyntaxFactory.IdentifierName(nameof(NDProperty))),
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
                                        .WithModifiers(setterModifier)
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
                                                                    SyntaxFactory.IdentifierName(nameof(NDProperty))),
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
                                                                        SyntaxFactory.IdentifierName("value"))})))))))})))
;
        }

        private MemberDeclarationSyntax GeneratePropertyKey(string propertyName, string className, string propertyKey, string propertyChangedMethod, Accessibility filedAccessibility)
        {
            var register = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.AliasQualifiedName(
                            SyntaxFactory.IdentifierName(
                                SyntaxFactory.Token(SyntaxKind.GlobalKeyword)),
                            SyntaxFactory.IdentifierName(nameof(NDProperty))),
                        SyntaxFactory.IdentifierName(nameof(PropertyRegistar))),
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
                                            SyntaxFactory.IdentifierName(nameof(NDProperty))),
                                        SyntaxFactory.IdentifierName(nameof(NullTreatment))),
                                    SyntaxFactory.IdentifierName(this.nullTreatment.ToString()))),
                            SyntaxFactory.Token(SyntaxKind.CommaToken),
                            SyntaxFactory.Argument(
                                SyntaxFactory.LiteralExpression(
                                    this.isParentReference ?SyntaxKind.TrueLiteralExpression :SyntaxKind.FalseLiteralExpression))
                        })));

            return GenerateLeftKeyPart(className, propertyKey, register, filedAccessibility, PropertyKind.Normal);
        }

        private MemberDeclarationSyntax GenerateReadOnlyPropertyKey(string propertyName, string propertyReadOnlyKey, string className, string propertyKey, string propertyChangedMethod)
        {
            var register = SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName(propertyKey),
                                SyntaxFactory.IdentifierName(nameof(NDProperty<object, object>.ReadOnlyProperty)));

            return GenerateLeftKeyPart(className, propertyReadOnlyKey, register, Accessibility.Public, PropertyKind.Readonly);
        }

        private enum PropertyKind
        {
            Normal,
            Readonly,
            Attached
        }

        private static FieldDeclarationSyntax GenerateLeftKeyPart(string className, string propertyKey, ExpressionSyntax register, Accessibility filedAccessibility, PropertyKind kind)
        {
            SyntaxKind accesibility;

            accesibility = ToAccessibilitySyntax(filedAccessibility);

            string propertyClass;
            switch (kind)
            {
                case PropertyKind.Normal:
                    propertyClass = nameof(NDProperty<object, object>);
                    break;
                case PropertyKind.Readonly:
                    propertyClass = nameof(NDReadOnlyProperty<object, object>);
                    break;
                case PropertyKind.Attached:
                    propertyClass = nameof(NDAttachedProperty<object, object>);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return SyntaxFactory.FieldDeclaration(
    SyntaxFactory.VariableDeclaration(
        SyntaxFactory.QualifiedName(
            SyntaxFactory.AliasQualifiedName(
                SyntaxFactory.IdentifierName(
                    SyntaxFactory.Token(SyntaxKind.GlobalKeyword)),
                                                                SyntaxFactory.IdentifierName(nameof(NDProperty))),
                                                            SyntaxFactory.GenericName(
                                                                SyntaxFactory.Identifier(propertyClass))
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
                                                                SyntaxFactory.EqualsValueClause(register)))))
                                                .WithModifiers(
                                                    SyntaxFactory.TokenList(
                                                        new[]{
                                SyntaxFactory.Token(accesibility),
                                SyntaxFactory.Token(SyntaxKind.StaticKeyword),
                                SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)}));


        }

        private static SyntaxKind ToAccessibilitySyntax(Accessibility filedAccessibility)
        {
            SyntaxKind accesibility;
            switch (filedAccessibility)
            {
                case Accessibility.Private:
                    accesibility = SyntaxKind.PrivateKeyword;
                    break;
                //case Accessibility.ProtectedAndFriend:
                case Accessibility.Protected:
                    accesibility = SyntaxKind.ProtectedKeyword;
                    break;
                //case Accessibility.Friend:
                case Accessibility.Internal:
                    accesibility = SyntaxKind.InternalKeyword;
                    break;
                case Accessibility.Public:
                    accesibility = SyntaxKind.PublicKeyword;
                    break;
                case Accessibility.ProtectedAndInternal:
                case Accessibility.ProtectedOrInternal:
                //case Accessibility.ProtectedOrFriend:
                case Accessibility.NotApplicable:
                default:
                    throw new ArgumentException($"Not supported access level {filedAccessibility}", nameof(filedAccessibility));
            }

            return accesibility;
        }
    }
}
