using System.Text;
using Newtonsoft.Json.Linq;

namespace GDExtensionSharp;

public sealed class GodotFilesGenerate(JObject jsonData, string targetSavePath, List<string> outputFiles)
{

	public void AddSource(string targetFile, StringBuilder data) {
		var newFile = Path.Combine(targetSavePath, targetFile);
		outputFiles.Add(newFile);
		File.WriteAllText(newFile, data.ToString());
	}

	public void AddSource(string targetFile, string data) {
		var newFile = Path.Combine(targetSavePath, targetFile);
		outputFiles.Add(newFile);
		File.WriteAllText(newFile, data);
	}
	public void GenerateFiles() {
		if (!Directory.Exists(targetSavePath)) {
			Directory.CreateDirectory(targetSavePath);
		}
		else {
			foreach (var file in Directory.GetFiles(targetSavePath)) {
				File.Delete(file);
			}
		}
		AddSource("test.cs", @"public class Test { }");
	}

}
