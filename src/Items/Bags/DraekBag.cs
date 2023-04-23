using CosmivengeonMod.Items.Equippable.Accessories.Draek;
using CosmivengeonMod.Items.Weapons.Draek;
using CosmivengeonMod.NPCs.Bosses.DraekBoss;
using CosmivengeonMod.Utility;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;

namespace CosmivengeonMod.Items.Bags {
	public class DraekBag : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Treasure Bag");
			Tooltip.SetDefault("{$CommonItemTooltip.RightClickToOpen}");
		}

		public override void SetDefaults() {
			Item.maxStack = 999;
			Item.consumable = true;
			Item.width = 32;
			Item.height = 32;
			Item.rare = ItemRarityID.Cyan;
			Item.expert = true;
		}

		public override bool CanRightClick() {
			return true;
		}

		public override void ModifyItemLoot(ItemLoot itemLoot) {
			itemLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(ModContent.NPCType<DraekP2Head>()));

			DraekP2Head.AddDrops(itemLoot, restrictNormalDrops: false);

			itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<JewelOfOronitus>()));
			itemLoot.Add(ItemDropRule.ByCondition(new LootConditions.DesolationMode(), ModContent.ItemType<TerraBolt>()));
		}
	}
}
