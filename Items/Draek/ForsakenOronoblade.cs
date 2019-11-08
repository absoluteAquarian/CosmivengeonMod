using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using CosmivengeonMod.Projectiles.Weapons;
using Microsoft.Xna.Framework.Graphics;

namespace CosmivengeonMod.Items.Draek{
	public class ForsakenOronoblade : ModItem{
		public override bool OnlyShootOnSwing => true;

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Forsaken Oronoblade");
			Tooltip.SetDefault("Occasionally Launches green energy blasts when swung" +
				"\nAn ancient blade, forged millennia ago and left to" +
				"\ndegrade over time.  Appears to have recently been reclaimed" +
				"\nby Draek, though its condition is still unstable.  Perhaps" +
				"\nwith the right tools, it could be reforged to its original state...");
		}
		public override void SetDefaults(){
			item.damage = 30;
			item.melee = true;
			item.useTurn = true;
			item.width = 40;
			item.height = 40;
			item.useTime = 19;
			item.useAnimation = 19;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.knockBack = 6.3f;
			item.value = Item.sellPrice(0, 2, 50, 0);
			item.shoot = 10;
			item.shootSpeed = 9f;
			item.rare = 2;
			item.scale = 0.86f;
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

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack){
			if(Main.rand.NextFloat() < 0.75f){
				Main.PlaySound(SoundID.Item43.WithVolume(0.5f), position);
				type = ModContent.ProjectileType<ForsakenOronobladeProjectile>();
				damage = (int)(item.damage * 0.6667f);
				return true;
			}
			return false;
		}

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI){
			Texture2D texture = Main.itemTexture[item.type];

			spriteBatch.Draw(texture, item.position - Main.screenPosition, null, lightColor, rotation, new Vector2(item.width / 2, item.width / 2), item.scale, SpriteEffects.None, 0);
			
			return false;
		}
	}
}
