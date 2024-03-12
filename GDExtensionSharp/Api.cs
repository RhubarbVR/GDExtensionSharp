using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GDExtensionSharp;

#nullable enable
#pragma warning disable IDE1006 // Naming Styles

public record struct Api
{
	public static Api Create(JObject objected) {
		return objected.ToObject<Api>();
	}

	public static JObject Save(Api apiData) {
		return JObject.FromObject(apiData);
	}

	public record struct Argument
	{
		public string name { get; set; }
		public string type { get; set; }
		[JsonProperty("default_value")] public string? defaultValue { get; set; }
		public string? meta { get; set; }
	}

	public record struct ReturnValue
	{
		public string type { get; set; }
		public string? meta { get; set; }
	}

	public record struct Constant
	{
		public string name { get; set; }
		public string type { get; set; }
		public string value { get; set; }
	}

	public record struct Size
	{
		public string name { get; set; }
		public int size { get; set; }
	}

	public record struct Signal
	{
		public string name { get; set; }
		public Argument[]? arguments { get; set; }
	}

	public record struct Property
	{
		public string type { get; set; }
		public string name { get; set; }
		public string setter { get; set; }
		public string getter { get; set; }
		public int? index { get; set; }
	}

	public record struct Singleton
	{
		public string name { get; set; }
		public string type { get; set; }
	}

	public record struct NativeStructure
	{
		public string name { get; set; }
		public string format { get; set; }
	}

	public record struct Header
	{
		[JsonProperty("version_major")] public int versionMajor { get; set; }
		[JsonProperty("version_minor")] public int versionMinor { get; set; }
		[JsonProperty("version_patch")] public int versionPatch { get; set; }
		[JsonProperty("version_status")] public string versionStatus { get; set; }
		[JsonProperty("version_build")] public string versionBuild { get; set; }
		[JsonProperty("version_full_name")] public string versionFullName { get; set; }
	}

	public record struct BuiltinClassSizes
	{
		[JsonProperty("build_configuration")] public string buildConfiguration { get; set; }

		public Size[] sizes { get; set; }
	}

	public record struct MemberOffset
	{
	 	public string member { get; set; }
		public int offset { get; set; }
		public string meta { get; set; }
	}

	public record struct Member
	{
		public string name { get; set; }
		public string type { get; set; }
	}

	public record struct ClassOffsets
	{
		public string name { get; set; }
		public MemberOffset[] members { get; set; }
	}

	public record struct BuiltinClassMemberOffsets
	{
		[JsonProperty("build_configuration")] public string buildConfiguration { get; set; }
		public ClassOffsets[] classes { get; set; }
	}

	public record struct Value
	{
		public string name { get; set; }
		public long value { get; set; }
	}

	public record struct Enum
	{
		public string name { get; set; }
		[JsonProperty("is_bitfield")] public bool? isBitfield { get; set; }
		public Value[] values { get; set; }
	}

	public record struct Method
	{
		public string name { get; set; }
		[JsonProperty("return_type")] public string returnType { get; set; }
		[JsonProperty("is_vararg")] public bool isVararg { get; set; }
		[JsonProperty("is_const")] public bool isConst { get; set; }
		[JsonProperty("is_static")] public bool isStatic { get; set; }
		[JsonProperty("is_virtual")] public bool isVirtual { get; set; }
		public uint hash { get; set; }
		[JsonProperty("return_value")] public ReturnValue? returnValue { get; set; }
		public Argument[]? arguments { get; set; }

		public string? category { get; set; }
	}

	public record struct Operator
	{
		public string name { get; set; }
		[JsonProperty("right_type")] public string? rightType { get; set; }
		[JsonProperty("return_type")] public string returnType { get; set; }
	}

	public record Constructor
	{
		public int index { get; set; }
		public Argument[]? arguments { get; set; }
	}

	public record struct BuiltinClass
	{
		public string name { get; set; }
		[JsonProperty("is_keyed")] public bool isKeyed { get; set; }
		[JsonProperty("indexing_return_type")] public string? indexingReturnType { get; set; }
		public Member[]? members { get; set; }
		public Constant[]? constants { get; set; }
		public Enum[]? enums { get; set; }
		public Operator[]? operators { get; set; }
		public Method[]? methods { get; set; }
		public Constructor[]? constructors { get; set; }
		[JsonProperty("has_destructor")] public bool hasDestructor { get; set; }
	}

	public record struct Class
	{
		public string name { get; set; }
		[JsonProperty("is_refcounted")] public bool isRefcounted { get; set; }
		[JsonProperty("is_instantiable")] public bool isInstantiable { get; set; }
		public string? inherits { get; set; }
		[JsonProperty("api_type")] public string apiType { get; set; }
		public Enum[]? enums { get; set; }
		public Method[]? methods { get; set; }
		public Signal[]? signals { get; set; }
		public Property[]? properties { get; set; }
		public Value[]? constants { get; set; }
	}

	public Header header { get; set; }
	[JsonProperty("builtin_class_sizes")] public BuiltinClassSizes[] builtinClassSizes { get; set; }
	[JsonProperty("builtin_class_member_offsets")] public BuiltinClassMemberOffsets[] builtinClassMemberOffsets { get; set; }
	[JsonProperty("global_constants")] public object[] globalConstants { get; set; }
	[JsonProperty("global_enums")] public Enum[] globalEnums { get; set; }
	[JsonProperty("utility_functions")] public Method[] untilityFunction { get; set; }
	[JsonProperty("builtin_classes")] public BuiltinClass[] builtinClasses { get; set; }
	public Class[] classes { get; set; }
	public Singleton[] singletons { get; set; }
	[JsonProperty("native_structures")] public NativeStructure[] nativeStructures { get; set; }
}

#pragma warning restore IDE1006 // Naming Styles
