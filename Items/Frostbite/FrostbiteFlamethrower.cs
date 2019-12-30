using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Frostbite{
	public class FrostbiteFlamethrower : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Frostfire's Breath");
			Tooltip.SetDefault("Shoots Frostburn-inflicing flames at a quick rate." +
				"\nUses [c/00dddd:Ice Blocks] for ammo.");
		}

		public override void SetDefaults(){
			item.width = 64;
			item.height = 24;
			item.ranged = true;
			item.noMelee = true;
			item.damage = 8;
			item.knockBack = 2.6f;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.useAmmo = ItemID.IceBlock;
			item.useTime = 6;
			item.useAnimation = 6;
			item.autoReuse = true;
			item.shoot = ModContent.ProjectileType<Projectiles.Frostbite.FrostbiteBreath>();
			item.shootSpeed = 11f;
			item.value = Item.sellPrice(silver: 4, copper: 60);
			item.UseSound = SoundID.Item34;
			item.rare = ItemRarityID.Blue;
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack){
			Vector2 newVelocity = new Vector2(speedX, speedY).RotatedByRandom(MathHelper.ToRadians(2f));
			Vector2 newPosition = Vector2.Normalize(newVelocity) * 4f * 16f;

			newPosition += ((Vector2)HoldoutOffset()).RotatedBy(newVelocity.ToRotation());

			if(Collision.CanHit(position, 0, 0, position + newPosition, 0, 0))
				position += newPosition;

			Projectile.NewProjectile(
				position,
				newVelocity,
				type,
				damage,
				knockBack,
				item.owner,
				0f,
				1f
			);
			
			return false;
		}

		public override Vector2? HoldoutOffset() => new Vector2(-6, 2);
	}
}