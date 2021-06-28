using CosmivengeonMod.Enums;
using CosmivengeonMod.NPCs.Bosses.DraekBoss;
using CosmivengeonMod.Utility;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Spawning.Boss{
	public class DraekSummon : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Mysterious Geode");
			Tooltip.SetDefault("Challenges the defender of the forest\nMust be used in the Forest biome");
		}

		public override void SetDefaults(){
			item.width = 20;
			item.height = 20;
			item.maxStack = 99;
			item.rare = ItemRarityID.Pink;
			item.useAnimation = 45;
			item.useTime = 45;
			item.useStyle = ItemUseStyleID.HoldingUp;
			item.UseSound = new Terraria.Audio.LegacySoundStyle(SoundID.Roar, 0);
			item.consumable = true;
		}

		public override bool CanUseItem(Player player)
			=> MiscUtils.TrySummonBoss(CosmivengeonBoss.Draek, player);

		public override bool UseItem(Player player)
			=> MiscUtils.SummonBossNearPlayer(player, ModContent.NPCType<Draek>(), 50f);

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.StoneBlock, 50);
			recipe.AddIngredient(ItemID.Emerald, 1);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
