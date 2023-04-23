using CosmivengeonMod.Items.Materials;
using CosmivengeonMod.Projectiles.Weapons.Draek;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Weapons.Draek {
	public class Stoneskipper : ModItem {

		public static float ShootSpeed = 28f;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Stoneskipper");
			Tooltip.SetDefault("Lightweight gun that fires fast, high-accuracy poison bullets");
		}

		public override void SetDefaults() {
			Item.damage = 58;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 66;
			Item.height = 22;
			Item.useTime = 42;
			Item.useAnimation = 42;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 8.7f;
			Item.value = Item.sellPrice(0, 2, 50, 0);
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item41;
			Item.autoReuse = false;
			Item.shoot = ProjectileID.PurificationPowder;  //Vanilla guns have this value, but it really doesn't matter since it's overwritten in Shoot()
			Item.useAmmo = AmmoID.Bullet;

			Item.crit = 25;  //+25% crit chance
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.StoneBlock, 50);
			recipe.AddIngredient(ModContent.ItemType<DraekScales>(), 15);
			recipe.AddIngredient(ModContent.ItemType<RaechonShell>());
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			type = ModContent.ProjectileType<StoneskipperProjectile>();
		}

		public override Vector2? HoldoutOffset() {
			return new Vector2(-10, 0);
		}
	}
}
