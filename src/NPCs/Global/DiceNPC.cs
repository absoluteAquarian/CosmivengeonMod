using CosmivengeonMod.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.NPCs.Global {
	public class DiceNPC : GlobalNPC {
		public override void SetupShop(int type, Chest shop, ref int nextSlot) {
			DicePlayer dp = Main.LocalPlayer.GetModPlayer<DicePlayer>();

			if (!dp.badShop && !dp.goodShop)
				return;

			float modifier = dp.badShop ? 1.25f : 0.85f;

			foreach (Item item in shop.item) {
				if (item.IsAir)
					continue;

				// Increase or decrease vanilla item prices in the NPC's shop
				if (item.shopSpecialCurrency == CustomCurrencyID.None)
					item.shopCustomPrice = (int)(item.GetStoreValue() * modifier);
			}
		}
	}
}
