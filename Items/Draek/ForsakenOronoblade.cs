using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using CosmivengeonMod.Projectiles.Weapons;

namespace CosmivengeonMod.Items.Draek{
	public class ForsakenOronoblade : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Forsaken Oronoblade");
			Tooltip.SetDefault("Description to be added.");
		}
		public override void SetDefaults(){
			item.damage = 50;
			item.melee = true;
			item.width = 40;
			item.height = 40;
			item.useTime = 25;
			item.useAnimation = 25;
			item.useStyle = 1;
			item.knockBack = 5;
			item.value = Item.sellPrice(0, 2, 50, 0);
			item.shoot = mod.ProjectileType<ForsakenOronobladeProjectile>();
			item.shootSpeed = 6f;
			item.rare = 2;
			item.scale = 0.7f;
			item.UseSound = SoundID.Item1;
			item.autoReuse = true;
		}
		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.StoneBlock, 50);
			recipe.AddIngredient(mod.ItemType<DraekScales>(), 15);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		private int projectilesShot = 0;

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack){
			//Only shoot a projectile every 5 swings
			damage = (int)(item.damage * 0.35f);
			if(++projectilesShot % 5 == 0){
				Main.PlaySound(SoundID.Item43, position);
				return true;
			}
			return false;
		}
	}
}
