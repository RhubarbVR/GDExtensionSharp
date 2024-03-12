using System.Data;
using System.Text;

using Newtonsoft.Json.Linq;

namespace GDExtensionSharp;

public sealed class GodotFilesGenerate(JObject jsonData, string jsonPath, string targetSavePath, List<string> outputFiles)
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

	public Api LoadedAPI;


	public void GenerateFiles() {
		LoadedAPI = Api.Create(jsonData);
		{
			//Todo add way to validate api data
			//if (false) {
			//	GodotCodeSourceGenerator.SendError(4, "Api Not supported");
			//}
		}
		if (!Directory.Exists(targetSavePath)) {
			Directory.CreateDirectory(targetSavePath);
		}
		else {
			foreach (var file in Directory.GetFiles(targetSavePath)) {
				File.Delete(file);
			}
		}
		LoadGlobalEnums();
	}


	private void LoadGlobalEnums() {
		foreach (var item in LoadedAPI.globalEnums) {
			var newBuilder = new StringBuilder();
			StartFile(newBuilder);
			LoadEnumData(item, "", newBuilder);
			EndField(newBuilder);
			AddSource("global_enum_" + item.name + ".g.cs", newBuilder);
		}
	}

	private void StartFile(StringBuilder stringBuilder) {
		stringBuilder.AppendLine("// This is a generated file changes will be removed");
		stringBuilder.AppendLine("namespace Godot {");
	}

	private void EndField(StringBuilder stringBuilder) {
		stringBuilder.AppendLine("}");
		stringBuilder.AppendLine("// This is a generated file changes will be removed");
	}

	private void LoadEnumData(Api.Enum enumData, string addedTab, StringBuilder data) {
		data.AppendLine($"public enum {NameConverter.GetEnumName(enumData.name)} {{");
		foreach (var item in enumData.values) {
			data.AppendLine($"{addedTab}{NameConverter.SnakeToPascal(item.name)},");
		}
		data.AppendLine($"}}");
	}

}
