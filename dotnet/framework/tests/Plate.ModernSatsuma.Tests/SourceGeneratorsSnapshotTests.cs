using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Plate.ModernSatsuma;
using Plate.ModernSatsuma.Generators;
using VerifyTests;
using VerifyXunit;
using Xunit;

namespace Plate.ModernSatsuma.Test
{
    public class SourceGeneratorsSnapshotTests
    {
        [Fact]
        public Task VerifyGraphBuilderGeneratorOutput()
        {
            const string source = @"using Plate.ModernSatsuma;
using Plate.ModernSatsuma.Generators;

namespace Sample
{
    [GraphBuilder(GraphType = typeof(CustomGraph))]
    public partial class SnapshotGraph
    {
        [NodeAttribute]
        public Node A { get; private set; }

        [NodeAttribute]
        public Node B { get; private set; }

        [ArcAttribute]
        public void A_to_B(Node from, Node to) { }
    }
}
";

            var compilation = CreateCompilation(source);

            IIncrementalGenerator generator = new GraphBuilderGenerator();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

            driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

            var runResult = driver.GetRunResult();
            var sb = new StringBuilder();

            foreach (var result in runResult.Results)
            {
                foreach (var generated in result.GeneratedSources.OrderBy(g => g.HintName, StringComparer.Ordinal))
                {
                    sb.AppendLine($"// HintName: {generated.HintName}");
                    sb.AppendLine(generated.SourceText.ToString());
                    sb.AppendLine();
                }
            }

            return Verifier.Verify(sb.ToString());
        }

        private static Compilation CreateCompilation(string source)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);

            var references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Node).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(GraphBuilderAttribute).GetTypeInfo().Assembly.Location),
            };

            return CSharpCompilation.Create(
                assemblyName: "GraphBuilderSnapshotTests",
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        }
    }
}
