using CosmivengeonMod.Projectiles.NPCSpawned.FrostbiteBoss;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Weapons.Frostbite {
	public class FrostbiteFlamethrower : ModItem {
		protected override bool CloneNewInstances => true;

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Frostfire's Breath");
			Tooltip.SetDefault("Shoots Frostburn-inflicing flames at a quick rate." +
				"\nUses [c/00dddd:Ice Blocks] for ammo." +
				"\nHas a 35% chance to not consume ammo.");
		}

		public override void SetDefaults() {
			Item.width = 64;
			Item.height = 24;
			Item.DamageType = DamageClass.Ranged;
			Item.noMelee = true;
			Item.damage = 9;
			Item.knockBack = 2.6f;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAmmo = ItemID.IceBlock;
			Item.useTime = 6;
			Item.useAnimation = 6;
			Item.autoReuse = true;
			Item.shoot = ModContent.ProjectileType<FrostbiteBreath>();
			Item.shootSpeed = 11f;
			Item.value = Item.sellPrice(silver: 4, copper: 60);
			Item.UseSound = SoundID.Item34;
			Item.rare = ItemRarityID.Blue;
		}

		public override bool CanConsumeAmmo(Item ammo, Player player)
			=> Main.rand.NextFloat() < 0.35f;

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Vector2 newVelocity = new Vector2(speedX, speedY).RotatedByRandom(MathHelper.ToRadians(2f));
			Vector2 newPosition = Vector2.Normalize(newVelocity) * 4f * 16f;

			newPosition += ((Vector2)HoldoutOffset()).RotatedBy(newVelocity.ToRotation());

			if (Collision.CanHit(position, 0, 0, position + newPosition, 0, 0))
				position += newPosition;

			Projectile.NewProjectile(
				position,
				newVelocity,
				type,
				damage,
				knockBack,
				Item.playerIndexTheItemIsReservedFor,
				0f,
				1f
			);

			return false;
		}

		public override Vector2? HoldoutOffset() => new Vector2(-6, 2);
	}
}
