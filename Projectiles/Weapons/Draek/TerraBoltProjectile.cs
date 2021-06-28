using CosmivengeonMod.API.Managers;
using CosmivengeonMod.Buffs.Harmful;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Weapons.Draek{
	public class TerraBoltProjectile : ModProjectile{
		public override void SetStaticDefaults(){
			Main.projFrames[projectile.type] = 3;
		}

		public override void SetDefaults(){
			projectile.width = 16;
			projectile.height = 16;
			projectile.friendly = true;
			projectile.hostile = false;
			projectile.penetrate = 5;
			projectile.timeLeft = 5 * 60;
			projectile.alpha = 0;
			projectile.ignoreWater = true;
			projectile.tileCollide = true;

			projectile.usesLocalNPCImmunity = true;
			projectile.localNPCHitCooldown = -1;
		}

		private bool hasSpawned = false;

		public const int RememberedPrimPoints = 6;
		private readonly Queue<Vector2> primPoints = new Queue<Vector2>(RememberedPrimPoints);
		private readonly Queue<Vector2> primPoints2 = new Queue<Vector2>(RememberedPrimPoints);
		private readonly Queue<Vector2> primPoints3 = new Queue<Vector2>(RememberedPrimPoints);
		private readonly Queue<Vector2> primPoints4 = new Queue<Vector2>(RememberedPrimPoints);
		private readonly Queue<Vector2> primPoints5 = new Queue<Vector2>(RememberedPrimPoints);

		private readonly Queue<Vector2> primAnchors = new Queue<Vector2>(RememberedPrimPoints);

		public ref float ColorFade => ref projectile.ai[0];

		public override void AI(){
			if(!hasSpawned){
				hasSpawned = true;
				projectile.frame = Main.rand.Next(3);
				projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
			}
			
			projectile.rotation += (projectile.velocity.X >= 0 ? 1 : -1) * MathHelper.ToRadians((5f + Main.rand.NextFloat(-2f, 2f)) * 360f / 60f);

			if(Main.rand.NextFloat() < 0.8f){
				Dust dust = Dust.NewDustDirect(projectile.width > 4 ? projectile.position + new Vector2(2) : projectile.Center,
					Math.Max(projectile.width - 4, 0),
					Math.Max(projectile.height - 4, 0),
					75);
				dust.noGravity = true;
				dust.fadeIn = 0.8f;
				dust.velocity = Vector2.Zero;
			}

			if(ColorFade < 1f){
				ColorFade += 1f / 80f;
				if(ColorFade > 1f)
					ColorFade = 1f;
			}
		}

		public override Color? GetAlpha(Color lightColor)
			=> Color.Lerp(Color.Lime, lightColor, ColorFade);

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit){
			int seconds = target.boss ? 5 : 7;
			target.AddBuff(ModContent.BuffType<PrimordialWrath>(), seconds * 60);
		}

		public override void OnHitPlayer(Player target, int damage, bool crit){
			target.AddBuff(ModContent.BuffType<PrimordialWrath>(), 3 * 60);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor){
			//Redraw the projectile with the color not influenced by light
			Texture2D texture = Main.projectileTexture[projectile.type];

			Rectangle drawFrame = new Rectangle(0, texture.Height / Main.projFrames[projectile.type] * projectile.frame, texture.Width, texture.Height / Main.projFrames[projectile.type]);
			Vector2 drawOrigin = new Vector2(drawFrame.Width * 0.5f, drawFrame.Height * 0.5f);
			Vector2 drawPos = projectile.Center - Main.screenPosition;
			Color color = projectile.GetAlpha(lightColor);

			spriteBatch.Draw(texture, drawPos, drawFrame, color, projectile.rotation, drawOrigin, projectile.scale, SpriteEffects.None, 0f);

			//Queue the new points
			QueuePoint(primPoints);
			QueuePoint(primPoints2);
			QueuePoint(primPoints3);
			QueuePoint(primPoints4);
			QueuePoint(primPoints5);

			if(primAnchors.Count == RememberedPrimPoints)
				primAnchors.Dequeue();

			primAnchors.Enqueue(projectile.Center);

			//Draw some cool primitives effects
			DrawPrims(primPoints, primAnchors);
			DrawPrims(primPoints2, primAnchors);
			DrawPrims(primPoints3, primAnchors);
			DrawPrims(primPoints4, primAnchors);
			DrawPrims(primPoints5, primAnchors);

			return false;
		}

		private void QueuePoint(Queue<Vector2> queue){
			//Update the primitive points queue
			if(queue.Count == RememberedPrimPoints)
				queue.Dequeue();

			//Get a random point in a box centered on the projectile
			queue.Enqueue(Vector2.UnitX.RotatedByRandom(MathHelper.Pi) * projectile.width / 2f);
		}

		private void DrawPrims(Queue<Vector2> queue, Queue<Vector2> anchors){
			Vector2[] points = queue.ToArray();
			Vector2[] anchorPoints = anchors.ToArray();

			//Compress the old points
			for(int i = 0; i < points.Length; i++){
				Vector2 offset = points[i];
				offset *= (i + 1) / (float)points.Length;
				points[i] = anchorPoints[i] + offset;
			}

			PrimitiveDrawing.DrawLineStrip(points, Color.Lime);
		}

		public override void Kill(int timeLeft){
			Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);
			Main.PlaySound(SoundID.Item10, projectile.position);
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough){
			width = Math.Min(width, 16);
			height = Math.Min(height, 16);
			return base.TileCollideStyle(ref width, ref height, ref fallThrough);
		}
	}
}
