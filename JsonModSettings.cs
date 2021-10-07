using MelonLoader.TinyJSON;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ModSettings {
	public abstract class JsonModSettings : ModSettingsBase {

		private static readonly string MODS_DIRECTORY = Path.GetFullPath(typeof(MelonLoader.MelonMod).Assembly.Location + @"\..\..\Mods");

		protected readonly string modName;
		protected readonly string jsonPath;

		public JsonModSettings() : this(null) {
		}

		public JsonModSettings(string relativeJsonFilePath) {
			modName = GetType().Assembly.GetName().Name;
			jsonPath = ToAbsoluteJsonPath(relativeJsonFilePath ?? modName);
			LoadOrCreate();
		}

		private static string ToAbsoluteJsonPath(string relativePath) {
			if (string.IsNullOrEmpty(relativePath)) {
				throw new ArgumentException("JSON file path cannot be null or empty");
			} else if (relativePath.IndexOfAny(Path.GetInvalidPathChars()) >= 0) {
				throw new ArgumentException($"JSON file path contains an invalid path character: {relativePath}");
			} else if (Path.IsPathRooted(relativePath)) {
				throw new ArgumentException("JSON file path must be relative. Absolute paths are not allowed.");
			}

			if (Path.GetExtension(relativePath) != ".json") {
				relativePath += ".json";
			}

			return Path.Combine(MODS_DIRECTORY, relativePath);
		}

		protected override void OnConfirm() {
			Save();
		}

		public void Save() {
			try {
				string json = JSON.Dump(this, EncodeOptions.PrettyPrint | EncodeOptions.NoTypeHints);
				File.WriteAllText(jsonPath, json, Encoding.UTF8);
				Debug.Log($"[{modName}] Config file saved to {jsonPath}");
			} catch (Exception ex) {
				MelonLoader.MelonLogger.Error($"[{modName}] Error while trying to write config file {jsonPath}: {ex}");
			}
		}

		public void Reload() {
			try {
				string json = File.ReadAllText(jsonPath, Encoding.UTF8);
				Variant parsed = JSON.Load(json);
				MethodInfo populateMethod = typeof(JSON).GetMethod("Populate").MakeGenericMethod(GetType());
				populateMethod.Invoke(null, new object[] { parsed, this });

				foreach (FieldInfo field in fields) {
					confirmedValues[field] = field.GetValue(this);
				}
			} catch (Exception ex) {
				MelonLoader.MelonLogger.Error($"[{modName}] Error while trying to read config file {jsonPath}: {ex}");

				// Re-throw to make error show up in main menu
				throw new IOException($"Error while trying to read config file {jsonPath}", ex);
			}
		}

		private void LoadOrCreate() {
			if (File.Exists(jsonPath)) {
				Reload();
			} else {
				Debug.Log($"[{modName}] Settings file {jsonPath} did not exist, writing default settings file");

				// All default field values are now confirmed values
				foreach (FieldInfo field in fields) {
					confirmedValues[field] = field.GetValue(this);
				}

				string json = JSON.Dump(this, EncodeOptions.PrettyPrint | EncodeOptions.NoTypeHints);
				File.WriteAllText(jsonPath, json, Encoding.UTF8);
			}
		}
	}
}
