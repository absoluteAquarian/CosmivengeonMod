using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Draek{
	public class Scalestorm : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Scalestorm");
			Tooltip.SetDefault("An elegant bow that rapidly fires arrows to their mark.");
		}

		public override void SetDefaults() {
			item.damage = 25;
			item.ranged = true;
			item.scale = 0.5f;
			item.width = (int)(42 * item.scale);
			item.height = (int)(108 * item.scale);
			item.useTime = 20;
			item.useAnimation = 20;
			item.useStyle = 5;
			item.noMelee = true;
			item.knockBack = 4;
			item.value = Item.sellPrice(0, 2, 50, 0);
			item.rare = 2;
			item.UseSound = SoundID.Item5;
			item.autoReuse = true;
			item.shoot = 10;		//Needed to shoot projectiles
			item.shootSpeed = 24f;
			item.useAmmo = AmmoID.Arrow;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.StoneBlock, 50);
			recipe.AddIngredient(ModContent.ItemType<DraekScales>(), 15);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		public override Vector2? HoldoutOffset() => new Vector2(-12, 0);

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI){
			Texture2D texture = Main.itemTexture[item.type];

			Vector2 vector = item.Top - Main.screenPosition + new Vector2(-2, 6);

			spriteBatch.Draw(texture, vector, null, lightColor, rotation, new Vector2(item.width / 2, item.width / 2), item.scale, SpriteEffects.None, 0);
			
			return false;
		}
	}
}
