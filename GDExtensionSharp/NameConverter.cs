using System.Text;


namespace GDExtensionSharp;

public static class NameConverter
{
	public static string GetEnumName(string name) {
		return name.Replace(".", "");
	}

	public static string MethodName(string name) {
		var res = "";
		var parts = name.Split('.');
		for (var i = 0; i < parts.Length - 1; i++) {
			res += parts[i] + ".";
		}
		var last = NameConverter.SnakeToPascal(parts.Last());
		return res + last switch {
			"GetType" => "GetTypeGD",
			_ => last,
		};
	}

	public static string MakeType(string name) {

		return name;
	}

	public static string ConvertTypeName(string name) {

		return name;
	}

	public static string SnakeToPascal(string name) {
		var res = "";
		foreach (var w in name.Split('_')) {
			if (w.Length == 0) {
				res += "_";
			}
			else {
				res += w[0].ToString().ToUpper() + w.Substring(1).ToLower();
			}
		}
		for (var i = 0; i < res.Length - 1; i++) {
			if (char.IsDigit(res[i]) && res[i + 1] == 'd') {
				res = string.Concat(res.Substring(0, i + 1), "D", res.Substring(i + 2));
			}
		}
		return res;
	}
}