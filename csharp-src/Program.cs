using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
#if FORGE_EDITOR
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
#endif

public partial class Forge
{
    static Func<string, string>? frame;
    static Action? start;
    static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };
    public static void Main() { }

    [JSExport]
    public static string Load(string assemblyBase64)
    {
        frame = null;
        start = null;
        try
        {
            var assembly = Assembly.Load(Convert.FromBase64String(assemblyBase64));
            var entry = assembly.GetType("MobileForge.__Entry", throwOnError: true)!;
            frame = entry.GetMethod("Frame")!.CreateDelegate<Func<string, string>>();
            start = entry.GetMethod("Start")!.CreateDelegate<Action>();
            return "{\"ok\":true}";
        }
        catch (Exception e) { return Error(e); }
    }

    [JSExport]
    public static string Start()
    {
        try { start!(); return "{\"ok\":true}"; }
        catch (Exception e) { return Error(e); }
    }

    [JSExport]
    public static string Frame(string input)
    {
        try { return frame!(input); }
        catch (Exception e) { return Error(e); }
    }

    static string Error(Exception e) => JsonSerializer.Serialize(new { ok = false, error = e.ToString() });

#if FORGE_EDITOR
    public record SourceFile(string Name, string Code);
    static string? engine;
    [JSExport]
    public static string Compile(string filesJson)
    {
        try
        {
            var files = JsonSerializer.Deserialize<SourceFile[]>(filesJson, Json)!;
            if (files.Length == 0 || files.Length > 64) throw new Exception("O projeto deve ter de 1 a 64 arquivos.");
            if (engine is null)
            {
                using var stream = typeof(Forge).Assembly.GetManifestResourceStream("ForgeRuntime.Engine.txt")!;
                using var reader = new StreamReader(stream);
                engine = reader.ReadToEnd();
            }
            var parse = new CSharpParseOptions(LanguageVersion.CSharp13);
            var trees = files.Select(f => CSharpSyntaxTree.ParseText(f.Code, parse, path: f.Name)).ToList();
            trees.Add(CSharpSyntaxTree.ParseText(engine, parse, path: "MobileForge.Engine.cs"));
            var compilation = CSharpCompilation.Create("Game_" + Guid.NewGuid().ToString("N"), trees,
                Basic.Reference.Assemblies.Net90.References.All,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));
            using var output = new MemoryStream();
            var result = compilation.Emit(output);
            var diagnostics = result.Diagnostics.Where(d => d.Severity is DiagnosticSeverity.Error or DiagnosticSeverity.Warning)
                .Select(d => { var p = d.Location.GetLineSpan(); return new {
                    file = p.Path, line = p.StartLinePosition.Line + 1, column = p.StartLinePosition.Character + 1,
                    code = d.Id, severity = d.Severity.ToString(), message = d.GetMessage()
                }; }).ToArray();
            return JsonSerializer.Serialize(new { ok = result.Success, diagnostics,
                assembly = result.Success ? Convert.ToBase64String(output.ToArray()) : null });
        }
        catch (Exception e) { return Error(e); }
    }
#endif
}
