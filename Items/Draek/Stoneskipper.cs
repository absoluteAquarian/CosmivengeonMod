using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using CosmivengeonMod.Projectiles.Weapons.Draek;
using Microsoft.Xna.Framework;

namespace CosmivengeonMod.Items.Draek{
	public class Stoneskipper : ModItem{

		public static float ShootSpeed = 28f;
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Stoneskipper");
			Tooltip.SetDefault("Lightweight gun that fires fast, high-accuracy poison bullets");
		}

		public override void SetDefaults(){
			item.damage = 58;
			item.ranged = true;
			item.width = 66;
			item.height = 22;
			item.useTime = 42;
			item.useAnimation = 42;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.noMelee = true;
			item.knockBack = 8.7f;
			item.value = Item.sellPrice(0, 2, 50, 0);
			item.rare = ItemRarityID.Green;
			item.UseSound = SoundID.Item41;
			item.autoReuse = false;
			item.shoot = ProjectileID.PurificationPowder;		//Vanilla guns have this value, but it really doesn't matter since it's overwritten in Shoot()
			item.useAmmo = AmmoID.Bullet;

			item.crit = 25;  //+25% crit chance
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.StoneBlock, 50);
			recipe.AddIngredient(ModContent.ItemType<DraekScales>(), 15);
			recipe.AddIngredient(ModContent.ItemType<RaechonShell>());
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack){
			type = ModContent.ProjectileType<StoneskipperProjectile>();
			return true;
		}

		public override Vector2? HoldoutOffset(){
			return new Vector2(-10, 0);
		}
	}
}
