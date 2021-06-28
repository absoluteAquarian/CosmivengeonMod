using CosmivengeonMod.Items.Equippable.Accessories.Frostbite;
using CosmivengeonMod.NPCs.Bosses.FrostbiteBoss;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Bags{
	public class FrostbiteBag : ModItem{
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
			if(Main.hardMode)
				player.TryGettingDevArmor();

			player.QuickSpawnItem(ModContent.ItemType<EyeOfTheBlizzard>());

			Frostbite.NormalModeDrops(player: player, quickSpawn: true);
		}

		public override int BossBagNPC => ModContent.NPCType<Frostbite>();
	}
}
