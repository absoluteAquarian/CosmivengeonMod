using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using CosmivengeonMod.Items.Draek;

namespace CosmivengeonMod.Projectiles.Weapons.Draek{
	public class BoulderChunkProjectile : ModProjectile{
		public const float MAX_VELOCITY = 11.1f;

		public override void SetDefaults(){
			projectile.width = 40;
			projectile.height = 40;
			projectile.aiStyle = 0;
			projectile.friendly = true;
			projectile.hostile = false;
			projectile.thrown = true;
			projectile.penetrate = 2;
			oldPenetrate = projectile.penetrate;
		}

		private int oldPenetrate;

		public override void AI(){
			if(oldPenetrate != projectile.penetrate){
				if(projectile.penetrate == 1)
					projectile.damage = (int)(projectile.damage * 0.45f);
				oldPenetrate = projectile.penetrate;
			}

			projectile.velocity.X *= 0.983f;
			
			projectile.velocity.Y += 8f / 60f;

			projectile.velocity.Y.Clamp(-MAX_VELOCITY, MAX_VELOCITY);

			UpdateRotation();
		}

		public override bool OnTileCollide(Vector2 oldVelocity){
			//Create some dust
			Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);
			//Play the "tile hit" sound
			Main.PlaySound(SoundID.Item10, projectile.position);

			//Kill the projectile
			projectile.Kill();

			return false;
		}

		public override void Kill(int timeLeft){
			//Only spawn one projectile - multiplayer compatability
			if(projectile.owner == Main.myPlayer){
				//Drop the item (100% chance)
				int item = Item.NewItem(projectile.getRect(), ModContent.ItemType<BoulderChunk>());

				//Sync the drop for multiplayer
				if (Main.netMode == 1 && item >= 0)
					NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f);
			}
		}

		private void UpdateRotation(){
			int spinDirection = (projectile.velocity.X == 0) ? 0 : ((projectile.velocity.X > 0) ? 1 : -1);
			float spinFactor = Math.Min(
				Math.Max(
					Math.Abs(projectile.velocity.X / MAX_VELOCITY),
					Math.Abs(projectile.velocity.Y / MAX_VELOCITY)
				),
				0.25f
			);

			projectile.rotation += MathHelper.ToRadians(60f) * spinFactor * spinDirection;
		}
	}
}
