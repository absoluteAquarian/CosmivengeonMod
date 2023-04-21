using CosmivengeonMod.Items.Equippable.Accessories.Draek;
using CosmivengeonMod.Items.Weapons.Draek;
using CosmivengeonMod.NPCs.Bosses.DraekBoss;
using CosmivengeonMod.Utility;
using CosmivengeonMod.Worlds;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Bags{
	public class DraekBag : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Treasure Bag");
			Tooltip.SetDefault("{$CommonItemTooltip.RightClickToOpen}");
		}

		public override void SetDefaults(){
			Item.maxStack = 999;
			Item.consumable = true;
			Item.width = 32;
			Item.height = 32;
			Item.rare = ItemRarityID.Cyan;
			Item.expert = true;
		}

		public override bool CanRightClick(){
			return true;
		}

		public override void OpenBossBag(Player player){
			if(Main.hardMode)
				player.TryGettingDevArmor();

			if(Main.expertMode && WorldEvents.desoMode && !WorldEvents.obtainedDesolator_DraekBoss){
				player.QuickSpawnItem(ModContent.ItemType<TerraBolt>());
				WorldEvents.obtainedDesolator_DraekBoss = true;

				Debug.CheckWorldFlagUpdate(nameof(WorldEvents.obtainedDesolator_DraekBoss));
			}

			player.QuickSpawnItem(ModContent.ItemType<JewelOfOronitus>());

			DraekP2Head.NormalModeDrops(player: player, quickSpawn: true);
		}

		public override int BossBagNPC => ModContent.NPCType<DraekP2Head>();
	}
}
