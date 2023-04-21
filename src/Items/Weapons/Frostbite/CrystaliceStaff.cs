using CosmivengeonMod.Buffs.Minions;
using CosmivengeonMod.Projectiles.Summons;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Weapons.Frostbite {
	public class CrystaliceStaff : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Crystalice Staff");
			Tooltip.SetDefault("Summons a baby Ice Prowler to fight for you.");
		}

		public override void SetDefaults() {
			Item.useStyle = ItemUseStyleID.Swing;
			Item.shoot = ModContent.ProjectileType<BabyIceProwler>();
			Item.width = 48;
			Item.height = 48;
			Item.UseSound = SoundID.Item44;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.rare = ItemRarityID.Green;
			Item.noMelee = true;
			Item.value = Item.sellPrice(silver: 60);
			Item.buffType = ModContent.BuffType<BabyIceProwlerBuff>();
			Item.mana = 8;
			Item.damage = 15;
			Item.knockBack = 1.72f;
			Item.DamageType = DamageClass.Summon;
		}

		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
				player.AddBuff(Item.buffType, 3600);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			position = Main.MouseWorld;  //Make the summon spawn at the cursor
			return true;
		}
	}
}
