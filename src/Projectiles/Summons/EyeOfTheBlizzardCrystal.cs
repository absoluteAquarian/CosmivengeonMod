using CosmivengeonMod.Buffs.Mechanics;
using CosmivengeonMod.Items.Equippable.Accessories.Frostbite;
using CosmivengeonMod.Players;
using CosmivengeonMod.Projectiles.Weapons.Frostbite;
using CosmivengeonMod.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Summons {
	public class EyeOfTheBlizzardCrystal : ModProjectile {
		public override string Texture => "CosmivengeonMod/Items/Equippable/Accessories/Frostbite/EyeOfTheBlizzard";

		public static readonly int Damage = 25;
		public static readonly float Knockback = 2.5f;

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Ice Crystal");
			Main.projFrames[Projectile.type] = 6;
		}

		public override void SetDefaults() {
			Projectile.width = 30;
			Projectile.height = 46;
			Projectile.timeLeft = 3600 * 60;
			Projectile.sentry = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
		}

		private bool spawned = false;
		private Player Parent;
		public int timer = 0;
		private float floatAngle = 0;

		private float baseScale;
		private int baseWidth, baseHeight;

		public override void AI() {
			if (!spawned) {
				spawned = true;
				Parent = Main.player[Projectile.owner];
				baseScale = Projectile.scale;
				baseWidth = Projectile.width;
				baseHeight = Projectile.height;
			}

			if (Parent.dead || !Parent.active || !Parent.GetModPlayer<AccessoriesPlayer>().blizzardEye) {
				Projectile.Kill();
				return;
			}

			bool equippedAccessory = false;
			for (int i = 3; i < 8 + Parent.extraAccessorySlots; i++) {
				if (Parent.armor[i].type == ModContent.ItemType<EyeOfTheBlizzard>())
					equippedAccessory = true;
			}
			if (!equippedAccessory) {
				Projectile.Kill();
				return;
			}

			float minScale = Main.hardMode ? 0.675f : 0.835f;
			float maxScale = Main.hardMode ? 1.2f : 1.08f;

			float preWoFShrink = Main.hardMode ? 1f : 0.7f;

			AccessoriesPlayer mp = Parent.GetModPlayer<AccessoriesPlayer>();

			int cooldownDebuff = Parent.FindBuffIndex(ModContent.BuffType<EyeOfTheBlizzardCooldown>());

			if (mp.activeBlizzardEye)
				Projectile.scale = baseScale * preWoFShrink * (minScale + (maxScale - minScale) * Projectile.ai[0] / (5 * 60));
			else if (cooldownDebuff > -1)
				Projectile.scale = baseScale * preWoFShrink * (minScale + (1f - minScale) * (1f - Parent.buffTime[cooldownDebuff] / (60f * 60f)));
			else
				Projectile.scale = baseScale * preWoFShrink;

			Projectile.width = (int)(baseWidth * Projectile.scale);
			Projectile.height = (int)(baseHeight * Projectile.scale);

			Vector2 floatOffset = new Vector2(0, MiscUtils.fSin(floatAngle) * (Main.hardMode ? 8f : 4f));
			float offset = Main.hardMode ? 48 : 28;
			Projectile.Center = Parent.gravDir > 0
				? Parent.Top - new Vector2(0, offset) + floatOffset
				: Parent.Bottom + new Vector2(0, offset) - floatOffset;

			//Don't shoot projectiles if the player has the cooldown debuff
			if (--timer < 0 && cooldownDebuff < 0) {
				const float detectionRange = 50 * 16;
				const float detSQ = detectionRange * detectionRange;

				List<NPC> closestNPCs = Main.npc.Where(n => n?.CanBeChasedBy() == true && Collision.CanHit(Projectile.Center, 0, 0, n.Center, 0, 0) && Projectile.DistanceSQ(n.Center) < detSQ)
					.OrderBy(n => Projectile.DistanceSQ(n.Center))
					.ToList();
				if (closestNPCs.Any()) {
					int randTime = !Main.hardMode ? Main.rand.Next(120, 241) : Main.rand.Next(75, 167);

					if (!Main.hardMode)
						timer = mp.activeBlizzardEye ? (int)(randTime * 0.489f) : randTime;
					else
						timer = mp.activeBlizzardEye ? (int)(randTime * 0.351f) : randTime;

					NPC closest = closestNPCs.First();

					SoundEngine.PlaySound(SoundID.Item28 with { Volume = 0.35f }, Projectile.Center);

					Projectile.NewProjectile(
						Projectile.GetSource_FromAI(),
						Projectile.Center,
						Projectile.DirectionTo(closest.Center) * CrystaliceShardProjectile.MAX_VELOCITY,
						ModContent.ProjectileType<CrystaliceShardProjectile>(),
						Main.hardMode ? Damage : Damage / 2,
						Main.hardMode ? Knockback : Knockback - 1,
						Projectile.owner,
						1f,
						1f);
				} else
					timer++;
			}

			if (Projectile.ai[0] > 0f && Main.rand.NextFloat() < 0.375f) {
				Dust dust = Dust.NewDustDirect(
					Projectile.Center - new Vector2(8, 8),
					16,
					16,
					Main.rand.Next(new int[] { 74, 107 }),
					Main.rand.NextFloat(-1.5f, 1.5f),
					Main.rand.NextFloat(-1.5f, 1.5f),
					newColor: Color.Blue);

				dust.noGravity = true;
			}

			Lighting.AddLight(Projectile.Center, Color.Cyan.ToVector3() * 2.5f);

			if (++Projectile.frameCounter % 9 == 0)
				Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];

			floatAngle += MathHelper.ToRadians(1.5f * 6f);
		}

		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Rectangle frame = texture.Frame(1, 6, 0, Projectile.frame);

			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Color.White, 0f, frame.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);

			return false;
		}
	}
}
