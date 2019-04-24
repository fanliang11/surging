using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Runtime.Client;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.Extensions.DependencyInjection;

#if !NET

using Microsoft.Extensions.DependencyModel;

#endif

namespace Surging.Core.ProxyGenerator.Utilitys
{
    public static class CompilationUtilitys
    {
        #region Public Method

        public static MemoryStream CompileClientProxy(IEnumerable<SyntaxTree> trees, IEnumerable<MetadataReference> references, ILogger logger = null)
        {
#if !NET
            var assemblys = new[]
            {
                "System.Runtime",
                "mscorlib",
                "System.Threading.Tasks",
                 "System.Collections"
            };
            references = assemblys.Select(i => MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName(i)).Location)).Concat(references);
#endif
            references = new[]
            {
                MetadataReference.CreateFromFile(typeof(Task).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ServiceDescriptor).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IRemoteInvokeService).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IServiceProxyGenerater).GetTypeInfo().Assembly.Location)
            }.Concat(references);
            return Compile(AssemblyInfo.Create("Surging.Cores.ClientProxys"), trees, references, logger);
        }

        public static MemoryStream Compile(AssemblyInfo assemblyInfo, IEnumerable<SyntaxTree> trees, IEnumerable<MetadataReference> references, ILogger logger = null)
        {
            return Compile(assemblyInfo.Title, assemblyInfo, trees, references, logger);
        }

        public static MemoryStream Compile(string assemblyName, AssemblyInfo assemblyInfo, IEnumerable<SyntaxTree> trees, IEnumerable<MetadataReference> references, ILogger logger = null)
        {
            trees = trees.Concat(new[] { GetAssemblyInfo(assemblyInfo) });
            var compilation = CSharpCompilation.Create(assemblyName, trees, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            var stream = new MemoryStream();
            var result = compilation.Emit(stream);
            if (!result.Success && logger != null)
            {
                foreach (var message in result.Diagnostics.Select(i => i.ToString()))
                {
                    logger.LogError(message);
                }
                return null;
            }
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        #endregion Public Method

        #region Private Method

        private static SyntaxTree GetAssemblyInfo(AssemblyInfo info)
        {
            return CompilationUnit()
                .WithUsings(
                    List(
                        new[]
                        {
                            UsingDirective(
                                QualifiedName(
                                    IdentifierName("System"),
                                    IdentifierName("Reflection"))),
                            UsingDirective(
                                QualifiedName(
                                    QualifiedName(
                                        IdentifierName("System"),
                                        IdentifierName("Runtime")),
                                    IdentifierName("InteropServices"))),
                            UsingDirective(
                                QualifiedName(
                                    QualifiedName(
                                        IdentifierName("System"),
                                        IdentifierName("Runtime")),
                                    IdentifierName("Versioning")))
                        }))
                .WithAttributeLists(
                    List(
                        new[]
                        {
                            AttributeList(
            SingletonSeparatedList(
                Attribute(
                    IdentifierName("TargetFramework"))
                .WithArgumentList(
                    AttributeArgumentList(
                        SeparatedList<AttributeArgumentSyntax>(
                            new SyntaxNodeOrToken[]{
                                AttributeArgument(
                                    LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        Literal(".NETFramework,Version=v4.5"))),
                                Token(SyntaxKind.CommaToken),
                                AttributeArgument(
                                    LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        Literal(".NET Framework 4.5")))
                                .WithNameEquals(
                                    NameEquals(
                                        IdentifierName("FrameworkDisplayName")))})))))
        .WithTarget(
            AttributeTargetSpecifier(
                Token(SyntaxKind.AssemblyKeyword))),
                            AttributeList(
                                SingletonSeparatedList(
                                    Attribute(
                                        IdentifierName("AssemblyTitle"))
                                        .WithArgumentList(
                                            AttributeArgumentList(
                                                SingletonSeparatedList(
                                                    AttributeArgument(
                                                        LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression,
                                                            Literal(info.Title))))))))
                                .WithTarget(
                                    AttributeTargetSpecifier(
                                        Token(SyntaxKind.AssemblyKeyword))),
                            AttributeList(
                                SingletonSeparatedList(
                                    Attribute(
                                        IdentifierName("AssemblyProduct"))
                                        .WithArgumentList(
                                            AttributeArgumentList(
                                                SingletonSeparatedList(
                                                    AttributeArgument(
                                                        LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression,
                                                            Literal(info.Product))))))))
                                .WithTarget(
                                    AttributeTargetSpecifier(
                                        Token(SyntaxKind.AssemblyKeyword))),
                            AttributeList(
                                SingletonSeparatedList(
                                    Attribute(
                                        IdentifierName("AssemblyCopyright"))
                                        .WithArgumentList(
                                            AttributeArgumentList(
                                                SingletonSeparatedList(
                                                    AttributeArgument(
                                                        LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression,
                                                            Literal(info.Copyright))))))))
                                .WithTarget(
                                    AttributeTargetSpecifier(
                                        Token(SyntaxKind.AssemblyKeyword))),
                            AttributeList(
                                SingletonSeparatedList(
                                    Attribute(
                                        IdentifierName("ComVisible"))
                                        .WithArgumentList(
                                            AttributeArgumentList(
                                                SingletonSeparatedList(
                                                    AttributeArgument(
                                                        LiteralExpression(info.ComVisible
                                                            ? SyntaxKind.TrueLiteralExpression
                                                            : SyntaxKind.FalseLiteralExpression)))))))
                                .WithTarget(
                                    AttributeTargetSpecifier(
                                        Token(SyntaxKind.AssemblyKeyword))),
                            AttributeList(
                                SingletonSeparatedList(
                                    Attribute(
                                        IdentifierName("Guid"))
                                        .WithArgumentList(
                                            AttributeArgumentList(
                                                SingletonSeparatedList(
                                                    AttributeArgument(
                                                        LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression,
                                                            Literal(info.Guid))))))))
                                .WithTarget(
                                    AttributeTargetSpecifier(
                                        Token(SyntaxKind.AssemblyKeyword))),
                            AttributeList(
                                SingletonSeparatedList(
                                    Attribute(
                                        IdentifierName("AssemblyVersion"))
                                        .WithArgumentList(
                                            AttributeArgumentList(
                                                SingletonSeparatedList(
                                                    AttributeArgument(
                                                        LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression,
                                                            Literal(info.Version))))))))
                                .WithTarget(
                                    AttributeTargetSpecifier(
                                        Token(SyntaxKind.AssemblyKeyword))),
                            AttributeList(
                                SingletonSeparatedList(
                                    Attribute(
                                        IdentifierName("AssemblyFileVersion"))
                                        .WithArgumentList(
                                            AttributeArgumentList(
                                                SingletonSeparatedList(
                                                    AttributeArgument(
                                                        LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression,
                                                            Literal(info.FileVersion))))))))
                                .WithTarget(
                                    AttributeTargetSpecifier(
                                        Token(SyntaxKind.AssemblyKeyword)))
                        }))
                .NormalizeWhitespace()
                .SyntaxTree;
        }

        #endregion Private Method

        #region Help Class

        public class AssemblyInfo
        {
            public string Title { get; set; }
            public string Product { get; set; }
            public string Copyright { get; set; }
            public string Guid { get; set; }
            public string Version { get; set; }
            public string FileVersion { get; set; }
            public bool ComVisible { get; set; }

            public static AssemblyInfo Create(string name, string copyright = "Copyright ©  Surging", string version = "0.0.0.1")
            {
                return new AssemblyInfo
                {
                    Title = name,
                    Product = name,
                    Copyright = copyright,
                    Guid = System.Guid.NewGuid().ToString("D"),
                    ComVisible = false,
                    Version = version,
                    FileVersion = version
                };
            }
        }

        #endregion Help Class
    }
}