using CosmivengeonMod.Items.Materials;
using CosmivengeonMod.Projectiles.Weapons.Draek;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Weapons.Draek{
	public class SlitherWand : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Slither Wand");
			Tooltip.SetDefault("Calls forth a lesser wyrm, which launches from the ground at the cursor's x-position.\nWyrm will cut through anything in its path, before falling back into the ground.");

			Item.staff[Item.type] = true;
		}

		public override void SetDefaults(){
			Item.autoReuse = false;
			Item.rare = ItemRarityID.Green;
			Item.mana = 24;
			Item.UseSound = SoundID.Item70;
			Item.noMelee = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.damage = 40;
			Item.useAnimation = 45;
			Item.useTime = 45;
			Item.width = 28;
			Item.height = 30;
			Item.shoot = ModContent.ProjectileType<SlitherWandProjectile_Head>();
			Item.knockBack = 3f;
			Item.DamageType = DamageClass.Magic;
			Item.value = Item.sellPrice(0, 2, 50, 0);

			//Needed to make the staff point
			Item.shootSpeed = 1f;
		}

		public override void AddRecipes(){
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.StoneBlock, 50);
			recipe.AddIngredient(ModContent.ItemType<DraekScales>(), 15);
			recipe.AddIngredient(ModContent.ItemType<RaechonShell>());
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}

		public override Vector2? HoldoutOffset() => new Vector2(-5, -8);

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback){
			speedX = 0;
			speedY = 0;
			return true;
		}
	}
}
