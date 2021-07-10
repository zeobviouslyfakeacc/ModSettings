using System;
using System.Collections.Generic;

namespace ModSettings {
	public class VirtualGUIBuilder {

		private readonly List<Setting> settings = new List<Setting>();

		internal void AddSetting(Setting setting) {
			settings.Add(setting);
		}
	}
}
