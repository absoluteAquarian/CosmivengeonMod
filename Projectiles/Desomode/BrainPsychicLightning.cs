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
			DisplayName.SetDefault(" ");
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

			List<Vector3> drawPos = PreparePoints(points);

			Color color;
			if(projectile.timeLeft <= Death_Delay)
				color = Color.Yellow * ((float)projectile.timeLeft / Death_Delay);
			else
				color = colorSwap ? Color.Yellow : Color.DeepPink;

			List<VertexPositionColor> draws = new List<VertexPositionColor>(drawPos.Count);
			for(int i = 0; i < drawPos.Count; i++)
				draws.Add(new VertexPositionColor(drawPos[i], color));

			CosmivengeonUtils.PrepareToDrawPrimitives(draws.Count, out VertexBuffer buffer);

			CosmivengeonUtils.DrawPrimitives(draws.ToArray(), buffer);

			if(projectile.timeLeft > earliestFrame + 30 + Death_Delay){
				Vector3[] line = new Vector3[2]{
					(projectile.Top - Main.screenPosition).ScreenCoord(),
					(projectile.Top + new Vector2(0, FinalHeight) - Main.screenPosition).ScreenCoord()
				};

				VertexPositionColor[] drawLine = new VertexPositionColor[2]{
					new VertexPositionColor(line[0], Color.White),
					new VertexPositionColor(line[1], Color.White)
				};

				CosmivengeonUtils.PrepareToDrawPrimitives(2, out VertexBuffer buffer2);

				CosmivengeonUtils.DrawPrimitives(drawLine, buffer2);
			}

			return false;
		}

		private List<Vector3> PreparePoints(List<Vector2> original){
			List<Vector2> prepared = new List<Vector2>(original);
			
			for(int i = 1; i < prepared.Count - 1; i += 2)
				prepared.Insert(i, prepared[i]);

			List<Vector3> ret = new List<Vector3>();

			for(int i = 0; i < prepared.Count; i++)
				ret.Add((prepared[i] + projectile.position - Main.screenPosition).ScreenCoord());

			return ret;
		}

		private bool ProjectileIsInScreen()
			=> projectile.Right.X >= Main.screenPosition.X && projectile.Left.X <= Main.screenPosition.X + Main.screenWidth;
	}
}
