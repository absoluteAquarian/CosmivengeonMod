using CosmivengeonMod.Items.Materials;
using CosmivengeonMod.Projectiles.Global;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Weapons.Crystalice {
	public class CrystaliceBow : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Crystalice Bow");
			Tooltip.SetDefault("A sleek, frozen bow." +
				"\nCauses all arrows fired from it to inflict [c/6fa8dc:Frostburn].");
		}

		public override void SetDefaults() {
			Item.DamageType = DamageClass.Ranged;
			Item.damage = 12;
			Item.knockBack = 3.1f;
			Item.useTime = 25;
			Item.useAnimation = 25;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.width = 28;
			Item.height = 58;
			Item.scale = 0.8f;
			Item.noMelee = true;
			Item.useAmmo = AmmoID.Arrow;
			Item.shoot = ProjectileID.PurificationPowder;
			Item.shootSpeed = 8f;
			Item.UseSound = SoundID.Item5;
			Item.value = Item.sellPrice(silver: 1, copper: 75);
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<FrostCrystal>());
			recipe.AddIngredient(ItemID.SnowBlock, 10);
			recipe.AddIngredient(ItemID.IceBlock, 10);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Projectile proj = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);
			proj.GetGlobalProjectile<WeaponAffectedProjectile>().shotFromCrystaliceBow = true;
			return false;
		}
	}
}
