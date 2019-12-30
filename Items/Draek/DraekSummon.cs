using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace CosmivengeonMod.Items.Draek{
	public class DraekSummon : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Mysterious Geode");
			Tooltip.SetDefault("Challenges the defender of the forest\nMust be used in the Forest biome");
		}

		public override void SetDefaults(){
			item.width = 20;
			item.height = 20;
			item.maxStack = 99;
			item.rare = 5;
			item.useAnimation = 45;
			item.useTime = 45;
			item.useStyle = 4;
			item.UseSound = new Terraria.Audio.LegacySoundStyle(SoundID.Roar, 0);
			item.consumable = true;
		}

		public override bool CanUseItem(Player player)
			=> CosmivengeonUtils.TrySummonBoss(CosmivengeonBoss.Draek, player);

		public override bool UseItem(Player player)
			=> CosmivengeonUtils.SummonBossNearPlayer(player, ModContent.NPCType<NPCs.Draek.Draek>(), 50f);

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
