using System.Diagnostics;

using Newtonsoft.Json.Linq;

namespace GDExtensionSharp;

public sealed class GodotBindingsBuilder(string godotLocation)
{
	public string GodotLocation { get; set; } = godotLocation;

	public string WorkingDir { get; set; }

	/// <summary>
	/// Run Godot With command 
	/// </summary>
	/// <param name="args"></param>
	public void RunWithCommand(params string[] args) {
		var fullPath = Path.GetFullPath(GodotLocation);
		var workingDir = Path.GetDirectoryName(fullPath);
		var worksDir = Path.Combine(workingDir, "GDExtensionSharp");
		WorkingDir = worksDir;
		if (!Directory.Exists(worksDir)) {
			Directory.CreateDirectory(worksDir);
		}
		var runningProcess = Process.Start(new ProcessStartInfo {
			WorkingDirectory = worksDir,
			Arguments = string.Join(" ", args),
			FileName = fullPath,
			UseShellExecute = false,
			RedirectStandardError = true,
			RedirectStandardOutput = true,
		});
		runningProcess.WaitForExit();
	}

	public class CahceInfo
	{
		public string EngineVersion { get; set; }
		public int GenVersion { get; set; }
		public GodotCodeSourceGenerator.Options loadedOptions { get; set; }
	}

	public IEnumerable<string> GenerateOrCacheSourceFiles(GodotCodeSourceGenerator.Options loadedOptions) {
		var allGenFiles = new List<string>();

		var jsonFile = Path.Combine(WorkingDir, "extension_api.json");
		if (!File.Exists(jsonFile)) {
			GodotCodeSourceGenerator.SendError(3, "extension api json not found");
		}

		var apiJson = JObject.Parse(File.ReadAllText(jsonFile));

		var currentCacheInfoJson = JObject.FromObject(new CahceInfo {
			EngineVersion = (string)apiJson["header"]["version_full_name"],
			loadedOptions = loadedOptions,
			GenVersion = 1
		}).ToString();
		var cacheInfoJson = Path.Combine(WorkingDir, "cache_info.json");
		var readFromCache = false;
		if (File.Exists(cacheInfoJson)) {
			if (currentCacheInfoJson == JObject.Parse(File.ReadAllText(cacheInfoJson)).ToString()) {
				readFromCache = true;
			}
		}
		var cacheDir = Path.Combine(WorkingDir, "CachedGen");
		if (!Directory.Exists(cacheDir)) {
			readFromCache = false;
		}
		if (readFromCache) {
			return Directory.GetFiles(cacheDir);
		}
		new GodotFilesGenerate(apiJson, jsonFile, cacheDir, allGenFiles).GenerateFiles();
		File.WriteAllText(cacheInfoJson, currentCacheInfoJson);
		return allGenFiles;
	}
}
