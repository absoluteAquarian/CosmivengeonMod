using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CosmivengeonMod.Systems {
	internal class RecipesHandler : ModSystem {
		public override void AddRecipeGroups() {
			RegisterRecipeGroup(CoreMod.RecipeGroups.EvilDrops, ItemID.ShadowScale, new int[] {
				ItemID.ShadowScale, ItemID.TissueSample
			});
			RegisterRecipeGroup(CoreMod.RecipeGroups.EvilBars, ItemID.DemoniteBar, new int[] {
				ItemID.DemoniteBar, ItemID.CrimtaneBar
			});
			RegisterRecipeGroup(CoreMod.RecipeGroups.Tier4Bars, ItemID.GoldBar, new int[] {
				ItemID.GoldBar, ItemID.PlatinumBar
			});
			RegisterRecipeGroup(CoreMod.RecipeGroups.WeirdPlant, ItemID.StrangePlant1, new int[] {
				ItemID.StrangePlant1, ItemID.StrangePlant2, ItemID.StrangePlant3, ItemID.StrangePlant4
			});
		}

		private static void RegisterRecipeGroup(string groupName, int itemForAnyName, int[] validTypes)
			=> RecipeGroup.RegisterGroup(groupName, new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(itemForAnyName)}", validTypes));
	}
}
