using MelonLoader;
using UnityEngine;

namespace ModSettings {
	internal class ModSettingsMod : MelonMod {

		public override void OnApplicationStart() {
#if DEBUG
			ModSettingsExample.BasicExample.OnLoad();
			ModSettingsExample.OnChangeExample.OnLoad();
			ModSettingsExample.VisibilityExample.OnLoad();
#endif
			Debug.Log($"[{Info.Name}] version {Info.Version} loaded!");
		}
	}
}
