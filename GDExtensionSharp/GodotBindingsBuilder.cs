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
		var arg = string.Join(" ", args);
		var runningProcess = Process.Start(new ProcessStartInfo {
			WorkingDirectory = worksDir,
			Arguments = arg,
			FileName = fullPath,
		});
		runningProcess.WaitForExit();
	}

	public class CacheInfo
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

		var currentCacheInfoJson = JObject.FromObject(new CacheInfo {
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
		if (GodotCodeSourceGenerator.LoadedOptions.ForceReload) {
			readFromCache = false;
		}
		if (readFromCache) {
			return Directory.GetFiles(cacheDir);
		}
		var docs = Path.Combine(WorkingDir, "doc", "classes") + "/";
		new GodotFilesGenerate(apiJson, jsonFile, docs, cacheDir, allGenFiles).GenerateFiles();
		File.WriteAllText(cacheInfoJson, currentCacheInfoJson);
		return allGenFiles;
	}
}
