using CosmivengeonMod.API.Managers;
using CosmivengeonMod.Buffs.Harmful;
using GraphicsLib.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Weapons.Draek {
	public class TerraBoltProjectile : ModProjectile {
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 3;
		}

		public override void SetDefaults() {
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.penetrate = 5;
			Projectile.timeLeft = 5 * 60;
			Projectile.alpha = 0;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}

		private bool hasSpawned = false;

		public const int RememberedPrimPoints = 6;
		private readonly Queue<Vector2> primPoints = new Queue<Vector2>(RememberedPrimPoints);
		private readonly Queue<Vector2> primPoints2 = new Queue<Vector2>(RememberedPrimPoints);
		private readonly Queue<Vector2> primPoints3 = new Queue<Vector2>(RememberedPrimPoints);
		private readonly Queue<Vector2> primPoints4 = new Queue<Vector2>(RememberedPrimPoints);
		private readonly Queue<Vector2> primPoints5 = new Queue<Vector2>(RememberedPrimPoints);

		private readonly Queue<Vector2> primAnchors = new Queue<Vector2>(RememberedPrimPoints);

		public ref float ColorFade => ref Projectile.ai[0];

		public override void AI() {
			if (!hasSpawned) {
				hasSpawned = true;
				Projectile.frame = Main.rand.Next(3);
				Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
			}

			Projectile.rotation += (Projectile.velocity.X >= 0 ? 1 : -1) * MathHelper.ToRadians((5f + Main.rand.NextFloat(-2f, 2f)) * 360f / 60f);

			if (Main.rand.NextFloat() < 0.8f) {
				Dust dust = Dust.NewDustDirect(Projectile.width > 4 ? Projectile.position + new Vector2(2) : Projectile.Center,
					Math.Max(Projectile.width - 4, 0),
					Math.Max(Projectile.height - 4, 0),
					DustID.CursedTorch);
				dust.noGravity = true;
				dust.fadeIn = 0.8f;
				dust.velocity = Vector2.Zero;
			}

			if (ColorFade < 1f) {
				ColorFade += 1f / 80f;
				if (ColorFade > 1f)
					ColorFade = 1f;
			}
		}

		public override Color? GetAlpha(Color lightColor)
			=> Color.Lerp(Color.Lime, lightColor, ColorFade);

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			int seconds = target.boss ? 5 : 7;
			target.AddBuff(ModContent.BuffType<PrimordialWrath>(), seconds * 60);
		}

		public override void OnHitPlayer(Player target, int damage, bool crit) {
			target.AddBuff(ModContent.BuffType<PrimordialWrath>(), 3 * 60);
		}

		public override bool PreDraw(ref Color lightColor) {
			//Redraw the projectile with the color not influenced by light
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

			Rectangle drawFrame = new Rectangle(0, texture.Height / Main.projFrames[Projectile.type] * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);
			Vector2 drawOrigin = new Vector2(drawFrame.Width * 0.5f, drawFrame.Height * 0.5f);
			Vector2 drawPos = Projectile.Center - Main.screenPosition;
			Color color = Projectile.GetAlpha(lightColor);

			Main.EntitySpriteDraw(texture, drawPos, drawFrame, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

			//Queue the new points
			QueuePoint(primPoints);
			QueuePoint(primPoints2);
			QueuePoint(primPoints3);
			QueuePoint(primPoints4);
			QueuePoint(primPoints5);

			if (primAnchors.Count == RememberedPrimPoints)
				primAnchors.Dequeue();

			primAnchors.Enqueue(Projectile.Center);

			//Draw some cool primitives effects
			DrawPrims(primPoints, primAnchors);
			DrawPrims(primPoints2, primAnchors);
			DrawPrims(primPoints3, primAnchors);
			DrawPrims(primPoints4, primAnchors);
			DrawPrims(primPoints5, primAnchors);

			return false;
		}

		private void QueuePoint(Queue<Vector2> queue) {
			//Update the primitive points queue
			if (queue.Count == RememberedPrimPoints)
				queue.Dequeue();

			//Get a random point in a box centered on the projectile
			queue.Enqueue(Vector2.UnitX.RotatedByRandom(MathHelper.Pi) * Projectile.width / 2f);
		}

		private void DrawPrims(Queue<Vector2> queue, Queue<Vector2> anchors) {
			Vector2[] points = queue.ToArray();
			Vector2[] anchorPoints = anchors.ToArray();

			//Compress the old points
			for (int i = 0; i < points.Length; i++) {
				Vector2 offset = points[i];
				offset *= (i + 1) / (float)points.Length;
				points[i] = anchorPoints[i] + offset;
			}

			PrimitiveDrawing.DrawLineStrip(points, Color.Lime);
		}

		public override void Kill(int timeLeft) {
			Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width = Math.Min(width, 16);
			height = Math.Min(height, 16);
			return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
		}
	}
}
