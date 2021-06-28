using CosmivengeonMod.Items.Materials;
using CosmivengeonMod.Projectiles.Weapons.Draek;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Weapons.Draek{
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
			item.damage = 26;
			item.melee = true;
			item.useTurn = true;
			item.width = 32;
			item.height = 32;	//width and height determine dropped item's hitbox, not the actual hitbox of the sword
			item.useTime = 23;
			item.useAnimation = 23;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.knockBack = 6.3f;
			item.value = Item.sellPrice(0, 2, 50, 0);
			item.shoot = ModContent.ProjectileType<ForsakenOronobladeProjectile>();
			item.shootSpeed = ForsakenOronobladeProjectile.ShootVelocity;
			item.rare = ItemRarityID.Green;
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
				damage = (int)(item.damage * 0.6667f);
				return true;
			}
			return false;
		}

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI){
			scale = item.scale;
			return true;
		}
	}
}
