using CosmivengeonMod.Projectiles.Weapons.Frostbite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Summons{
	public class EyeOfTheBlizzardCrystal : ModProjectile{
		public override string Texture => "CosmivengeonMod/Items/Frostbite/EyeOfTheBlizzard";

		public static readonly int Damage = 35;
		public static readonly float Knockback = 4f;

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Ice Crystal");
			Main.projFrames[projectile.type] = 6;
		}

		public override void SetDefaults(){
			projectile.width = 30;
			projectile.height = 46;
			projectile.timeLeft = 3600 * 60;
			projectile.sentry = true;
		}

		private bool spawned = false;
		private Player Parent;
		public int timer = 0;
		private float floatAngle = 0;

		private float baseScale;
		private int baseWidth, baseHeight;

		public override void AI(){
			if(!spawned){
				spawned = true;
				Parent = Main.player[projectile.owner];
				baseScale = projectile.scale;
				baseWidth = projectile.width;
				baseHeight = projectile.height;
			}

			if(Parent.dead || !Parent.active || !Parent.GetModPlayer<CosmivengeonPlayer>().equipped_EyeOfTheBlizzard){
				projectile.active = false;
				return;
			}

			float minScale = 0.675f;
			float maxScale = 1.2f;

			CosmivengeonPlayer mp = Parent.GetModPlayer<CosmivengeonPlayer>();

			int cooldownDebuff = Parent.FindBuffIndex(ModContent.BuffType<Buffs.EyeOfTheBlizzard_Cooldown>());

			if(mp.abilityActive_EyeOfTheBlizzard)
				projectile.scale = baseScale * (minScale + (maxScale - minScale) * projectile.ai[0] / (5 * 60));
			else if(cooldownDebuff > -1)
				projectile.scale = baseScale * (minScale + (1f - minScale) * (1f - Parent.buffTime[cooldownDebuff] / (60f * 60f)));
			else
				projectile.scale = baseScale;

			projectile.width = (int)(baseWidth * projectile.scale);
			projectile.height = (int)(baseHeight * projectile.scale);

			Vector2 floatOffset = new Vector2(0, CosmivengeonUtils.fSin(floatAngle) * 8f);
			projectile.Center = Parent.Top - new Vector2(0, 48) + floatOffset;

			if(--timer < 0){
				List<NPC> closestNPCs = Main.npc.Where(n => n?.CanBeChasedBy() == true && Collision.CanHit(projectile.Center, 0, 0, n.Center, 0, 0) && projectile.Distance(n.Center) < 50 * 16)
					.OrderBy(n => projectile.Distance(n.Center))
					.ToList();
				if(closestNPCs.Any()){
					int randTime = Main.rand.Next(45, 105);
					timer = mp.abilityActive_EyeOfTheBlizzard ? (int)(randTime * 0.223f) : randTime;

					NPC closest = closestNPCs.First();

					Main.PlaySound(SoundID.Item28.WithVolume(0.35f), projectile.Center);

					Projectile.NewProjectile(
						projectile.Center,
						projectile.DirectionTo(closest.Center) * CrystaliceShardProjectile.MAX_VELOCITY,
						ModContent.ProjectileType<CrystaliceShardProjectile>(),
						projectile.damage,
						projectile.knockBack,
						projectile.owner,
						1f,
						1f
					);
				}else
					timer++;
			}

			if(projectile.ai[0] > 0f && Main.rand.NextFloat() < 0.375f){
				Dust dust = Dust.NewDustDirect(
					projectile.Center - new Vector2(8, 8),
					16,
					16,
					Main.rand.Next(new int[]{ 74, 107 }),
					Main.rand.NextFloat(-1.5f, 1.5f),
					Main.rand.NextFloat(-1.5f, 1.5f),
					newColor: Color.Blue
				);
				dust.noGravity = true;
			}

			Lighting.AddLight(projectile.Center, Color.Cyan.ToVector3() * 2.5f);

			if(++projectile.frameCounter > 8)
				projectile.frame = ++projectile.frame % Main.projFrames[projectile.type];

			floatAngle += MathHelper.ToRadians(1.5f * 6f);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor){
			Texture2D texture = Main.projectileTexture[projectile.type];
			Rectangle frame = texture.Frame(1, 6, 0, projectile.frame);

			spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, frame, Color.White, 0f, frame.Size() / 2f, projectile.scale, SpriteEffects.None, 0);

			return false;
		}
	}
}
