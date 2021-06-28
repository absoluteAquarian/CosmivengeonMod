using CosmivengeonMod.Items.Materials;
using CosmivengeonMod.Projectiles.Global;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Weapons.Crystalice{
	public class CrystaliceBow : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Crystalice Bow");
			Tooltip.SetDefault("A sleek, frozen bow." +
				"\nCauses all arrows fired from it to inflict [c/6fa8dc:Frostburn].");
		}

		public override void SetDefaults(){
			item.ranged = true;
			item.damage = 12;
			item.knockBack = 3.1f;
			item.useTime = 25;
			item.useAnimation = 25;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.width = 28;
			item.height = 58;
			item.scale = 0.8f;
			item.noMelee = true;
			item.useAmmo = AmmoID.Arrow;
			item.shoot = ProjectileID.PurificationPowder;
			item.shootSpeed = 8f;
			item.UseSound = SoundID.Item5;
			item.value = Item.sellPrice(silver: 1, copper: 75);
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<FrostCrystal>());
			recipe.AddIngredient(ItemID.SnowBlock, 10);
			recipe.AddIngredient(ItemID.IceBlock, 10);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack){
			Projectile proj = Main.projectile[Projectile.NewProjectile(position, new Vector2(speedX, speedY), type, damage, knockBack, player.whoAmI)];
			proj.GetGlobalProjectile<WeaponAffectedProjectile>().shotFromCrystaliceBow = true;
			return false;
		}
	}
}
