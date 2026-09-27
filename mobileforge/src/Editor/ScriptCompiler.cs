using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MobileForge.Core;

namespace MobileForge.Editor;

internal static class ScriptCompiler
{
    internal static (IGame? game, string message) Compile(string source)
    {
        var references = new[]
        {
            typeof(object).Assembly,
            typeof(IGame).Assembly,
            typeof(System.Linq.Enumerable).Assembly,
            typeof(System.Runtime.GCSettings).Assembly
        }.DistinctBy(x => x.Location).Where(x => !string.IsNullOrEmpty(x.Location))
            .Select(x => MetadataReference.CreateFromFile(x.Location));

        var tree = CSharpSyntaxTree.ParseText(source);
        var compilation = CSharpCompilation.Create(
            $"MobileForgeProject_{Guid.NewGuid():N}",
            new[] { tree }, references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var output = new MemoryStream();
        var result = compilation.Emit(output);
        if (!result.Success)
        {
            var errors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)
                .Take(8).Select(d => d.ToString());
            return (null, string.Join("\n", errors));
        }

        try
        {
            var assembly = Assembly.Load(output.ToArray());
            var type = assembly.GetTypes().FirstOrDefault(t => typeof(IGame).IsAssignableFrom(t) && !t.IsAbstract);
            return type is null ? (null, "Crie uma classe pública que implemente IGame.")
                : (Activator.CreateInstance(type) as IGame, "Compilado. Toque na prévia para mover o quadrado.");
        }
        catch (Exception ex)
        {
            return (null, $"Falha ao carregar o jogo: {ex.Message}");
        }
    }
}
