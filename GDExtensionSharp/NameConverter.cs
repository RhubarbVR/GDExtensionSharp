using System.Text;


namespace GDExtensionSharp;

public static class NameConverter
{
	public static string GetEnumName(string name) {
		return name.Replace(".", "");
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