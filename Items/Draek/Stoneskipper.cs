using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using CosmivengeonMod.Projectiles.Weapons;
using Microsoft.Xna.Framework;

namespace CosmivengeonMod.Items.Draek{
	public class Stoneskipper : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Stoneskipper");
			Tooltip.SetDefault("Description to be added.");
		}

		public override void SetDefaults(){
			item.damage = 60;
			item.ranged = true;
			item.width = 66;
			item.height = 22;
			item.useTime = 40;
			item.useAnimation = 40;
			item.useStyle = 5;
			item.noMelee = true;
			item.knockBack = 10f;
			item.value = Item.sellPrice(0, 2, 50, 0);
			item.rare = 2;
			item.UseSound = SoundID.Item41;
			item.autoReuse = false;
			item.shoot = 10;		//Vanilla guns have this value, but it really doesn't matter since it's overwritten in Shoot()
			item.shootSpeed = 40f;
			item.useAmmo = AmmoID.Bullet;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.StoneBlock, 50);
			recipe.AddIngredient(mod.ItemType<DraekScales>(), 15);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack){
			if(type == ProjectileID.Bullet)
				type = mod.ProjectileType<StoneskipperProjectile>();
			return true;
		}

		public override Vector2? HoldoutOffset(){
			return new Vector2(-10, 0);
		}
	}
}
