using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Frostbite{
	public class CrystaliceStaff : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Crystalice Staff");
			Tooltip.SetDefault("Summons a baby Ice Prowler to fight for you.");
		}

		public override void SetDefaults(){
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.shoot = ModContent.ProjectileType<Projectiles.Summons.BabyIceProwler>();
			item.width = 48;
			item.height = 48;
			item.UseSound = SoundID.Item44;
			item.useAnimation = 30;
			item.useTime = 30;
			item.rare = ItemRarityID.Green;
			item.noMelee = true;
			item.value = Item.sellPrice(silver: 60);
			item.buffType = ModContent.BuffType<Buffs.BabyIceProwlerBuff>();
			item.mana = 8;
			item.damage = 15;
			item.knockBack = 1.72f;
			item.summon = true;
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
