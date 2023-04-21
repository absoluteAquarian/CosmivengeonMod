using CosmivengeonMod.Projectiles.Weapons.Frostbite;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Weapons.Frostbite{
	public class FrostRifle : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("I-SKL");
			Tooltip.SetDefault("Fires fast, piercing icicles which inflict [c/00ffff:Frostburn] on hit" +
				"\nUses [c/00ffff:Ice Blocks] for ammo");
		}

		public override void SetDefaults(){
			Item.width = 60;
			Item.height = 16;
			Item.DamageType = DamageClass.Ranged;
			Item.noMelee = true;
			Item.useTime = 45;
			Item.useAnimation = 45;
			Item.shoot = ModContent.ProjectileType<FrostRifleProjectile>();
			Item.useAmmo = ItemID.IceBlock;
			Item.UseSound = SoundID.Item40;
			Item.shootSpeed = 14f;	//actual speed: 28px/frame
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.damage = 32;
			Item.knockBack = 8.7f;
			Item.value = Item.sellPrice(silver: 5);
			Item.rare = ItemRarityID.Blue;
		}

		public override Vector2? HoldoutOffset() => new Vector2(-10, 0);
	}
}
