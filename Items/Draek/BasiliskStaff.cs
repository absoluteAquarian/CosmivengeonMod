using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Draek{
	public class BasiliskStaff : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Basilisk Staff");
			Tooltip.SetDefault("Summons a baby basilisk to fight for you.");
		}

		public override void SetDefaults(){
			item.useStyle = 1;
			item.shoot = ModContent.ProjectileType<Projectiles.Summons.BabySnek>();
			item.scale = 0.6667f;
			item.width = 80;
			item.height = 80;
			item.UseSound = SoundID.Item44;
			item.useAnimation = 30;
			item.useTime = 30;
			item.rare = ItemRarityID.Green;
			item.noMelee = true;
			item.value = Item.sellPrice(gold: 1, silver: 50);
			item.buffType = ModContent.BuffType<Buffs.BabyBasiliskBuff>();
			item.mana = 12;
			item.damage = 15;
			item.knockBack = 2f;
			item.summon = true;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.StoneBlock, 50);
			recipe.AddIngredient(ModContent.ItemType<DraekScales>(), 15);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		public override void UseStyle(Player player){
			if(player.whoAmI == Main.myPlayer && player.itemTime == 0)
				player.AddBuff(item.buffType, 3600);
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack){
			position = Main.MouseWorld;		//Make the summon spawn at the cursor
			return true;
		}
	}
}