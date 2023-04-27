using CosmivengeonMod.Enums;
using CosmivengeonMod.NPCs.Bosses.DraekBoss;
using CosmivengeonMod.Utility;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Spawning.Boss {
	public class DraekSummon : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Mysterious Geode");
			Tooltip.SetDefault("Challenges the defender of the forest\nMust be used in the Forest biome");

			ItemID.Sets.SortingPriorityBossSpawns[Type] = 12; // This helps sort inventory know that this is a boss summoning Item.

			SacrificeTotal = 3;
		}

		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 20;
			Item.maxStack = 99;
			Item.rare = ItemRarityID.Pink;
			Item.useAnimation = 45;
			Item.useTime = 45;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.UseSound = SoundID.Roar;
			Item.consumable = true;
		}

		public override bool CanUseItem(Player player)
			=> MiscUtils.TrySummonBoss(CosmivengeonBoss.Draek, player);

		public override bool? UseItem(Player player)
			=> MiscUtils.SummonBossNearPlayer(player, ModContent.NPCType<Draek>(), 50f) ? true : null;

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.StoneBlock, 50);
			recipe.AddIngredient(ItemID.Emerald, 1);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}
