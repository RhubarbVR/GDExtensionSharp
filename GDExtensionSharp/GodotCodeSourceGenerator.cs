using Microsoft.CodeAnalysis;
using SharpFileDialog;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace GDExtensionSharp;

[Generator]
public class GodotCodeSourceGenerator : ISourceGenerator
{
	public class LocalOptions
	{
		public string Note { get; set; } = "You probably want to add this file to your gitignore";
		public string EditorPath { get; set; }
	}

	public class SharedOptions
	{
		public bool AllPublic { get; set; }

		public bool ExtendableAPI { get; set; }
	}

	public class Options
	{
		public string EditorPath { get; set; }

		public bool AllPublic { get; set; }

		public bool ExtendableAPI { get; set; }
	}

	public static Options LoadedOptions;

	public static string GetPlatformExecutableExtension() {
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
			return "exe";
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
			return "";
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
			return "app";
		}
		else {
			throw new PlatformNotSupportedException();
		}
	}

	private static GeneratorExecutionContext _generatorExecutionContext;
	public static void SendError(int errorIndex, string message) {
		_generatorExecutionContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor($"GDS{errorIndex:D4}", message, message, "GodotSharp", DiagnosticSeverity.Error, true), null));
		throw new Exception("Send error");
	}

	public void Execute(GeneratorExecutionContext context) {
		_generatorExecutionContext = context;
		//if (!Debugger.IsAttached) {
		//	Debugger.Launch();
		//}

		if (!context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.projectdir", out var currentPath)) {
			SendError(1, "Failed to find Project Dir");
		}

		var localJsonPath = Path.Combine(currentPath, "GDSharpConfig.local.json");
		var noneLocalJsonPath = Path.Combine(currentPath, "GDSharpConfig.json");
		JObject jObject = null;
		if (File.Exists(localJsonPath)) {
			jObject = JObject.Parse(File.ReadAllText(localJsonPath));
		}
		if (File.Exists(noneLocalJsonPath)) {
			if (jObject is null) {
				jObject = JObject.Parse(File.ReadAllText(noneLocalJsonPath));
			}
			else {
				var newObject = JObject.Parse(File.ReadAllText(noneLocalJsonPath));
				newObject.Merge(jObject);
				jObject = newObject;
			}
		}
		jObject ??= [];

		LoadedOptions = jObject.ToObject<Options>();
		if (!File.Exists(LoadedOptions.EditorPath)) {
			string newEditorPath;
			try {
				if (!NativeFileDialog.OpenDialog([new() { Extensions = [GetPlatformExecutableExtension()], Name = "Godot Executable" }], currentPath, out newEditorPath)) {
					newEditorPath = null;
				}
			}
			catch {
				newEditorPath = null;
			}
			if (File.Exists(newEditorPath)) {
				if (File.Exists(localJsonPath)) {
					var oldObject = JObject.Parse(File.ReadAllText(localJsonPath));
					var newObject = JObject.FromObject(new LocalOptions { EditorPath = newEditorPath });
					oldObject.Merge(newObject);
					File.WriteAllText(localJsonPath, oldObject.ToString());
				}
				else {
					File.WriteAllText(localJsonPath, JObject.FromObject(new LocalOptions { EditorPath = newEditorPath }).ToString());
				}
				LoadedOptions.EditorPath = newEditorPath;
			}
			else {
				SendError(2, "Failed to load godot editor path");
			}
		}
		if (File.Exists(noneLocalJsonPath)) {
			var oldObject = JObject.Parse(File.ReadAllText(noneLocalJsonPath));
			var newObject = JObject.FromObject(new SharedOptions());
			newObject.Merge(oldObject);
			File.WriteAllText(noneLocalJsonPath, newObject.ToString());
		}
		else {
			File.WriteAllText(noneLocalJsonPath, JObject.FromObject(new SharedOptions()).ToString());
		}


		var runner = new GodotBindingsBuilder(LoadedOptions.EditorPath);
		runner.RunWithCommand("--dump-extension-api", "--headless");
		foreach (var item in runner.GenerateOrCacheSourceFiles(LoadedOptions)) {
			context.AddSource(Path.GetFileName(item), File.ReadAllText(item));
		}
	}

	public void Initialize(GeneratorInitializationContext context) {
	}
}
