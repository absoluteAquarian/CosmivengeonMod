using CosmivengeonMod.API;
using CosmivengeonMod.Worlds;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Global {
	public class ModeDisabler : GlobalItem {
		public override bool CanUseItem(Item item, Player player) {
			//Check if this item is from Calamity
			bool disableModeDisabler = true;
			if (!disableModeDisabler && ModReferences.Calamity.Active && item.ModItem?.Mod == ModReferences.Calamity) {
				ModItem revengeanceItem = ModReferences.Calamity.Instance.GetItem("Revenge");
				ModItem deathItem = ModReferences.Calamity.Instance.GetItem("Death");

				//If the item is either of these types, disable Desolation mode if it is active
				//However, if the item is Death and revengeance isn't active, don't do anything
				if (item.type == deathItem.Item.type && !(bool)ModReferences.Calamity.Call("Difficulty", "Rev"))
					return false;
				if (WorldEvents.desoMode && item.type == revengeanceItem.Item.type || item.type == deathItem.Item.type)
					WorldEvents.desoMode = false;
			}

			return true;
		}
	}
}
