using CosmivengeonMod.Enums;
using CosmivengeonMod.Items.Materials;
using CosmivengeonMod.NPCs.Bosses.FrostbiteBoss;
using CosmivengeonMod.Utility;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Spawning.Boss {
	public class IcyLure : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Icy Lure");
			Tooltip.SetDefault("Attracts an elemental frost lizard." +
				"\nMust be used in the Snow biome.");
		}

		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 20;
			Item.maxStack = 99;
			Item.rare = ItemRarityID.Green;
			Item.useAnimation = 45;
			Item.useTime = 45;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.UseSound = new Terraria.Audio.LegacySoundStyle(SoundID.Roar, 0);
			Item.consumable = true;
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<FrostCrystal>(), 2);
			recipe.AddIngredient(ItemID.SnowBlock, 50);
			recipe.AddIngredient(ItemID.IceBlock, 25);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}

		public override bool CanUseItem(Player player)
			=> MiscUtils.TrySummonBoss(CosmivengeonBoss.Frostbite, player);

		public override bool? UseItem(Player player)/* tModPorter Suggestion: Return null instead of false */
			=> MiscUtils.SummonBossAbovePlayer(player, ModContent.NPCType<Frostbite>(), -50 * 16, 50 * 16, -10 * 16, -5 * 16);
	}
}
