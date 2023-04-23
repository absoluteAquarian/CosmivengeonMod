using CosmivengeonMod.Items.Weapons.Draek;
using CosmivengeonMod.Utility;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Weapons.Draek {
	public class BoulderChunkProjectile : ModProjectile {
		public const float MAX_VELOCITY = 11.1f;

		public override void SetDefaults() {
			Projectile.width = 40;
			Projectile.height = 40;
			Projectile.aiStyle = 0;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Throwing;
			Projectile.penetrate = 2;
			oldPenetrate = Projectile.penetrate;
		}

		private int oldPenetrate;

		public override void AI() {
			if (oldPenetrate != Projectile.penetrate) {
				if (Projectile.penetrate == 1)
					Projectile.damage = (int)(Projectile.damage * 0.45f);
				oldPenetrate = Projectile.penetrate;
			}

			Projectile.velocity.X *= 0.983f;

			Projectile.velocity.Y += 8f / 60f;

			Projectile.velocity.Y.Clamp(-MAX_VELOCITY, MAX_VELOCITY);

			UpdateRotation();
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			//Create some dust
			Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			//Play the "tile hit" sound
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);

			//Kill the projectile
			Projectile.Kill();

			return false;
		}

		public override void Kill(int timeLeft) {
			//Only spawn one projectile - multiplayer compatability
			if (Projectile.owner == Main.myPlayer && !Projectile.noDropItem) {
				//Drop the item (100% chance)
				int item = Item.NewItem(Projectile.GetSource_DropAsItem(), Projectile.getRect(), ModContent.ItemType<BoulderChunk>());

				//Sync the drop for multiplayer
				if (Main.netMode == NetmodeID.MultiplayerClient && item >= 0)
					NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f);
			}
		}

		private void UpdateRotation() {
			int spinDirection = (Projectile.velocity.X == 0) ? 0 : ((Projectile.velocity.X > 0) ? 1 : -1);
			float spinFactor = Math.Min(
				Math.Max(
					Math.Abs(Projectile.velocity.X / MAX_VELOCITY),
					Math.Abs(Projectile.velocity.Y / MAX_VELOCITY)
				),
				0.25f
			);

			Projectile.rotation += MathHelper.ToRadians(60f) * spinFactor * spinDirection;
		}
	}
}
