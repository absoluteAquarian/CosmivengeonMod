using CosmivengeonMod.API.Managers;
using CosmivengeonMod.DataStructures;
using CosmivengeonMod.Utility;
using CosmivengeonMod.Utility.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Desomode{
	public class BrainPsychicLightning : ModProjectile{
		public override string Texture => "CosmivengeonMod/Projectiles/Desomode/white_pixel";

		public bool fastAttack;
		private bool delayApplied;

		public const int Death_Delay = 20;
		public const int Max_TimeLeft = 120;

		private const int earliestFrame = 10;

		public int AttackDelay;

		private List<Vector2> points;

		public SoundEffectInstance zap;

		public const int FinalHeight = 120 * 16;

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Psychic Lightning");
		}

		public override void SetDefaults(){
			projectile.width = 24;
			projectile.height = 12;
			projectile.hostile = false;
			projectile.timeLeft = Max_TimeLeft;
			projectile.tileCollide = false;

			points = new List<Vector2>();
		}

		public override void AI(){
			if(!delayApplied){
				delayApplied = true;

				if(AttackDelay % 2 == 1)
					AttackDelay--;

				projectile.timeLeft += AttackDelay;
			}

			float factor = fastAttack ? 2 : 1;

			if(projectile.timeLeft <= earliestFrame * factor + Death_Delay && projectile.timeLeft > Death_Delay){
				projectile.hostile = true;

				projectile.height = FinalHeight;
			}else if(projectile.timeLeft <= Death_Delay)
				projectile.hostile = false;

			if(projectile.timeLeft > earliestFrame * factor + Death_Delay){
				//An electric "cloud"
				points.Clear();

				if(Main.rand.NextBool(6))
					for(int i = 0; i < 40; i++)
						points.Add(new Vector2(Main.rand.NextFloat(-16, projectile.width + 16), Main.rand.NextFloat(-16, projectile.height + 16)));
			}else if(projectile.timeLeft == earliestFrame * factor + Death_Delay){
				//Approximately 3 points per tile
				points.Clear();
				float pointsPerTile = 3f;

				int count = (int)(projectile.height / 16f * pointsPerTile + 0.5f);
				float y = 0;
				for(int i = 0; i < count; i++){
					points.Add(new Vector2(Main.rand.NextFloat(projectile.width), y));
					y += 16f / pointsPerTile;
				}
			}

			if(projectile.timeLeft <= earliestFrame * factor + Death_Delay){
				zap?.Stop();
				zap = mod.PlayCustomSound(projectile.Top + new Vector2(0, 8), "PsychicAttackZap");
			}

			if(fastAttack)
				projectile.timeLeft--;
			
			colorSwap = !colorSwap;
		}

		bool colorSwap;

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor){
			if(points.Count == 0)
				return false;

			if(!ProjectileIsInScreen())
				return false;

			for(int i = 0; i < points.Count; i++)
				points[i] += projectile.position;

			Color color;
			if(projectile.timeLeft <= Death_Delay)
				color = Color.Yellow * ((float)projectile.timeLeft / Death_Delay);
			else
				color = colorSwap ? Color.Yellow : Color.DeepPink;

			PrimitivePacket lightning = new PrimitivePacket(PrimitiveType.LineStrip);
			lightning.AddDraw(PrimitiveDrawing.ToPrimitive(points[0], color), PrimitiveDrawing.ToPrimitive(points[1], color));
			for(int i = 2; i < points.Count; i++)
				lightning.AddDraw(PrimitiveDrawing.ToPrimitive(points[i], color));

			PrimitiveDrawing.SubmitPacket(lightning);

			if(projectile.timeLeft > earliestFrame + 30 + Death_Delay){
				Vector2[] line = new Vector2[2]{
					projectile.Top,
					projectile.Top + new Vector2(0, FinalHeight)
				};

				PrimitivePacket linePacket = new PrimitivePacket(PrimitiveType.LineStrip);
				linePacket.AddDraw(PrimitiveDrawing.ToPrimitive(line[0], Color.White), PrimitiveDrawing.ToPrimitive(line[1], Color.White));

				PrimitiveDrawing.SubmitPacket(linePacket);
			}

			return false;
		}

		private bool ProjectileIsInScreen()
			=> projectile.Right.X >= Main.screenPosition.X && projectile.Left.X <= Main.screenPosition.X + Main.screenWidth;
	}
}
