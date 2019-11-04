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
			item.width = 24;
			item.height = 24;
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

			NPCs.Draek.DraekP2Head.NormalModeDrops(player: player, quickSpawn: true);
		}

		public override int BossBagNPC => ModContent.NPCType<NPCs.Draek.DraekP2Head>();
	}
}
