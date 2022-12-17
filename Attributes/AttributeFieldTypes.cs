using HarmonyLib;

namespace ModSettings {
	internal static class AttributeFieldTypes {

		private static readonly HashSet<Type> integerTypes = new HashSet<Type>() {
			typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long)
		};
		private static readonly HashSet<Type> floatTypes = new HashSet<Type>() {
			typeof(float), typeof(double), typeof(decimal)
		};

		private static readonly ICollection<Type> choiceTypes = new HashSet<Type>(integerTypes) { typeof(bool) };
		private static readonly ICollection<Type> sliderTypes = Union(integerTypes, floatTypes);
		private static readonly ICollection<Type> supportedTypes = Union(choiceTypes, sliderTypes);

		private static ICollection<Type> Union(ICollection<Type> left, ICollection<Type> right) {
			HashSet<Type> result = new HashSet<Type>(left);
			result.UnionWith(right);
			return result;
		}

		internal static bool IsIntegerType(Type type) => integerTypes.Contains(type);
		internal static bool IsFloatType(Type type) => floatTypes.Contains(type);
		internal static bool IsChoiceType(Type type) => choiceTypes.Contains(type);
		internal static bool IsSliderType(Type type) => sliderTypes.Contains(type);
		internal static bool IsSupportedType(Type type) => type.IsEnum || supportedTypes.Contains(type);

		internal static long MaxValue(Type numericType) {
			if (numericType == typeof(bool))
				return 1L;
			else
				return Convert.ToInt64(AccessTools.Field(numericType, "MaxValue").GetValue(null));
		}

		internal static long MinValue(Type numericType) {
			if (numericType == typeof(bool))
				return 0L;
			else
				return Convert.ToInt64(AccessTools.Field(numericType, "MinValue").GetValue(null));
		}
	}
}
