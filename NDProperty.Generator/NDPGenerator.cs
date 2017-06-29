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
    public class NDPGeneratorProperty : NDPGenerator
    {
        public NDPGeneratorProperty(AttributeData attributeData) : base(attributeData) { }

        public NDPGeneratorProperty() { }


        /// <summary>
        /// Method must be named after Convention
        /// </summary>
        public static readonly DiagnosticDescriptor methodNameConvention = new DiagnosticDescriptor(NDP0001, "Method must be named after Convention", "Method must be named after Convention 'On<Property name>Changed.", "NDP", DiagnosticSeverity.Error, true);
        /// <summary>
        /// Wrong Parameter
        /// </summary>
        public static readonly DiagnosticDescriptor wrongParameter = new DiagnosticDescriptor(NDP0002, "Wrong Parameter", $"The method must have a singel parameter of the type {typeof(OnChangedArg<>).FullName}.", "NDP", DiagnosticSeverity.Error, true);
        /// <summary>
        /// Class is not partial
        /// </summary>
        public static readonly DiagnosticDescriptor classNotPartial = new DiagnosticDescriptor(NDP0003, "Class is not partial", $"The containing class must be partial.", "NDP", DiagnosticSeverity.Error, true);
        /// <summary>
        /// Generator could not find Class
        /// </summary>
        public static readonly DiagnosticDescriptor classNotFound = new DiagnosticDescriptor(NDP0004, "Generator could not find Class", $"The Attribute must be applied to a member of an class.", "NDP", DiagnosticSeverity.Error, true);

        public override DiagnosticDescriptor MethodNameConvention => methodNameConvention;

        public override DiagnosticDescriptor WrongParameter => wrongParameter;

        public override DiagnosticDescriptor ClassNotPartial => classNotPartial;

        public override DiagnosticDescriptor ClassNotFound => classNotFound;

        public override Type OnChangedArgs => typeof(OnChangedArg<>);


        protected override SyntaxList<MemberDeclarationSyntax> GenerateProperty(MethodDeclarationSyntax method, SemanticModel semanticModel, bool isReadOnly)
        {

            var originalClassDeclaration = method.Parent as ClassDeclarationSyntax;
            var className = SyntaxFactory.IdentifierName(originalClassDeclaration.Identifier.Text);

            var nameMatch = nameRegex.Match(method.Identifier.Text);
            var propertyName = nameMatch.Groups["name"].Value;

            var genericType = method.ParameterList.Parameters.First().Type.DescendantNodesAndSelf().OfType<GenericNameSyntax>().First();
            var genericTypeArgument = genericType.TypeArgumentList.Arguments.First();
            var defaultValueExpresion = GetDefaultSyntax(method, semanticModel, genericTypeArgument);

            var list = new System.Collections.Generic.List<MemberDeclarationSyntax>();
            if (isReadOnly)
            {
                list.Add(GeneratePropertyKey(propertyName, defaultValueExpresion, className, Accessibility.Private, genericTypeArgument, PropertyKind.Normal));
                list.Add(GeneratePropertyKey(propertyName, defaultValueExpresion, className, Accessibility.Public, genericTypeArgument, PropertyKind.Readonly));
            }
            else
            {
                list.Add(GeneratePropertyKey(propertyName, defaultValueExpresion, className, Accessibility.Public, genericTypeArgument, PropertyKind.Normal));
            }

            list.Add(GeneratePropertyProperty(propertyName, genericTypeArgument, isReadOnly ? Accessibility.Private : Accessibility.NotApplicable));
            list.Add(GenerateEvent(propertyName, className, genericTypeArgument));
            return SyntaxFactory.List(list);
        }



        private MemberDeclarationSyntax GenerateEvent(string propertyName, TypeSyntax className, TypeSyntax genericTypeArgument)
        {
            var propertyKey = GetPropertyKey(propertyName);
            var propertyEvent = GetPropertyEvent(propertyName);
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
                                                        genericTypeArgument,
                                                        SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                        className}))))))),
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

        private MemberDeclarationSyntax GeneratePropertyProperty(string propertyName, TypeSyntax genericTypeArgument, Accessibility setterAccessibility = Accessibility.NotApplicable)
        {
            var propertyKey = GetPropertyKey(propertyName);
            var setterModifier = SyntaxFactory.TokenList();
            if (setterAccessibility != Accessibility.NotApplicable)
                setterModifier = setterModifier.Add(SyntaxFactory.Token(ToAccessibilitySyntax(setterAccessibility)));
            return SyntaxFactory.PropertyDeclaration(
                         genericTypeArgument,
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
    }

    public class NDPGeneratorAttachedProperty : NDPGenerator
    {
        public NDPGeneratorAttachedProperty(AttributeData attributeData) : base(attributeData) { }

        public NDPGeneratorAttachedProperty() { }

        /// <summary>
        /// Method must be named after Convention
        /// </summary>
        public static readonly DiagnosticDescriptor methodNameConvention = new DiagnosticDescriptor(NDP0005, "Method must be named after Convention", "Method must be named after Convention 'On<Property name>Changed.", "NDP", DiagnosticSeverity.Error, true);
        /// <summary>
        /// Wrong Parameter
        /// </summary>
        public static readonly DiagnosticDescriptor wrongParameter = new DiagnosticDescriptor(NDP0006, "Wrong Parameter", $"The method must have a singel parameter of the type {typeof(OnChangedArg<,>).FullName}.", "NDP", DiagnosticSeverity.Error, true);
        /// <summary>
        /// Class is not partial
        /// </summary>
        public static readonly DiagnosticDescriptor classNotPartial = new DiagnosticDescriptor(NDP0007, "Class is not partial", $"The containing class must be partial.", "NDP", DiagnosticSeverity.Error, true);
        /// <summary>
        /// Generator could not find Class
        /// </summary>
        public static readonly DiagnosticDescriptor classNotFound = new DiagnosticDescriptor(NDP0008, "Generator could not find Class", $"The Attribute must be applied to a member of an class.", "NDP", DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor notStatic = new DiagnosticDescriptor(NDP0009, "Change handler must be static", $"The Change handler of an Attached Property must be static.", "NDP", DiagnosticSeverity.Error, true);

        public override DiagnosticDescriptor MethodNameConvention => methodNameConvention;

        public override DiagnosticDescriptor WrongParameter => wrongParameter;

        public override DiagnosticDescriptor ClassNotPartial => classNotPartial;

        public override DiagnosticDescriptor ClassNotFound => classNotFound;

        public DiagnosticDescriptor NotStatic => notStatic;

        public override Type OnChangedArgs => typeof(OnChangedArg<,>);

        protected override SyntaxList<MemberDeclarationSyntax> GenerateProperty(MethodDeclarationSyntax method, SemanticModel semanticModel, bool isReadOnly)
        {
            var propertyName = GetPropertyName(method);

            var genericType = method.ParameterList.Parameters.First().Type.DescendantNodesAndSelf().OfType<GenericNameSyntax>().First();
            var genericValueType = genericType.TypeArgumentList.Arguments[0];
            var genericTypeType = genericType.TypeArgumentList.Arguments[1];

            var defaultValueExpresion = GetDefaultSyntax(method, semanticModel, genericValueType);

            var list = new System.Collections.Generic.List<MemberDeclarationSyntax>();
            if (isReadOnly)
            {
                list.Add(GeneratePropertyKey(propertyName, defaultValueExpresion, genericTypeType, Accessibility.Private, genericValueType, PropertyKind.Attached));
                list.Add(GeneratePropertyKey(propertyName, defaultValueExpresion, genericTypeType, Accessibility.Public, genericValueType, PropertyKind.Readonly));
            }
            else
            {
                list.Add(GeneratePropertyKey(propertyName, defaultValueExpresion, genericTypeType, Accessibility.Public, genericValueType, PropertyKind.Attached));
            }

            list.Add(GenerateHelper(propertyName, genericTypeType, genericValueType));
            return SyntaxFactory.List(list);
        }

        public override IEnumerable<Diagnostic> GenerateDiagnostics(MethodDeclarationSyntax method, SemanticModel model)
        {
            foreach (var diagnostic in base.GenerateDiagnostics(method, model))
            {
                yield return diagnostic;
            }
            if (!method.Modifiers.Any(SyntaxKind.StaticKeyword))
                yield return Diagnostic.Create(NotStatic, method.Identifier.GetLocation());

        }


        private MemberDeclarationSyntax GenerateHelper(string propertyName, TypeSyntax genericTypeType, TypeSyntax genericValueType)
        {
            var propertyKey = GetPropertyKey(propertyName);
            return SyntaxFactory.PropertyDeclaration(
             SyntaxFactory.QualifiedName(
                 SyntaxFactory.QualifiedName(
                     SyntaxFactory.AliasQualifiedName(
                         SyntaxFactory.IdentifierName(
                             SyntaxFactory.Token(SyntaxKind.GlobalKeyword)),
                         SyntaxFactory.IdentifierName(nameof(NDProperty))),
                     SyntaxFactory.IdentifierName(nameof(PropertyRegistar))),
                 SyntaxFactory.GenericName(
                     SyntaxFactory.Identifier(nameof(PropertyRegistar.AttachedHelper<object, object>)))
                 .WithTypeArgumentList(
                     SyntaxFactory.TypeArgumentList(
                         SyntaxFactory.SeparatedList<TypeSyntax>(
                             new SyntaxNodeOrToken[]{
                                genericValueType,
                                SyntaxFactory.Token(SyntaxKind.CommaToken),
                                genericTypeType})))),
             SyntaxFactory.Identifier(propertyName))
         .WithModifiers(
             SyntaxFactory.TokenList(
                 new[]{
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.StaticKeyword)}))
         .WithAccessorList(
             SyntaxFactory.AccessorList(
                 SyntaxFactory.SingletonList(
                     SyntaxFactory.AccessorDeclaration(
                         SyntaxKind.GetAccessorDeclaration)
                     .WithSemicolonToken(
                         SyntaxFactory.Token(SyntaxKind.SemicolonToken)))))
         .WithInitializer(
             SyntaxFactory.EqualsValueClause(
                 SyntaxFactory.InvocationExpression(
                     SyntaxFactory.MemberAccessExpression(
                         SyntaxKind.SimpleMemberAccessExpression,
                         SyntaxFactory.MemberAccessExpression(
                             SyntaxKind.SimpleMemberAccessExpression,
                             SyntaxFactory.MemberAccessExpression(
                                 SyntaxKind.SimpleMemberAccessExpression,
                                 SyntaxFactory.AliasQualifiedName(
                                     SyntaxFactory.IdentifierName(
                                         SyntaxFactory.Token(SyntaxKind.GlobalKeyword)),
                                     SyntaxFactory.IdentifierName(nameof(NDProperty))),
                                 SyntaxFactory.IdentifierName(nameof(PropertyRegistar))),
                             SyntaxFactory.IdentifierName(nameof(PropertyRegistar.AttachedHelper))),
                         SyntaxFactory.IdentifierName(nameof(PropertyRegistar.AttachedHelper.Create))))
                 .WithArgumentList(
                     SyntaxFactory.ArgumentList(
                         SyntaxFactory.SingletonSeparatedList(
                             SyntaxFactory.Argument(
                                 SyntaxFactory.IdentifierName(propertyKey)))))))
         .WithSemicolonToken(
             SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }
    }
    public abstract class NDPGenerator : ICodeGenerator
    {
        /// <summary>
        /// Method must be named after Convention. Normal Attribute.
        /// </summary>
        public const string NDP0001 = "NDP0001";
        /// <summary>
        /// Wrong Parameter. Normal Attribute.
        /// </summary>
        public const string NDP0002 = "NDP0002";
        /// <summary>
        /// Class is not partial. Normal Attribute.
        /// </summary>
        public const string NDP0003 = "NDP0003";
        /// <summary>
        /// Generator could not find Class. Normal Attribute.
        /// </summary>
        public const string NDP0004 = "NDP0004";


        /// <summary>
        /// Method must be named after Convention. Normal Attribute.
        /// </summary>
        public const string NDP0005 = "NDP0005";
        /// <summary>
        /// Wrong Parameter. Normal Attribute.
        /// </summary>
        public const string NDP0006 = "NDP0006";
        /// <summary>
        /// Class is not partial. Normal Attribute.
        /// </summary>
        public const string NDP0007 = "NDP0007";
        /// <summary>
        /// Generator could not find Class. Normal Attribute.
        /// </summary>
        public const string NDP0008 = "NDP0008";


        /// <summary>
        /// Attached Change handler must be static
        /// </summary>
        public const string NDP0009 = "NDP0009";

        /// <summary>
        /// Default value has wrong Type
        /// </summary>
        public const string NDP0010 = "NDP0010";




        public abstract Type OnChangedArgs { get; }

        public abstract DiagnosticDescriptor MethodNameConvention { get; }
        /// <summary>
        /// Wrong Parameter
        /// </summary>
        public abstract DiagnosticDescriptor WrongParameter { get; }
        /// <summary>
        /// Class is not partial
        /// </summary>
        public abstract DiagnosticDescriptor ClassNotPartial { get; }
        /// <summary>
        /// Generator could not find Class
        /// </summary>
        public abstract DiagnosticDescriptor ClassNotFound { get; }

        public static readonly DiagnosticDescriptor defaultValueWrongType = new DiagnosticDescriptor(NDP0010, "The Default Value could not be cast to correct Type", "The value must be assignable to the Property Type. (implicite or explicite)", "NDP", DiagnosticSeverity.Error, true);
        public DiagnosticDescriptor DefaultValueWrongType => defaultValueWrongType;



        protected readonly bool inherited;
        protected readonly bool isReadOnly;
        protected readonly bool isParentReference;
        private readonly NDPropertySettings propertySettings;
        protected static readonly Regex nameRegex = new Regex(@"On(?<name>\S+)Changed", RegexOptions.Compiled);

        internal NDPGenerator(AttributeData attributeData)
        {
            Requires.NotNull(attributeData, nameof(attributeData));

            var d = attributeData.NamedArguments.ToDictionary(x => x.Key, x => x.Value);

            this.propertySettings = d.ContainsKey("Settigns") ? (NDPropertySettings)d["Settigns"].Value : NDPropertySettings.None;
            this.isReadOnly = this.propertySettings.HasFlag(NDPropertySettings.ReadOnly);
            this.inherited = this.propertySettings.HasFlag(NDPropertySettings.Inherited);
            this.isParentReference = this.propertySettings.HasFlag(NDPropertySettings.ParentReference);
        }
        internal NDPGenerator() { }
        [System.ComponentModel.DefaultValue("")]
        public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(MemberDeclarationSyntax applyTo, CSharpCompilation compilation, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            var method = applyTo as MethodDeclarationSyntax;
            var semanticModel = compilation.GetSemanticModel(method.SyntaxTree, true);
            var diagnostics = GenerateDiagnostics(method, semanticModel);



            bool detectedError = false;

            foreach (var d in diagnostics)
            {
                detectedError = true;
                progress.Report(d);
            }
            if (detectedError)
                return Task.FromResult(SyntaxFactory.List<MemberDeclarationSyntax>());

            return Task.FromResult(GenerateProperty(method, semanticModel, this.isReadOnly));
        }

        public virtual IEnumerable<Diagnostic> GenerateDiagnostics(MethodDeclarationSyntax method, SemanticModel semanticModel)
        {
            var originalClassDeclaration = method.Parent as ClassDeclarationSyntax;

            if (originalClassDeclaration == null)
            {
                yield return Diagnostic.Create(ClassNotFound, originalClassDeclaration.Identifier.GetLocation());
                yield break; // no other diagnostics if this one
            }

            // check if class is partial
            if (!originalClassDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
                yield return Diagnostic.Create(ClassNotPartial, originalClassDeclaration.Identifier.GetLocation());

            bool problemWithParameter = false;
            if (method.ParameterList.Parameters.Count != 1) // No parameters, or more then one Mark Parameter List (includes parenthise)
            {
                yield return Diagnostic.Create(WrongParameter, method.ParameterList.GetLocation());
                problemWithParameter = true;
            }
            else // One Parameter, Check type
            {
                var changedMethodParameter = method.ParameterList.Parameters.FirstOrDefault();
                var typeInfo = Microsoft.CodeAnalysis.CSharp.CSharpExtensions.GetTypeInfo(semanticModel, (ExpressionSyntax)changedMethodParameter.Type);
                if (!TypeSymbolMatchesType(typeInfo.ConvertedType, OnChangedArgs, (SemanticModel)semanticModel, false))
                {
                    yield return Diagnostic.Create(WrongParameter, changedMethodParameter.Type.GetLocation());
                    problemWithParameter = true;
                }
            }

            var nameMatch = nameRegex.Match(method.Identifier.Text);
            if (!nameMatch.Success)
                yield return Diagnostic.Create(MethodNameConvention, method.Identifier.GetLocation());

            ////////////////////////////////////////
            var defaultAttribute = method.AttributeLists.SelectMany(x => x.Attributes).FirstOrDefault(attribute =>
            {
                var attributeType = Microsoft.CodeAnalysis.CSharp.CSharpExtensions.GetTypeInfo(semanticModel, (AttributeSyntax)attribute);
                return TypeSymbolMatchesType(attributeType.ConvertedType, typeof(System.ComponentModel.DefaultValueAttribute), semanticModel);
            });

            if (defaultAttribute != null && !problemWithParameter)
            {

                var valueType = method.ParameterList.Parameters.First().Type.DescendantNodesAndSelf().OfType<GenericNameSyntax>().First().TypeArgumentList.Arguments.First();

                ExpressionSyntax defaultValueExpresion = null;
                if (defaultAttribute != null && defaultAttribute.ArgumentList.Arguments.Count > 0)
                {
                    //if (!System.Diagnostics.Debugger.IsAttached)
                    //    System.Diagnostics.Debugger.Launch();
                    defaultValueExpresion = defaultAttribute.ArgumentList.Arguments.First().Expression;
                    var defaultTypeInfo = Microsoft.CodeAnalysis.CSharp.CSharpExtensions.GetTypeInfo(semanticModel, (ExpressionSyntax)defaultValueExpresion);
                    var valueTypeInfo = semanticModel.GetTypeInfo(valueType);
                    var conversion = Microsoft.CodeAnalysis.CSharp.CSharpExtensions.ClassifyConversion(semanticModel.Compilation, defaultTypeInfo.Type, valueTypeInfo.Type);
                    if (!conversion.Exists)
                        yield return Diagnostic.Create(DefaultValueWrongType, defaultValueExpresion.GetLocation());

                }

            }
        }


        public static bool TypeSymbolMatchesType(ITypeSymbol typeSymbol, Type type, SemanticModel semanticModel, bool expandGeneric = true)
        {
            if (!expandGeneric)
                typeSymbol = (typeSymbol as INamedTypeSymbol)?.ConstructedFrom ?? typeSymbol;

            return GetTypeSymbolForType(type, semanticModel, expandGeneric).Equals(typeSymbol);
        }

        private static INamedTypeSymbol GetTypeSymbolForType(Type type, SemanticModel semanticModel, bool expandGeneric)
        {

            if (!type.IsConstructedGenericType || !expandGeneric)
                return semanticModel.Compilation.GetTypeByMetadataName(type.FullName);

            // get all typeInfo's for the Type arguments 
            var typeArgumentsTypeInfos = type.GenericTypeArguments.Select(a => GetTypeSymbolForType(a, semanticModel, true));

            var openType = type.GetGenericTypeDefinition();
            var typeSymbol = semanticModel.Compilation.GetTypeByMetadataName(openType.FullName);
            return typeSymbol.Construct(typeArgumentsTypeInfos.ToArray<ITypeSymbol>());
        }

        protected abstract SyntaxList<MemberDeclarationSyntax> GenerateProperty(MethodDeclarationSyntax method, SemanticModel semanticModel, bool isReadOnly);

        protected enum PropertyKind
        {
            Normal,
            Readonly,
            Attached
        }
        protected static SyntaxKind ToAccessibilitySyntax(Accessibility filedAccessibility)
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

        protected static FieldDeclarationSyntax GenerateLeftKeyPart(TypeSyntax className, string propertyKey, ExpressionSyntax register, Accessibility filedAccessibility, PropertyKind kind, TypeSyntax genericTypeArgument)
        {
            SyntaxKind accesibility;

            accesibility = ToAccessibilitySyntax(filedAccessibility);

            string propertyClass;
            switch (kind)
            {
                case PropertyKind.Normal:
                    propertyClass = nameof(NDPropertyKey<object, object>);
                    break;
                case PropertyKind.Readonly:
                    propertyClass = nameof(NDReadOnlyPropertyKey<object, object>);
                    break;
                case PropertyKind.Attached:
                    propertyClass = nameof(NDAttachedPropertyKey<object, object>);
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
                                             genericTypeArgument,
                                                SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                className})))))
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
        private MemberDeclarationSyntax GenerateReadOnlyPropertyKey(string propertyName, TypeSyntax className, string propertyChangedMethod, TypeSyntax genericTypeArgument)
        {
            var propertyReadOnlyKey = GetReadOnlyPropertyKey(propertyName);
            var propertyKey = GetPropertyKey(propertyName);

            var register = SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName(propertyKey),
                                SyntaxFactory.IdentifierName(nameof(INDProperty<object, object>.ReadOnlyProperty)));

            return GenerateLeftKeyPart(className, propertyReadOnlyKey, register, Accessibility.Public, PropertyKind.Readonly, genericTypeArgument);
        }

        protected static string GetReadOnlyPropertyKey(string propertyName)
        {
            return propertyName + "ReadOnlyProperty";
        }

        protected static string GetPropertyKey(string propertyName)
        {
            return propertyName + "Property";
        }
        protected static string GetPropertyEvent(string propertyName)
        {
            return propertyName + "Changed";
        }

        protected static string GetChangedHandler(string propertyName)
        {
            return $"On{propertyName}Changed";
        }

        protected MemberDeclarationSyntax GeneratePropertyKey(string propertyName, ExpressionSyntax defalutExpresion, TypeSyntax className, Accessibility filedAccessibility, TypeSyntax propertyValue, PropertyKind kind)
        {

            //Callback
            ExpressionSyntax callback;
            var propertyKey = GetPropertyKey(propertyName);
            var propertyChangedMethod = GetChangedHandler(propertyName);
            string registerMethod;
            switch (kind)
            {
                case PropertyKind.Normal:
                    callback = SyntaxFactory.SimpleLambdaExpression(
                        SyntaxFactory.Parameter(
                            SyntaxFactory.Identifier("t")),
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("t"),
                            SyntaxFactory.IdentifierName(propertyChangedMethod)));

                    registerMethod = nameof(PropertyRegistar.Register);
                    break;
                case PropertyKind.Readonly:
                    return GenerateReadOnlyPropertyKey(propertyName, className, propertyChangedMethod, propertyValue);
                case PropertyKind.Attached:
                    callback = SyntaxFactory.IdentifierName(propertyChangedMethod);
                    registerMethod = nameof(PropertyRegistar.RegisterAttached);
                    break;
                default:
                    throw new NotSupportedException($"The PropertyKind: {kind} is not supported.");
            }


            // Build Settings Parameter
            ExpressionSyntax settingsSyntax = null;
            foreach (NDPropertySettings item in Enum.GetValues(typeof(NDPropertySettings)))
            {
                if (item == NDPropertySettings.None)
                    continue;

                if (this.propertySettings.HasFlag(item))
                {
                    if (settingsSyntax == null)
                        settingsSyntax = SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        SyntaxFactory.AliasQualifiedName(
                                                            SyntaxFactory.IdentifierName(
                                                                SyntaxFactory.Token(SyntaxKind.GlobalKeyword)),
                                                            SyntaxFactory.IdentifierName(nameof(NDProperty))),
                                                        SyntaxFactory.IdentifierName(nameof(NDPropertySettings))),
                                                    SyntaxFactory.IdentifierName(item.ToString()));
                    else
                        settingsSyntax = SyntaxFactory.BinaryExpression(
                            SyntaxKind.BitwiseOrExpression,
                            settingsSyntax,
                            SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        SyntaxFactory.AliasQualifiedName(
                                                            SyntaxFactory.IdentifierName(
                                                                SyntaxFactory.Token(SyntaxKind.GlobalKeyword)),
                                                            SyntaxFactory.IdentifierName(nameof(NDProperty))),
                                                        SyntaxFactory.IdentifierName(nameof(NDPropertySettings))),
                                                    SyntaxFactory.IdentifierName(item.ToString())));
                }
            }
            if (settingsSyntax == null)
            {
                settingsSyntax = SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        SyntaxFactory.AliasQualifiedName(
                                                            SyntaxFactory.IdentifierName(
                                                                SyntaxFactory.Token(SyntaxKind.GlobalKeyword)),
                                                            SyntaxFactory.IdentifierName(nameof(NDProperty))),
                                                        SyntaxFactory.IdentifierName(nameof(NDPropertySettings))),
                                                    SyntaxFactory.IdentifierName(nameof(NDPropertySettings.None)));
            }
            
            // Put everything together
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
                        SyntaxFactory.Identifier(registerMethod))
                        .WithTypeArgumentList(
                        SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SeparatedList<TypeSyntax>(
                                new SyntaxNodeOrToken[]{
                               propertyValue,
                                    SyntaxFactory.Token(SyntaxKind.CommaToken),
                                    className})))))
                                    .WithArgumentList(
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList<ArgumentSyntax>(
                        new SyntaxNodeOrToken[]{
                            SyntaxFactory.Argument(callback),
                            SyntaxFactory.Token(SyntaxKind.CommaToken),
                            SyntaxFactory.Argument(defalutExpresion),
                            SyntaxFactory.Token(SyntaxKind.CommaToken),
                            SyntaxFactory.Argument(settingsSyntax)})));

            return GenerateLeftKeyPart(className, propertyKey, register, filedAccessibility, kind, propertyValue);
        }

        protected static ExpressionSyntax GetDefaultSyntax(MethodDeclarationSyntax method, SemanticModel semanticModel, TypeSyntax valueType)
        {
            var defaultAttribute = method.AttributeLists.SelectMany(x => x.Attributes).FirstOrDefault(attribute =>
            {
                var attributeType = semanticModel.GetTypeInfo(attribute);
                return TypeSymbolMatchesType(attributeType.ConvertedType, typeof(System.ComponentModel.DefaultValueAttribute), semanticModel);
            });


            ExpressionSyntax defaultValueExpresion = null;
            if (defaultAttribute != null)
            {
                defaultValueExpresion = defaultAttribute.ArgumentList.Arguments.First().Expression;
                var defaultTypeInfo = semanticModel.GetTypeInfo(defaultValueExpresion);
                var valueTypeInfo = semanticModel.GetTypeInfo(valueType);
                var conversion = semanticModel.Compilation.ClassifyConversion(defaultTypeInfo.Type, valueTypeInfo.Type);
                if (!conversion.Exists)
                    throw new Exception("Converstion Fail");

                if (conversion.IsExplicit)
                {
                    defaultValueExpresion = SyntaxFactory.CastExpression(valueType, defaultValueExpresion);
                }
            }
            else
                defaultValueExpresion = SyntaxFactory.DefaultExpression(valueType);
            return defaultValueExpresion;
        }

        protected static string GetPropertyName(MethodDeclarationSyntax method)
        {
            var nameMatch = nameRegex.Match(method.Identifier.Text);
            var propertyName = nameMatch.Groups["name"].Value;
#if DEBUG
            var calculatedChangeHandler = GetChangedHandler(propertyName);
            System.Diagnostics.Debug.Assert(method.Identifier.Text == calculatedChangeHandler, $"Problem Getting Property name. (PropertyName:{propertyName}, ChangedHandler:{method.Identifier.Text}, CalculatedChangeHandler:{calculatedChangeHandler})");
#endif
            return propertyName;
        }

    }
}
