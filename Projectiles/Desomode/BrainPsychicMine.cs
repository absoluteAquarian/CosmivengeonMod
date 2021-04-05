using CosmivengeonMod.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Desomode{
	public class BrainPsychicMine : ModProjectile{
		public override string Texture => "CosmivengeonMod/Projectiles/Desomode/white_pixel";
		
		public const int Attack_Death_Delay = 20;
		public const int Attack_Timer_Max = 240 + Attack_Death_Delay;
		public const int Attack_Leadup = 180 + Attack_Death_Delay;
		public const int Attack_Shrink_Start = 60 + Attack_Death_Delay;

		public bool fastAttack;

		private Vector2 storedPosition;

		public SoundEffectInstance teleport;

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Psychic Explosion");
		}

		public override void SetDefaults(){
			projectile.width = 48;
			projectile.height = 48;
			projectile.tileCollide = false;
			projectile.timeLeft = Attack_Timer_Max;
		}

		public override void AI(){
			//Playing cool sound effects
			if(projectile.timeLeft > Attack_Death_Delay)
				teleport = mod.PlayCustomSound(projectile.Center, "PsychicAttackLeadup");
			else{
				if(teleport != null){
					teleport?.Stop();
					teleport = null;
				}
				mod.PlayCustomSound(projectile.Center, "PsychicAttackCrash");
			}

			float rotation = MathHelper.ToRadians(1.25f * 6f);
			projectile.rotation += rotation;
			projectile.ai[0] += rotation * 0.8f;

			if(projectile.timeLeft > Attack_Shrink_Start)
				storedPosition = Main.player[(int)projectile.ai[1]].Center;

			projectile.Center = storedPosition;

			if(fastAttack && projectile.timeLeft == 4 + Attack_Death_Delay)
				projectile.hostile = true;
			if(projectile.timeLeft == 2 + Attack_Death_Delay)
				projectile.hostile = true;

			if(fastAttack)
				projectile.timeLeft--;
		}

		public override void OnHitPlayer(Player target, int damage, bool crit){
			projectile.hostile = false;

			target.AddBuff(BuffID.Confused, 120);
			target.AddBuff(BuffID.Slow, 120);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor){
			int capacity = 64 * 2;
			List<Vector2> points = new List<Vector2>();

			//Set the points
			float radiusOuter;
			if(projectile.timeLeft > Attack_Leadup)
				radiusOuter = 6 * 16 - 1.5f * 16 * (1f - (float)(projectile.timeLeft - Attack_Leadup) / (Attack_Timer_Max - Attack_Leadup));
			else if(projectile.timeLeft > Attack_Shrink_Start + 12)
				radiusOuter = 4.5f * 16f;
			else if(projectile.timeLeft > Attack_Shrink_Start)
				radiusOuter = 4.5f * 16f + 0.5f * 16 * (float)Math.Sin((1f - (float)(projectile.timeLeft - Attack_Shrink_Start) / 12) * Math.PI);
			else if(projectile.timeLeft > Attack_Death_Delay)
				radiusOuter = 4.5f * 16f * ((float)(projectile.timeLeft - Attack_Death_Delay) / (Attack_Shrink_Start - Attack_Death_Delay));
			else
				radiusOuter = 10f * 16f * ((float)(projectile.timeLeft - Attack_Death_Delay) / Attack_Death_Delay);

			float radiusInner = radiusOuter * 0.6667f;

			int halfCapacity = capacity / 2;
			for(int i = 0; i < halfCapacity; i++){
				Vector2 vector = Vector2.UnitX.RotatedBy(projectile.rotation + MathHelper.ToRadians(360f / halfCapacity * i)) * (radiusOuter - (i % 2 == 1 ? 8 : 0));
				points.Add(vector + projectile.Center);
			}

			DrawPrims(points, Color.Pink);
			points.Clear();

			for(int i = halfCapacity; i < capacity; i++){
				Vector2 vector = Vector2.UnitX.RotatedBy(projectile.ai[0] + MathHelper.ToRadians(360f / halfCapacity * (i - halfCapacity))) * (radiusInner - (i % 2 == 1 ? 8 : 0));
				points.Add(vector + projectile.Center);
			}

			DrawPrims(points, Color.Purple);

			return false;
		}

		private void DrawPrims(List<Vector2> points, Color color){
			PrimitivePacket packet = new PrimitivePacket(PrimitiveType.LineStrip);

			packet.AddDraw(PrimitiveDrawing.ToPrimitive(points[0], color), PrimitiveDrawing.ToPrimitive(points[1], color));
			for(int i = 2; i < points.Count; i++)
				packet.AddDraw(PrimitiveDrawing.ToPrimitive(points[i], color));

			PrimitiveDrawing.SubmitPacket(packet);
		}
	}
}
