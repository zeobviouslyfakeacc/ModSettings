using System;
using System.Collections.Generic;
using System.Reflection;
using static CustomExperienceModeManager;
using static ModSettings.AttributeFieldTypes;

namespace ModSettings {
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class ChoiceAttribute : Attribute {

		private static ChoiceAttribute Localized(params string[] names) => new ChoiceAttribute(names) { Localize = true };
		private static readonly Dictionary<Type, ChoiceAttribute> predefinedHinterlandEnums = new Dictionary<Type, ChoiceAttribute>() {
			{ typeof(CustomTunableDayNightMultiplier), Localized("GAMEPLAY_OneX", "GAMEPLAY_TwoX", "GAMEPLAY_ThreeX", "GAMEPLAY_FourX") },
			{ typeof(CustomTunableDistance),           Localized("GAMEPLAY_DistanceClose", "GAMEPLAY_DistanceMedium", "GAMEPLAY_DistanceFar") },
			{ typeof(CustomTunableLMH),                Localized("GAMEPLAY_Low", "GAMEPLAY_Medium", "GAMEPLAY_High") },
			{ typeof(CustomTunableLMHV),               Localized("GAMEPLAY_Low", "GAMEPLAY_Medium", "GAMEPLAY_High", "GAMEPLAY_VeryHigh") },
			{ typeof(CustomTunableNLH),                Localized("GAMEPLAY_None", "GAMEPLAY_Low", "GAMEPLAY_High") },
			{ typeof(CustomTunableNLMH),               Localized("GAMEPLAY_None", "GAMEPLAY_Low", "GAMEPLAY_Medium", "GAMEPLAY_High") },
			{ typeof(CustomTunableNLMHV),              Localized("GAMEPLAY_None", "GAMEPLAY_Low", "GAMEPLAY_Medium", "GAMEPLAY_High", "GAMEPLAY_VeryHigh") },
			{ typeof(CustomTunableTimeOfDay),          Localized("GAMEPLAY_Dawn", "GAMEPLAY_Noon", "GAMEPLAY_Dusk", "GAMEPLAY_Midnight", "GAMEPLAY_Random") },
			{ typeof(CustomTunableWeather),            Localized("GAMEPLAY_WeatherClear", "GAMEPLAY_WeatherLightSnow", "GAMEPLAY_WeatherHeavySnow",
			                                                     "GAMEPLAY_WeatherBlizzard", "GAMEPLAY_WeatherLightFog", "GAMEPLAY_WeatherHeavyFog", "GAMEPLAY_Random") }
		};

		internal static readonly ChoiceAttribute YesNoAttribute = Localized("GAMEPLAY_No", "GAMEPLAY_Yes");

		internal static ChoiceAttribute ForEnumType(Type enumType) {
			// Use predefined value names for Hinterland enums
			if (predefinedHinterlandEnums.TryGetValue(enumType, out ChoiceAttribute predefined)) {
				return predefined;
			}

			// If it's an unknown enum, create the value names by prettifying the enum constant names
			string[] enumNames = Enum.GetNames(enumType);
			string[] result = new string[enumNames.Length];
			for (int i = 0; i < result.Length; ++i) {
				result[i] = PrettifyEnumName(enumNames[i]);
			}

			return new ChoiceAttribute(result);
		}

		private static string PrettifyEnumName(string enumName) {
			string name = enumName.Replace('_', ' ');
			bool lower = false;

			for (int j = 0; j < name.Length; ++j) {
				char c = name[j];
				if (lower && Char.IsUpper(c))
					name = name.Insert(j, " ");
				lower = Char.IsLower(c);
			}

			return name;
		}

		private readonly string[] names;
		private bool localize = false;

		public ChoiceAttribute(params string[] names) {
			this.names = names;
		}

		public string[] Names {
			get => names;
		}

		public bool Localize {
			get => localize;
			set => localize = value;
		}

		internal void ValidateFor(ModSettingsBase modSettings, FieldInfo field) {
			Type fieldType = field.FieldType;
			Type enumType = null;

			if (fieldType.IsEnum) {
				enumType = fieldType;
				fieldType = Enum.GetUnderlyingType(fieldType);
			}

			if (!IsChoiceType(fieldType))
				throw new ArgumentException("[ModSettings] 'Choice' attribute doesn't support fields of type " + fieldType.Name, field.Name);

			long typeMax = MaxValue(fieldType);

			if (names == null || names.Length == 0) {
				throw new ArgumentException("[ModSettings] 'Choice' attribute must contain non-empty array of non-empty strings", field.Name);
			} else if (names.Length == 1) {
				throw new ArgumentException("[ModSettings] 'Choice' attribute must contain array of at least two elements", field.Name);
			} else if (names.Length - 1 > typeMax) {
				throw new ArgumentException("[ModSettings] 'Choice' attribute contains more elements than " + fieldType.Name
				                            + " can represent: " + names.Length + " > " + (typeMax + 1), field.Name);
			} else if (enumType != null && names.Length != Enum.GetValues(enumType).Length)
				throw new ArgumentException("[ModSettings] 'Choice' attribute array length doesn't match " + enumType.Name
				                            + " enum value count: " + names.Length + " != " + Enum.GetValues(enumType).Length, field.Name);

			foreach (string name in names) {
				if (string.IsNullOrEmpty(name))
					throw new ArgumentException("[ModSettings] 'Choice' attribute must contain non-empty array of non-empty strings", field.Name);
			}

			int defaultValue = Convert.ToInt32(field.GetValue(modSettings));
			if (defaultValue < 0 || defaultValue >= names.Length) {
				throw new ArgumentException("[ModSettings] Default value out of range for 'Choice' attribute", field.Name);
			}
		}
	}
}
