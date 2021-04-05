using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace CosmivengeonMod{
	public abstract class HidableTooltip : ModItem{
		public virtual string ItemName => null;

		public virtual string AlwaysDisplayText => null;

		public virtual string FlavourText => null;

		public sealed override void SetStaticDefaults(){
			SafeSetStaticDefaults();

			if(ItemName != null)
				DisplayName.SetDefault(ItemName);

			Tooltip.SetDefault("<>");
		}

		public virtual void SafeSetStaticDefaults(){ }

		public sealed override void ModifyTooltips(List<TooltipLine> tooltips){
			int descriptionIndex = tooltips.FindIndex(tl => tl.text == "<>");
			if(descriptionIndex >= 0){
				var keys = PlayerInput.CurrentProfile.InputModes[InputMode.Keyboard].KeyStatus[TriggerNames.SmartSelect];
				string key = keys.Count == 0 ? "<NOT BOUND>" : keys[0];
				
				string name = Main.LocalPlayer.controlTorch ? "CustomTooltip" : "RevealTooltip";
				string text = Main.LocalPlayer.controlTorch ? FlavourText : $"[c/555555:[Press \"{key}\" to view full tooltip.][c/555555:]]";
				
				if(!Main.LocalPlayer.controlTorch)
					tooltips[descriptionIndex] = new TooltipLine(mod, name, text);
				else{
					var s = text.Split(new char[]{ '\n' }, StringSplitOptions.RemoveEmptyEntries);
					for(int i = 0; i < s.Length; i++)
						tooltips.Insert(descriptionIndex++, new TooltipLine(mod, i > 0 ? "CustomTooltip" + i : "CustomTooltip", s[i]));
				}

				string always = AlwaysDisplayText;
				if(always != null){
					string[] lines = always.Split(new char[]{ '\n' }, StringSplitOptions.RemoveEmptyEntries);
					for(int i = 0; i < lines.Length; i++){
						string line = lines[i];
						tooltips.Insert(descriptionIndex++, new TooltipLine(mod, "AlwaysText" + i, line));
					}
				}
			}

			SafeModifyTooltips(tooltips);

			//Clean up the placeholder line
			int index = tooltips.FindIndex(tl => tl.text == "<>");
			if(index >= 0)
				tooltips.RemoveAt(index);
		}

		internal int FindCustomTooltipIndex(List<TooltipLine> tooltips) => tooltips.FindIndex(tl => tl.mod == mod.Name && tl.Name == "CustomTooltip");

		public virtual void SafeModifyTooltips(List<TooltipLine> tooltips){ }
	}
}
