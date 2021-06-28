using CosmivengeonMod.Enums;
using CosmivengeonMod.Items.Materials;
using CosmivengeonMod.NPCs.Bosses.FrostbiteBoss;
using CosmivengeonMod.Utility;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Spawning.Boss{
	public class IcyLure : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Icy Lure");
			Tooltip.SetDefault("Attracts an elemental frost lizard." +
				"\nMust be used in the Snow biome.");
		}

		public override void SetDefaults(){
			item.width = 20;
			item.height = 20;
			item.maxStack = 99;
			item.rare = ItemRarityID.Green;
			item.useAnimation = 45;
			item.useTime = 45;
			item.useStyle = ItemUseStyleID.HoldingUp;
			item.UseSound = new Terraria.Audio.LegacySoundStyle(SoundID.Roar, 0);
			item.consumable = true;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<FrostCrystal>(), 2);
			recipe.AddIngredient(ItemID.SnowBlock, 50);
			recipe.AddIngredient(ItemID.IceBlock, 25);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		public override bool CanUseItem(Player player)
			=> MiscUtils.TrySummonBoss(CosmivengeonBoss.Frostbite, player);

		public override bool UseItem(Player player)
			=> MiscUtils.SummonBossAbovePlayer(player, ModContent.NPCType<Frostbite>(), -50 * 16, 50 * 16, -10 * 16, -5 * 16);
	}
}
