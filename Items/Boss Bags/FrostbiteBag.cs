using CosmivengeonMod.Items.Frostbite;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Boss_Bags{
	public class FrostbiteBag : ModItem{
		public override string Texture => "CosmivengeonMod/Items/Boss Bags/FrostbiteBag";

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Treasure Bag");
			Tooltip.SetDefault("{$CommonItemTooltip.RightClickToOpen}");
		}

		public override void SetDefaults(){
			item.maxStack = 999;
			item.consumable = true;
			item.width = 40;
			item.height = 36;
			item.rare = ItemRarityID.Expert;
			item.expert = true;
		}

		public override bool CanRightClick() => true;

		public override void OpenBossBag(Player player){
			player.TryGettingDevArmor();

			player.QuickSpawnItem(ModContent.ItemType<EyeOfTheBlizzard>());

			NPCs.Frostbite.Frostbite.NormalModeDrops(player: player, quickSpawn: true);
		}

		public override int BossBagNPC => ModContent.NPCType<NPCs.Frostbite.Frostbite>();
	}
}
