using System;
using System.Collections.Generic;
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
        /// <summary>
        /// Method must be named after Convention
        /// </summary>
        public const string NDP0001 = "NDP0001";
        /// <summary>
        /// Wrong Parameter
        /// </summary>
        public const string NDP0002 = "NDP0002";
        /// <summary>
        /// Class is not partial
        /// </summary>
        public const string NDP0003 = "NDP0004";
        /// <summary>
        /// Generator could not find Class
        /// </summary>
        public const string NDP0004 = "NDP0005";

        /// <summary>
        /// Method must be named after Convention
        /// </summary>
        public static readonly DiagnosticDescriptor RuleNDP0001 = new DiagnosticDescriptor(NDP0001, "Method must be named after Convention", "Method must be named after Convention 'On<Property name>Changed.", "NDP", DiagnosticSeverity.Error, true);
        /// <summary>
        /// Wrong Parameter
        /// </summary>
        public static readonly DiagnosticDescriptor RuleNDP0002 = new DiagnosticDescriptor(NDP0002, "Wrong Parameter", $"The method must have a singel parameter of the type {typeof(OnChangedArg).FullName}.", "NDP", DiagnosticSeverity.Error, true);
        /// <summary>
        /// Class is not partial
        /// </summary>
        public static readonly DiagnosticDescriptor RuleNDP0003 = new DiagnosticDescriptor(NDP0003, "Class is not partial", $"The containing class must be partial.", "NDP", DiagnosticSeverity.Error, true);
        /// <summary>
        /// Generator could not find Class
        /// </summary>
        public static readonly DiagnosticDescriptor RuleNDP0004 = new DiagnosticDescriptor(NDP0004, "Generator could not find Class", $"The Attribute must be applied to a member of an class.", "NDP", DiagnosticSeverity.Error, true);


        private readonly bool inherited;
        private readonly NullTreatment nullTreatment;
        private readonly bool isReadOnly;
        private readonly bool isParentReference;
        private static readonly Regex nameRegex = new Regex(@"On(?<name>\S+)Changed", RegexOptions.Compiled);

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
            var method = applyTo as MethodDeclarationSyntax;
            var diagnostics = GenerateDiagnostics(method);

            bool detectedError = false;

            foreach (var d in diagnostics)
            {
                detectedError = true;
                progress.Report(d);
            }
            if (detectedError)
                return Task.FromResult(SyntaxFactory.List<MemberDeclarationSyntax>());
            ClassDeclarationSyntax originalClassDeclaration;
            originalClassDeclaration = applyTo.Parent as ClassDeclarationSyntax;


            var nameMatch = nameRegex.Match(method.Identifier.Text);
            var propertyName = nameMatch.Groups["name"].Value;

            return Task.FromResult(GenerateProperty(propertyName, originalClassDeclaration.Identifier.Text, this.isReadOnly));
        }

        public static IEnumerable<Diagnostic> GenerateDiagnostics(MethodDeclarationSyntax method)
        {
            var originalClassDeclaration = method.Parent as ClassDeclarationSyntax;

            if (originalClassDeclaration == null)
            {
                yield return Diagnostic.Create(RuleNDP0004, originalClassDeclaration.Identifier.GetLocation());
                yield break; // no other diagnostics if this one
            }

            // check if class is partial
            if (!originalClassDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
                yield return Diagnostic.Create(RuleNDP0003, originalClassDeclaration.Identifier.GetLocation());


            var changedMethodParameter = method.ParameterList.Parameters.FirstOrDefault();
            var parameterType = changedMethodParameter?.Type as GenericNameSyntax;
            if (method.ParameterList.Parameters.Count != 1 || parameterType?.Identifier.Text != nameof(OnChangedArg))
                yield return Diagnostic.Create(RuleNDP0002, method.ParameterList.Parameters.Count != 1 ? method.ParameterList.GetLocation() : changedMethodParameter.Identifier.GetLocation());

            var nameMatch = nameRegex.Match(method.Identifier.Text);
            if (!nameMatch.Success)
                yield return Diagnostic.Create(RuleNDP0001, method.Identifier.GetLocation());
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

            list.Add(GeneratePropertyProperty(propertyName, propertyKey, isReadOnly ? Accessibility.Private : Accessibility.NotApplicable));
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
