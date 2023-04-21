using CosmivengeonMod.Items.Materials;
using CosmivengeonMod.Projectiles.Weapons.Draek;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Weapons.Draek{
	public class ForsakenOronoblade : ModItem{
		public override bool OnlyShootOnSwing/* tModPorter Note: Removed. If you returned true, set Item.useTime to a multiple of Item.useAnimation */ => true;

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Forsaken Oronoblade");
			Tooltip.SetDefault("Occasionally Launches green energy blasts when swung" +
				"\nAn ancient blade, forged millennia ago and left to" +
				"\ndegrade over time.  Appears to have recently been reclaimed" +
				"\nby Draek, though its condition is still unstable.  Perhaps" +
				"\nwith the right tools, it could be reforged to its original state...");
		}
		public override void SetDefaults(){
			Item.damage = 26;
			Item.DamageType = DamageClass.Melee/* tModPorter Suggestion: Consider MeleeNoSpeed for no attack speed scaling */;
			Item.useTurn = true;
			Item.width = 32;
			Item.height = 32;	//width and height determine dropped item's hitbox, not the actual hitbox of the sword
			Item.useTime = 23;
			Item.useAnimation = 23;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 6.3f;
			Item.value = Item.sellPrice(0, 2, 50, 0);
			Item.shoot = ModContent.ProjectileType<ForsakenOronobladeProjectile>();
			Item.shootSpeed = ForsakenOronobladeProjectile.ShootVelocity;
			Item.rare = ItemRarityID.Green;
			Item.scale = 0.86f;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
		}
		public override void AddRecipes(){
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.StoneBlock, 50);
			recipe.AddIngredient(ModContent.ItemType<DraekScales>(), 15);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback){
			if(Main.rand.NextFloat() < 0.75f){
				SoundEngine.PlaySound(SoundID.Item43.WithVolume(0.5f), position);
				damage = (int)(Item.damage * 0.6667f);
				return true;
			}
			return false;
		}

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI){
			scale = Item.scale;
			return true;
		}
	}
}
