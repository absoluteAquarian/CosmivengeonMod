using CosmivengeonMod.Items.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Weapons.Draek {
	public class Scalestorm : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Scalestorm");
			Tooltip.SetDefault("An elegant bow that rapidly fires arrows to their mark.");
		}

		public override void SetDefaults() {
			Item.damage = 17;
			Item.DamageType = DamageClass.Ranged;
			Item.scale = 0.5f;
			Item.width = 42;
			Item.height = 108;
			Item.useTime = 22;
			Item.useAnimation = 22;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 4;
			Item.value = Item.sellPrice(0, 2, 50, 0);
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item5;
			Item.autoReuse = true;
			Item.shoot = ProjectileID.PurificationPowder;       //Needed to shoot projectiles
			Item.shootSpeed = 19f;
			Item.useAmmo = AmmoID.Arrow;
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.StoneBlock, 50);
			recipe.AddIngredient(ModContent.ItemType<DraekScales>(), 15);
			recipe.AddIngredient(ModContent.ItemType<RaechonShell>());
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}

		public override Vector2? HoldoutOffset() => new Vector2(-6, 0);

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
			Texture2D texture = TextureAssets.Item[Item.type].Value;

			Vector2 vector = Item.Top - Main.screenPosition + new Vector2(-2, 6);

			spriteBatch.Draw(texture, vector, null, lightColor, rotation, new Vector2(Item.width / 2, Item.width / 2), Item.scale, SpriteEffects.None, 0);

			return false;
		}
	}
}
