using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using CosmivengeonMod.Projectiles.Weapons;

namespace CosmivengeonMod.Items.Draek{
	public class ForsakenOronoblade : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Forsaken Oronoblade");
			Tooltip.SetDefault("Occasionally Launches green energy blasts when swung\nAn ancient blade, forged millennia ago and left to degrade over time.\nAppears to have recently been reclaimed by Draek, though its condition is still unstable.\nPerhaps with the right tools, it could be reforged to its original state...");
		}
		public override void SetDefaults(){
			item.damage = 35;
			item.melee = true;
			item.width = 40;
			item.height = 40;
			item.useTime = 12;
			item.useAnimation = 12;
			item.useStyle = 1;
			item.knockBack = 5;
			item.value = Item.sellPrice(0, 2, 50, 0);
			item.shoot = ModContent.ProjectileType<ForsakenOronobladeProjectile>();
			item.shootSpeed = 9f;
			item.rare = 2;
			item.scale = 1f;
			item.UseSound = SoundID.Item1;
			item.autoReuse = true;
		}
		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.StoneBlock, 50);
			recipe.AddIngredient(ModContent.ItemType<DraekScales>(), 15);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

//		private int projectilesShot = 0;

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack){
			Main.PlaySound(SoundID.Item43, position);
			return true;
		}
	}
}
