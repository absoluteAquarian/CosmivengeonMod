using CosmivengeonMod.NPCs.Draek;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Boss_Bags{
	public class DraekBag : ModItem{
		public override string Texture => "CosmivengeonMod/Items/Boss Bags/DraekTreasureBag";

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Treasure Bag");
			Tooltip.SetDefault("{$CommonItemTooltip.RightClickToOpen}");
		}

		public override void SetDefaults(){
			item.maxStack = 999;
			item.consumable = true;
			item.width = 32;
			item.height = 32;
			item.rare = 9;
			item.expert = true;
		}

		public override bool CanRightClick(){
			return true;
		}

		public override void OpenBossBag(Player player){
			player.TryGettingDevArmor();

			if(Main.expertMode && CosmivengeonWorld.desoMode && !CosmivengeonWorld.obtainedDesolator_DraekBoss){
				player.QuickSpawnItem(ModContent.ItemType<Draek.TerraBolt>());
				CosmivengeonWorld.obtainedDesolator_DraekBoss = true;

				CosmivengeonWorld.CheckWorldFlagUpdate(nameof(CosmivengeonWorld.obtainedDesolator_DraekBoss));
			}

			player.QuickSpawnItem(ModContent.ItemType<Draek.JewelOfOronitus>());

			DraekP2Head.NormalModeDrops(player: player, quickSpawn: true);
		}

		public override int BossBagNPC => ModContent.NPCType<DraekP2Head>();
	}
}
