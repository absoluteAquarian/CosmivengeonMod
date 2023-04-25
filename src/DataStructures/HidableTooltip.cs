using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace CosmivengeonMod.DataStructures {
	public abstract class HidableTooltip : ModItem {
		public virtual string ItemName => null;

		/// <summary>
		/// The part of the tooltip that should always be displayed, regardless of whether the extended tooltip (<see cref="FlavourText"/>) is shown.
		/// <para>
		/// This part of the tooltip is always before the extended tooltip (<see cref="FlavourText"/>)
		/// </para>
		/// </summary>
		public virtual string AlwaysDisplayText => null;

		/// <summary>
		/// The part of the tooltip which can be hidden
		/// </summary>
		public virtual string FlavourText => null;

		public sealed override void SetStaticDefaults() {
			SafeSetStaticDefaults();

			if (ItemName != null)
				DisplayName.SetDefault(ItemName);

			Tooltip.SetDefault("<>");
		}

		public virtual void SafeSetStaticDefaults() { }

		public sealed override void ModifyTooltips(List<TooltipLine> tooltips) {
			int descriptionIndex = tooltips.FindIndex(tl => tl.Text == "<>");
			if (descriptionIndex >= 0) {
				string always = AlwaysDisplayText;
				if (always != null) {
					string[] lines = always.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
					for (int i = 0; i < lines.Length; i++) {
						string line = lines[i];
						tooltips.Insert(descriptionIndex++, new TooltipLine(Mod, "AlwaysText" + i, line));
					}
				}

				var keys = PlayerInput.CurrentProfile.InputModes[InputMode.Keyboard].KeyStatus[TriggerNames.SmartSelect];
				string key = keys.Count == 0 ? "<NOT BOUND>" : keys[0];

				string name = PlayerInput.Triggers.Current.SmartSelect ? "CustomTooltip" : "RevealTooltip";
				string text = PlayerInput.Triggers.Current.SmartSelect ? FlavourText : $"[c/555555:[Press \"{key}\" to view full tooltip.][c/555555:]]";

				if (!PlayerInput.Triggers.Current.SmartSelect)
					tooltips[descriptionIndex] = new TooltipLine(Mod, name, text);
				else {
					var s = text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
					for (int i = 0; i < s.Length; i++)
						tooltips.Insert(descriptionIndex++, new TooltipLine(Mod, i > 0 ? "CustomTooltip" + i : "CustomTooltip", s[i]));
				}
			}

			SafeModifyTooltips(tooltips);

			//Clean up the placeholder line
			int index = tooltips.FindIndex(tl => tl.Text == "<>");
			if (index >= 0)
				tooltips.RemoveAt(index);
		}

		internal int FindCustomTooltipIndex(List<TooltipLine> tooltips) => tooltips.FindIndex(tl => tl.Mod == Mod.Name && tl.Name == "CustomTooltip");

		public virtual void SafeModifyTooltips(List<TooltipLine> tooltips) { }
	}
}
