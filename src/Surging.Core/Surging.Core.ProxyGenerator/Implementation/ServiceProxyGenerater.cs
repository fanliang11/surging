using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Convertibles;
using Surging.Core.CPlatform.Ids;
using Surging.Core.CPlatform.Runtime.Client;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.ProxyGenerator.Utilitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Surging.Core.ProxyGenerator.Implementation
{
    /// <summary>
    /// Defines the <see cref="ServiceProxyGenerater" />
    /// </summary>
    public class ServiceProxyGenerater : IServiceProxyGenerater, IDisposable
    {
        #region 字段

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger<ServiceProxyGenerater> _logger;

        /// <summary>
        /// Defines the _serviceIdGenerator
        /// </summary>
        private readonly IServiceIdGenerator _serviceIdGenerator;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProxyGenerater"/> class.
        /// </summary>
        /// <param name="serviceIdGenerator">The serviceIdGenerator<see cref="IServiceIdGenerator"/></param>
        /// <param name="logger">The logger<see cref="ILogger{ServiceProxyGenerater}"/></param>
        public ServiceProxyGenerater(IServiceIdGenerator serviceIdGenerator, ILogger<ServiceProxyGenerater> logger)
        {
            _serviceIdGenerator = serviceIdGenerator;
            _logger = logger;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Dispose
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 生成服务代理。
        /// </summary>
        /// <param name="interfacTypes">需要被代理的接口类型。</param>
        /// <param name="namespaces">The namespaces<see cref="IEnumerable{string}"/></param>
        /// <returns>服务代理实现。</returns>
        public IEnumerable<Type> GenerateProxys(IEnumerable<Type> interfacTypes, IEnumerable<string> namespaces)
        {
#if NET
            var assemblys = AppDomain.CurrentDomain.GetAssemblies();
#else
            var assemblys = DependencyContext.Default.RuntimeLibraries.SelectMany(i => i.GetDefaultAssemblyNames(DependencyContext.Default).Select(z => Assembly.Load(new AssemblyName(z.Name))));
#endif
            assemblys = assemblys.Where(i => i.IsDynamic == false).ToArray();
            var types = assemblys.Select(p => p.GetType());
            types = interfacTypes.Except(types);
            foreach (var t in types)
            {
                assemblys = assemblys.Append(t.Assembly);
            }
            var trees = interfacTypes.Select(p => GenerateProxyTree(p, namespaces)).ToList();
            var stream = CompilationUtilitys.CompileClientProxy(trees,
                assemblys
                    .Select(a => MetadataReference.CreateFromFile(a.Location))
                    .Concat(new[]
                    {
                        MetadataReference.CreateFromFile(typeof(Task).GetTypeInfo().Assembly.Location)
                    }),
                _logger);

            using (stream)
            {
#if NET
                var assembly = Assembly.Load(stream.ToArray());
#else
                var assembly = AssemblyLoadContext.Default.LoadFromStream(stream);
#endif
                return assembly.GetExportedTypes();
            }
        }

        /// <summary>
        /// 生成服务代理代码树。
        /// </summary>
        /// <param name="interfaceType">需要被代理的接口类型。</param>
        /// <param name="namespaces">The namespaces<see cref="IEnumerable{string}"/></param>
        /// <returns>代码树。</returns>
        public SyntaxTree GenerateProxyTree(Type interfaceType, IEnumerable<string> namespaces)
        {
            var className = interfaceType.Name.StartsWith("I") ? interfaceType.Name.Substring(1) : interfaceType.Name;
            className += "ClientProxy";

            var members = new List<MemberDeclarationSyntax>
            {
                GetConstructorDeclaration(className)
            };

            members.AddRange(GenerateMethodDeclarations(interfaceType.GetMethods()));
            return CompilationUnit()
                .WithUsings(GetUsings(namespaces))
                .WithMembers(
                    SingletonList<MemberDeclarationSyntax>(
                        NamespaceDeclaration(
                            QualifiedName(
                                QualifiedName(
                                    IdentifierName("Surging"),
                                    IdentifierName("Cores")),
                                IdentifierName("ClientProxys")))
                .WithMembers(
                    SingletonList<MemberDeclarationSyntax>(
                        ClassDeclaration(className)
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                            .WithBaseList(
                                BaseList(
                                    SeparatedList<BaseTypeSyntax>(
                                        new SyntaxNodeOrToken[]
                                        {
                                            SimpleBaseType(IdentifierName("ServiceProxyBase")),
                                            Token(SyntaxKind.CommaToken),
                                            SimpleBaseType(GetQualifiedNameSyntax(interfaceType))
                                        })))
                            .WithMembers(List(members))))))
                .NormalizeWhitespace().SyntaxTree;
        }

        /// <summary>
        /// The GetConstructorDeclaration
        /// </summary>
        /// <param name="className">The className<see cref="string"/></param>
        /// <returns>The <see cref="ConstructorDeclarationSyntax"/></returns>
        private static ConstructorDeclarationSyntax GetConstructorDeclaration(string className)
        {
            return ConstructorDeclaration(Identifier(className))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(
                    ParameterList(
                        SeparatedList<ParameterSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                Parameter(
                                    Identifier("remoteInvokeService"))
                                    .WithType(
                                        IdentifierName("IRemoteInvokeService")),
                                Token(SyntaxKind.CommaToken),
                                Parameter(
                                    Identifier("typeConvertibleService"))
                                    .WithType(
                                        IdentifierName("ITypeConvertibleService")),
                                Token(SyntaxKind.CommaToken),
                                Parameter(
                                    Identifier("serviceKey"))
                                    .WithType(
                                        IdentifierName("String")),
                                 Token(SyntaxKind.CommaToken),
                                Parameter(
                                    Identifier("serviceProvider"))
                                    .WithType(
                                        IdentifierName("CPlatformContainer"))
                            })))
                .WithInitializer(
                        ConstructorInitializer(
                            SyntaxKind.BaseConstructorInitializer,
                            ArgumentList(
                                SeparatedList<ArgumentSyntax>(
                                    new SyntaxNodeOrToken[]{
                                        Argument(
                                            IdentifierName("remoteInvokeService")),
                                        Token(SyntaxKind.CommaToken),
                                        Argument(
                                            IdentifierName("typeConvertibleService")),
                                          Token(SyntaxKind.CommaToken),
                                        Argument(
                                            IdentifierName("serviceKey")),
                                           Token(SyntaxKind.CommaToken),
                                        Argument(
                                            IdentifierName("serviceProvider"))
                                    }))))
                .WithBody(Block());
        }

        /// <summary>
        /// The GetQualifiedNameSyntax
        /// </summary>
        /// <param name="names">The names<see cref="IReadOnlyCollection{string}"/></param>
        /// <returns>The <see cref="QualifiedNameSyntax"/></returns>
        private static QualifiedNameSyntax GetQualifiedNameSyntax(IReadOnlyCollection<string> names)
        {
            var ids = names.Select(IdentifierName).ToArray();

            var index = 0;
            QualifiedNameSyntax left = null;
            while (index + 1 < names.Count)
            {
                left = left == null ? QualifiedName(ids[index], ids[index + 1]) : QualifiedName(left, ids[index + 1]);
                index++;
            }
            return left;
        }

        /// <summary>
        /// The GetQualifiedNameSyntax
        /// </summary>
        /// <param name="fullName">The fullName<see cref="string"/></param>
        /// <returns>The <see cref="QualifiedNameSyntax"/></returns>
        private static QualifiedNameSyntax GetQualifiedNameSyntax(string fullName)
        {
            return GetQualifiedNameSyntax(fullName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries));
        }

        /// <summary>
        /// The GetQualifiedNameSyntax
        /// </summary>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <returns>The <see cref="QualifiedNameSyntax"/></returns>
        private static QualifiedNameSyntax GetQualifiedNameSyntax(Type type)
        {
            var fullName = type.Namespace + "." + type.Name;
            return GetQualifiedNameSyntax(fullName);
        }

        /// <summary>
        /// The GetTypeSyntax
        /// </summary>
        /// <param name="type">The type<see cref="Type"/></param>
        /// <returns>The <see cref="TypeSyntax"/></returns>
        private static TypeSyntax GetTypeSyntax(Type type)
        {
            //没有返回值。
            if (type == null)
                return null;

            //非泛型。
            if (!type.GetTypeInfo().IsGenericType)
                return GetQualifiedNameSyntax(type.FullName);

            var list = new List<SyntaxNodeOrToken>();

            foreach (var genericTypeArgument in type.GenericTypeArguments)
            {
                list.Add(genericTypeArgument.GetTypeInfo().IsGenericType
                    ? GetTypeSyntax(genericTypeArgument)
                    : GetQualifiedNameSyntax(genericTypeArgument.FullName));
                list.Add(Token(SyntaxKind.CommaToken));
            }

            var array = list.Take(list.Count - 1).ToArray();
            var typeArgumentListSyntax = TypeArgumentList(SeparatedList<TypeSyntax>(array));
            return GenericName(type.Name.Substring(0, type.Name.IndexOf('`')))
                .WithTypeArgumentList(typeArgumentListSyntax);
        }

        /// <summary>
        /// The GetUsings
        /// </summary>
        /// <param name="namespaces">The namespaces<see cref="IEnumerable{string}"/></param>
        /// <returns>The <see cref="SyntaxList{UsingDirectiveSyntax}"/></returns>
        private static SyntaxList<UsingDirectiveSyntax> GetUsings(IEnumerable<string> namespaces)
        {
            var directives = new List<UsingDirectiveSyntax>();
            foreach (var name in namespaces)
            {
                directives.Add(UsingDirective(GetQualifiedNameSyntax(name)));
            }
            return List(
                new[]
                {
                    UsingDirective(IdentifierName("System")),
                    UsingDirective(GetQualifiedNameSyntax("System.Threading.Tasks")),
                    UsingDirective(GetQualifiedNameSyntax("System.Collections.Generic")),
                    UsingDirective(GetQualifiedNameSyntax(typeof(ITypeConvertibleService).Namespace)),
                    UsingDirective(GetQualifiedNameSyntax(typeof(IRemoteInvokeService).Namespace)),
                    UsingDirective(GetQualifiedNameSyntax(typeof(CPlatformContainer).Namespace)),
                    UsingDirective(GetQualifiedNameSyntax(typeof(ISerializer<>).Namespace)),
                    UsingDirective(GetQualifiedNameSyntax(typeof(ServiceProxyBase).Namespace))
                }.Concat(directives));
        }

        /// <summary>
        /// The GenerateMethodDeclaration
        /// </summary>
        /// <param name="method">The method<see cref="MethodInfo"/></param>
        /// <returns>The <see cref="MemberDeclarationSyntax"/></returns>
        private MemberDeclarationSyntax GenerateMethodDeclaration(MethodInfo method)
        {
            var serviceId = _serviceIdGenerator.GenerateServiceId(method);
            var returnDeclaration = GetTypeSyntax(method.ReturnType);

            var parameterList = new List<SyntaxNodeOrToken>();
            var parameterDeclarationList = new List<SyntaxNodeOrToken>();

            foreach (var parameter in method.GetParameters())
            {
                if (parameter.ParameterType.IsGenericType)
                {
                    parameterDeclarationList.Add(Parameter(
                                     Identifier(parameter.Name))
                                     .WithType(GetTypeSyntax(parameter.ParameterType)));
                }
                else
                {
                    parameterDeclarationList.Add(Parameter(
                                        Identifier(parameter.Name))
                                        .WithType(GetQualifiedNameSyntax(parameter.ParameterType)));
                }
                parameterDeclarationList.Add(Token(SyntaxKind.CommaToken));

                parameterList.Add(InitializerExpression(
                    SyntaxKind.ComplexElementInitializerExpression,
                    SeparatedList<ExpressionSyntax>(
                        new SyntaxNodeOrToken[]{
                            LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                Literal(parameter.Name)),
                            Token(SyntaxKind.CommaToken),
                            IdentifierName(parameter.Name)})));
                parameterList.Add(Token(SyntaxKind.CommaToken));
            }
            if (parameterList.Any())
            {
                parameterList.RemoveAt(parameterList.Count - 1);
                parameterDeclarationList.RemoveAt(parameterDeclarationList.Count - 1);
            }

            var declaration = MethodDeclaration(
                returnDeclaration,
                Identifier(method.Name))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.AsyncKeyword)))
                .WithParameterList(ParameterList(SeparatedList<ParameterSyntax>(parameterDeclarationList)));

            ExpressionSyntax expressionSyntax;
            StatementSyntax statementSyntax;

            if (method.ReturnType != typeof(Task))
            {
                expressionSyntax = GenericName(
                Identifier("Invoke")).WithTypeArgumentList(((GenericNameSyntax)returnDeclaration).TypeArgumentList);
            }
            else
            {
                expressionSyntax = IdentifierName("Invoke");
            }
            expressionSyntax = AwaitExpression(
                InvocationExpression(expressionSyntax)
                    .WithArgumentList(
                        ArgumentList(
                            SeparatedList<ArgumentSyntax>(
                                new SyntaxNodeOrToken[]
                                {
                                        Argument(
                                            ObjectCreationExpression(
                                                GenericName(
                                                    Identifier("Dictionary"))
                                                    .WithTypeArgumentList(
                                                        TypeArgumentList(
                                                            SeparatedList<TypeSyntax>(
                                                                new SyntaxNodeOrToken[]
                                                                {
                                                                    PredefinedType(
                                                                        Token(SyntaxKind.StringKeyword)),
                                                                    Token(SyntaxKind.CommaToken),
                                                                    PredefinedType(
                                                                        Token(SyntaxKind.ObjectKeyword))
                                                                }))))
                                                .WithInitializer(
                                                    InitializerExpression(
                                                        SyntaxKind.CollectionInitializerExpression,
                                                        SeparatedList<ExpressionSyntax>(
                                                            parameterList)))),
                                        Token(SyntaxKind.CommaToken),
                                        Argument(
                                            LiteralExpression(
                                                SyntaxKind.StringLiteralExpression,
                                                Literal(serviceId)))
                                }))));

            if (method.ReturnType != typeof(Task))
            {
                statementSyntax = ReturnStatement(expressionSyntax);
            }
            else
            {
                statementSyntax = ExpressionStatement(expressionSyntax);
            }

            declaration = declaration.WithBody(
                        Block(
                            SingletonList(statementSyntax)));

            return declaration;
        }

        /// <summary>
        /// The GenerateMethodDeclarations
        /// </summary>
        /// <param name="methods">The methods<see cref="IEnumerable{MethodInfo}"/></param>
        /// <returns>The <see cref="IEnumerable{MemberDeclarationSyntax}"/></returns>
        private IEnumerable<MemberDeclarationSyntax> GenerateMethodDeclarations(IEnumerable<MethodInfo> methods)
        {
            var array = methods.ToArray();
            return array.Select(p => GenerateMethodDeclaration(p)).ToArray();
        }

        #endregion 方法
    }
}