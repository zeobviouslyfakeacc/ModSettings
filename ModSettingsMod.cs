using MelonLoader;
using ModSettings.Examples;
using UnityEngine;

namespace ModSettings {
	internal class ModSettingsMod : MelonMod {

		public override void OnApplicationStart() {
#if DEBUG
			BasicExample.OnLoad();
			OnChangeExample.OnLoad();
			VisibilityExample.OnLoad();
			DynamicExample.OnLoad();
			CustomChoiceBoxExample.OnLoad();
#endif
			Debug.Log($"[{Info.Name}] version {Info.Version} loaded!");
		}
	}
}
