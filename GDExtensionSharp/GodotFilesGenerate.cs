using System;
using System.Data;
using System.Text;
using System.Xml.Serialization;

using Newtonsoft.Json.Linq;

using static GDExtensionSharp.Api;

namespace GDExtensionSharp;

public sealed class GodotFilesGenerate(JObject jsonData, string jsonPath, string docsPath, string targetSavePath, List<string> outputFiles)
{
	private readonly XmlSerializer _classXml = new(typeof(Class));
	private readonly XmlSerializer _builtinXml = new(typeof(BuiltinClass));

	private string GetFloat() {
		return "float";
	}

	private string ExtendableApiString() {
		if (GodotCodeSourceGenerator.LoadedOptions.ExtendableAPI) {
			return " partial";
		}
		else {
			return "";
		}
	}
	private string ExposedApiString() {
		if (GodotCodeSourceGenerator.LoadedOptions.AllPublic) {
			return "public";
		}
		else {
			return "internal";
		}
	}

	public BuiltinClass GetBuiltinDocs(string name) {
		if (docsPath == null) { return null; }
		var path = Path.Combine(docsPath, name + ".xml");
		if (File.Exists(path)) {
			var file = File.OpenRead(path);
			var d = (BuiltinClass)_builtinXml.Deserialize(file)!;
			file.Close();
			return d;
		}
		else {
			return null;
		}
	}

	public Class GetDocs(string name) {
		if (docsPath == null) { return null; }
		var path = Path.Combine(docsPath, name + ".xml");
		if (File.Exists(path)) {
			var file = File.OpenRead(path);
			var d = (Class)_classXml.Deserialize(file)!;
			file.Close();
			return d;
		}
		else {
			return null;
		}
	}

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
		LoadBuildBuiltInClasses();
		LoadBuildClasss();
	}


	private void LoadGlobalEnums() {
		foreach (var item in LoadedAPI.globalEnums) {
			var newBuilder = new StringBuilder();
			StartFile(newBuilder);
			LoadEnumData(item, "\t", newBuilder, []);
			EndField(newBuilder);
			AddSource("global_enum_" + item.name + ".g.cs", newBuilder);
		}
	}
	private void LoadBuildClasss() {
		foreach (var item in LoadedAPI.classes) {
			var newBuilder = new StringBuilder();
			StartFile(newBuilder);
			BuildClass(item, "\t", newBuilder);
			EndField(newBuilder);
			AddSource("class_" + item.name + ".g.cs", newBuilder);
		}
	}

	private void LoadBuildBuiltInClasses() {
		foreach (var item in LoadedAPI.builtinClasses) {
			var newBuilder = new StringBuilder();
			StartFile(newBuilder);
			BuildBuiltInClass(item, "\t", newBuilder);
			EndField(newBuilder);
			AddSource("built_in_" + item.name + ".g.cs", newBuilder);
		}
	}

	private void BuildClass(Api.Class classData, string addedTab, StringBuilder data) {
		var docData = GetDocs(classData.name);

		{
			StartNameSpace(data, addedTab, "Raw");
			if (docData?.description is not null) {
				data.AppendLine(DocsTools.XMLComment(docData.description, addedTab + "\t"));
			}
			data.AppendLine($"{addedTab + "\t"}{ExposedApiString()} ref{ExtendableApiString()} struct Raw{NameConverter.ConvertTypeName(classData.name)} {{");
			data.AppendLine($"{addedTab + "\t\t"}public unsafe void* instance;");
			foreach (var item in classData.methods ?? []) {
				BuildRawMethod(item, (docData?.methods ?? []).Where(x => x.name == item.name).FirstOrDefault(), addedTab + "\t\t", data);
			}
			data.AppendLine($"{addedTab + "\t"}}}");
			EndNameSpace(data, addedTab);
		}
		data.AppendLine();
		if (docData?.description is not null) {
			data.AppendLine(DocsTools.XMLComment(docData.description, addedTab));
		}
		data.AppendLine($"{addedTab}public {ExtendableApiString()} class {NameConverter.ConvertTypeName(classData.name)} {{");
		foreach (var item in classData.enums ?? []) {
			LoadEnumData(item, addedTab + "\t", data, docData?.constants ?? []);
		}
		data.AppendLine($"{addedTab}}}");
	}

	private void BuildRawMethod(Api.Method methodData, Method methodDoc, string addedTab, StringBuilder data) {
		if (methodDoc?.description is not null) {
			data.AppendLine(DocsTools.XMLComment(methodDoc.description, addedTab));
		}
		data.Append($"{addedTab}{ExposedApiString()} static unsafe ");
		var hadReturnData = false;
		var ret = methodData.returnType ?? methodData.returnValue?.type ?? "";
		if (!string.IsNullOrEmpty(ret)) {
			if(ret != "void") {
				hadReturnData = true;
				ret = NameConverter.MakeType(ret);
				if (IsCSharpBuiltInType(ret)) {
					if(ret == "float") {
						data.Append($"{GetFloat()}");
					}
					else {
						data.Append($"{ret}");
					}

				}
				else {
					data.Append($"Raw.GD_{ret}");
				}
			}
			else {
				hadReturnData = false;
				data.Append("void");
			}
		}
		else {
			hadReturnData = false;
			data.Append("void");
		}
		data.Append(' ');
		data.Append(NameConverter.MethodName(methodData.name));
		data.Append('(');
		data.AppendLine(") {");
		if (hadReturnData) {
			data.AppendLine($"{addedTab}\treturn defualt;");
		}
		data.AppendLine($"{addedTab}}}");
	}

	private static bool IsCSharpBuiltInType(string name) {
		return name is "bool" or "int" or "float";
	}

	private void BuildBuiltInClass(Api.BuiltinClass builtinClass, string addedTab, StringBuilder data) {
		if (IsCSharpBuiltInType(builtinClass.name)) {
			return;
		}
		var docData = GetBuiltinDocs(builtinClass.name);
		{
			StartNameSpace(data, addedTab, "Raw");
			if (docData?.description is not null) {
				data.AppendLine(DocsTools.XMLComment(docData.description, addedTab + "\t"));
			}
			data.AppendLine($"{addedTab + "\t"}{ExposedApiString()} ref{ExtendableApiString()} struct GD_{NameConverter.ConvertTypeName(builtinClass.name)} {{");
			foreach (var item in builtinClass.methods ?? []) {
				BuildRawMethod(item, (docData?.methods ?? []).Where(x => x.name == item.name).FirstOrDefault(), addedTab + "\t\t", data);
			}
			data.AppendLine($"{addedTab + "\t"}}}");
			EndNameSpace(data, addedTab);
		}
		data.AppendLine();
		if (docData?.description is not null) {
			data.AppendLine(DocsTools.XMLComment(docData.description, addedTab));
		}
		var isClass = false;
		data.AppendLine($"{addedTab}public {ExtendableApiString()} {(isClass ? "class" : "struct")} {NameConverter.ConvertTypeName(builtinClass.name)} {{");
		foreach (var item in builtinClass.enums ?? []) {
			LoadEnumData(item, addedTab + "\t", data, docData?.constants ?? []);
		}
		data.AppendLine($"{addedTab}}}");
	}

	private void StartNameSpace(StringBuilder stringBuilder, string addedTab, string extraNameSpace = "") {
		stringBuilder.AppendLine($"{addedTab}namespace {extraNameSpace} {{");
	}
	private void EndNameSpace(StringBuilder stringBuilder, string addedTab) {
		stringBuilder.AppendLine($"{addedTab}}}");
	}

	private void StartFile(StringBuilder stringBuilder, string extraNameSpace = "") {
		stringBuilder.AppendLine("// This is a generated file changes will be removed");
		stringBuilder.AppendLine("using System;");
		stringBuilder.AppendLine($"namespace Godot{extraNameSpace} {{");
		stringBuilder.AppendLine();
	}

	private void EndField(StringBuilder stringBuilder) {
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("}");
		stringBuilder.AppendLine("// This is a generated file changes will be removed");
	}

	private void LoadEnumData(Api.Enum enumData, string addedTab, StringBuilder data, Constant[] constants) {
		if (enumData.isBitfield ?? false) {
			data.AppendLine($"{addedTab}[Flags]");
		}
		data.AppendLine($"{addedTab}public enum {NameConverter.GetEnumName(enumData.name)} : long {{");
		foreach (var item in enumData.values) {
			var target = constants.Where(c => c.name == item.name).FirstOrDefault();
			if (target?.comment is not null) {
				data.AppendLine(DocsTools.XMLComment(target.comment, addedTab + "\t"));
			}
			data.AppendLine($"{addedTab}\t{NameConverter.SnakeToPascal(item.name)} = {item.value},");
		}
		data.AppendLine($"{addedTab}}}");
	}

}
