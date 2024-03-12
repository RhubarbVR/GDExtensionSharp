using System.Text;
using System.Text.RegularExpressions;


namespace GDExtensionSharp;

public static class DocsTools
{
	static string XMLConstant(string value) {
		value = NameConverter.SnakeToPascal(value);
		return value switch {
			"@gdscript.nan" => "NaN",
			"@gdscript.tau" => "Tau",
			_ => value,
		};
	}


	static readonly (string, MatchEvaluator)[] _xmlReplacements = [
		(@"<", x => "&lt;"),
		(@">", x => "&gt;"),
		(@"&", x => "&amp;"),
		(@"\[b\](?<a>.+?)\[/b\]", x => $"<b>{x.Groups["a"].Captures[0].Value}</b>"),
		(@"\[i\](?<a>.+?)\[/i\]", x => $"<i>{x.Groups["a"].Captures[0].Value}</i>"),
		(@"\[constant (?<a>\S+?)\]", x => $"<see cref=\"{XMLConstant(x.Groups["a"].Captures[0].Value)}\"/>"),
		(@"\[code\](?<a>.+?)\[/code\]", x => $"<c>{x.Groups["a"].Captures[0].Value}</c>"),
		(@"\[param (?<a>\S+?)\]",x => $"<paramref name=\"{x.Groups["a"].Captures[0].Value}\"/>"),
		(@"\[method (?<a>\S+?)\]", x => $"<see cref=\"{NameConverter.MethodName(x.Groups["a"].Captures[0].Value)}\"/>"),
		(@"\[member (?<a>\S+?)\]", x => $"<see cref=\"{x.Groups["a"].Captures[0].Value}\"/>"),
		(@"\[enum (?<a>\S+?)\]",x => $"<see cref=\"{x.Groups["a"].Captures[0].Value}\"/>"),
		(@"\[signal (?<a>\S+?)\]", x => $"<see cref=\"EmitSignal{NameConverter.SnakeToPascal( x.Groups["a"].Captures[0].Value)}\"/>"), //currently just two functions
		(@"\[theme_item (?<a>\S+?)\]", x => $"<see cref=\"{x.Groups["a"].Captures[0].Value}\"/>"), //no clue
		//(@"cref=""Url=\$docsUrl/(?<a>.+?)/>", x => $"href=\"https://docs.godotengine.org/en/stable/{x.Groups["a"].Captures[0].Value}\"/>"),
		(@"\[url=(?<a>.+?)\](?<b>.+?)\[/url]", x => $"<see href=\"{x.Groups["a"].Captures[0].Value}\">{x.Groups["b"].Captures[0].Value}</see>"),
		(@"\[(?<a>\S+?)\]", x => $"<see cref=\"{x.Groups["a"].Captures[0].Value}\"/>"), //can be multiple things
	];

	public static string XMLComment(string comment, string tabs) {
		var result = tabs + "/// <summary>\n";
		var lines = comment.Trim().Split('\n');
		for (var i = 0; i < lines.Length; i++) {
			var line = lines[i].Trim();
			if (line.Contains("[codeblock]")) {
				var offset = lines[i].Count(x => x == '\t');
				result += tabs + "/// <code>\n";
				i += 1;
				line = lines[i].Substring(offset);
				while (line.Contains("[/codeblock]") == false) {
					i += 1;
					result += tabs + "/// " + line + "\n";
					while (lines[i].Length <= offset) { i += 1; }
					line = lines[i].Substring(offset);
				}
				result += tabs + "/// </code>\n";
			}
			else if (line.Contains("[codeblocks]")) {
				while (line.Contains("[/codeblocks]") == false) {
					i += 1;
					line = lines[i].Trim();
					if (line.Contains("[csharp]")) {
						var offset = lines[i].Count(x => x == '\t');
						result += tabs + "/// <code>\n";
						i += 1;
						line = lines[i].Substring(offset);
						while (line.Contains("[/csharp]") == false) {
							i += 1;
							result += tabs + "/// " + line + "\n";
							while (lines[i].Length <= offset) { i += 1; }
							line = lines[i].Substring(offset);
						}
						result += tabs + "/// </code>\n";
					}
				}
			}
			else {
				foreach (var (pattern, replacement) in _xmlReplacements) {
					line = Regex.Replace(line, pattern, replacement);
				}
				result += tabs + "/// " + line + "<br/>" + "\n";
			}
		}
		result += tabs + "/// </summary>";
		return result;
	}
}