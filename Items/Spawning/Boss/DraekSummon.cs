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
			Item.width = 20;
			Item.height = 20;
			Item.maxStack = 99;
			Item.rare = ItemRarityID.Pink;
			Item.useAnimation = 45;
			Item.useTime = 45;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.UseSound = new Terraria.Audio.LegacySoundStyle(SoundID.Roar, 0);
			Item.consumable = true;
		}

		public override bool CanUseItem(Player player)
			=> MiscUtils.TrySummonBoss(CosmivengeonBoss.Draek, player);

		public override bool? UseItem(Player player)/* tModPorter Suggestion: Return null instead of false */
			=> MiscUtils.SummonBossNearPlayer(player, ModContent.NPCType<Draek>(), 50f);

		public override void AddRecipes(){
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.StoneBlock, 50);
			recipe.AddIngredient(ItemID.Emerald, 1);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}
