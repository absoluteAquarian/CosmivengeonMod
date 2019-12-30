using Microsoft.Xna.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;

namespace CosmivengeonMod.Projectiles.Weapons.Draek{
	public class SlitherWandProjectile_Head : ModProjectile{
		public override string Texture => "CosmivengeonMod/NPCs/Draek/DraekWyrmSummon_Head";

		//Worm AI fields
		private float speed;
		private float gravity;

		private bool hasSpawned = false;
		private bool ReachedCursor = false;
		private bool finalTileXSmaller = false;
		private bool fall = false;
		private bool playSound = true;
		private Vector2 target;

		private float currentX;
		private float BurrowSpeed => (16f * 16f) / (1f * 60f);	//(distance * pixelsPerTile) / (secondCount * framesPerSecond)

		private int BurrowTimer = 0;

		private List<Point> TilesToTarget = null;

		private int[] SegmentIDs{ get; } = new int[4]{0, 0, 0, 0};

		public override void SetDefaults(){
			projectile.tileCollide = false;
			//These are false until the snake pops up from the ground
			projectile.friendly = false;
			projectile.hostile = false;
			projectile.alpha = 255;

			projectile.width = 28;
			projectile.height = 32;

			projectile.penetrate = -1;

			speed = 11f;
			gravity = 0.53f;
		}

		private Point previousPoint = new Point();

		private Vector2 DespawnPoint = Vector2.Zero;
		private int DespawnDelay = 40;
		private bool AllowDespawnDelayDecay = false;

		public override void AI(){
			bool prevReachedCursor = ReachedCursor;
			ReachedCursor = Math.Abs(projectile.Center.X - target.X) < 8;

			//If the value has changed, then set the current (X, Y) tile to the "despawn point" location
			//Also set some other values
			if(!AllowDespawnDelayDecay && prevReachedCursor != ReachedCursor){
				DespawnPoint = projectile.position;
				AllowDespawnDelayDecay = true;

				projectile.alpha = 0;
				projectile.friendly = true;
			}

			if(!hasSpawned){
				//Spawn the other segments
				hasSpawned = true;

				currentX = projectile.Center.X;
				target = Main.MouseWorld;

				finalTileXSmaller = currentX > target.X;
				projectile.Center = new Vector2(projectile.Center.X - 16f * (finalTileXSmaller ? -5 : 4), projectile.Center.Y);

				currentX = projectile.Center.X;

				projectile.ai = new float[]{0f, 0f};

				//Only spawn the segments if we're in a multiplayer server or singleplayer game
				if(Main.netMode != NetmodeID.MultiplayerClient){
					SegmentIDs[0] = projectile.whoAmI;
					int proj = Projectile.NewProjectile(
						projectile.Center,
						Vector2.Zero,
						ModContent.ProjectileType<SlitherWandProjectile_Body0>(),
						projectile.damage,
						projectile.knockBack,
						projectile.owner,
						projectile.whoAmI,
						1
					);
					SegmentIDs[1] = proj;
					proj = Projectile.NewProjectile(
						projectile.Center,
						Vector2.Zero,
						ModContent.ProjectileType<SlitherWandProjectile_Body1>(),
						projectile.damage,
						projectile.knockBack,
						projectile.owner,
						proj,
						1
					);
					SegmentIDs[2] = proj;
					proj = Projectile.NewProjectile(
						projectile.Center,
						Vector2.Zero,
						ModContent.ProjectileType<SlitherWandProjectile_Tail>(),
						projectile.damage,
						projectile.knockBack,
						projectile.owner,
						proj,
						1
					);
					SegmentIDs[3] = proj;
				}

				//Finally, get all tiles that are in the way of the target
				TilesToTarget = GetTiles();

				if(TilesToTarget.Count > 0)
					previousPoint = TilesToTarget[0];
				else
					previousPoint = new Point((int)(Main.player[projectile.owner].Center.X / 16f),
						(int)(Main.player[projectile.owner].Bottom.Y / 16f) + 1);
			}

			//Wait until we are under the coordinates given by AI
			if(!ReachedCursor){
				bool nextTileReached = (finalTileXSmaller) ? (currentX / 16f < previousPoint.X) : (currentX / 16f > previousPoint.X);

				currentX += finalTileXSmaller ? -BurrowSpeed : BurrowSpeed;

				Point point = Point.Zero;

				if(!finalTileXSmaller && nextTileReached)
					point = TilesToTarget.Find(p => p.X > (int)(currentX / 16f));
				else if(finalTileXSmaller && nextTileReached){
					point = TilesToTarget.Find(p => p.X < (int)(currentX / 16f));
					point.X--;
				}else
					point = previousPoint;

				if(BurrowTimer % 5 == 0)
					WorldGen.KillTile((int)(projectile.position.X / 16f), (int)(projectile.position.Y / 16f), true, true, false);

				previousPoint = point;

				BurrowTimer++;

				projectile.position = new Vector2(currentX, point.Y * 16);
			}else{
				playSound = false;

				//Move down if we've reached the initial target
				if(Math.Abs(projectile.position.Y - target.Y) < 16){
					fall = true;
				}

				//If we've reached the despawn point, kill the projectile
				if(DespawnDelay == 0 && Main.projectile[SegmentIDs[3]].position.Y > DespawnPoint.Y){
					projectile.active = false;
				}

				//We've reached the coordinates.  Jump up, do other things, then despawn
				AI_Worm_Head();
			}

			projectile.velocity.Y.Clamp(-speed, speed);

			//Set ai[0] to 0/1 if we have not/have reached the target X
			projectile.ai[0] = ReachedCursor ? 1 : 0;

			//Set ai[1] to 0 if rising, 1 if falling
			float oldAI = projectile.ai[1];
			projectile.ai[1] = projectile.velocity.Y > 0 ? 1 : 0;

			if(oldAI != projectile.ai[1] && projectile.ai[1] == 1){
				//Get the ratio of each segment's position relative to the entire worm
				float[] ratios = new float[4];

				float entireLength = 0f;
				float topPos = projectile.position.Y;
				
				//Get the length
				for(int id = 0; id < 4; id++)
					entireLength += Main.projectileTexture[Main.projectile[SegmentIDs[id]].type].Height;

				//Get the ratios
				for(int r = 0; r < 3; r++)
					ratios[r] = Main.projectileTexture[Main.projectile[SegmentIDs[r]].type].Height / entireLength;
				ratios[3] = 1;
				ratios[2] += ratios[1] + ratios[0];
				ratios[1] += ratios[0];

				//Finally, set each segment's position to its inverse ratio (1 - ratios[r])
				for(int i = 0; i < 4; i++){
					Main.projectile[SegmentIDs[i]].position.Y = topPos + (1 - ratios[i]) * entireLength;
				}
			}

			if(AllowDespawnDelayDecay && DespawnDelay > 0)
				DespawnDelay--;

			if(playSound){
				if(projectile.soundDelay == 0){
					float targetRoundedPosX = (int)(target.X / 16.0) * 16;
					float length = targetRoundedPosX - projectile.Center.X;	//Only using x-direction for sound delay

					float delay = length / 40f;
					if(delay < 10.0)
						delay = 10f;
					if(delay > 20.0)
						delay = 20f;
					projectile.soundDelay = (int)delay;
					Main.PlaySound(15, (int)projectile.position.X, (int)projectile.position.Y, 1);
				}
			}
		}

		private void AI_Worm_Head(){
			// acceleration is exactly what it sounds like. The speed at which this projectile accelerates.
			if(fall)
				projectile.velocity.Y += gravity;
			else
				projectile.velocity.Y = -speed;		//Move upwards at maximum velocity

			// Set the correct rotation for this projectile.
			projectile.rotation = projectile.velocity.ToRotation();

			//Changed from NPC Worm AI:  "npc.justHit" piece removed
			if((projectile.velocity.X > 0.0 && projectile.oldVelocity.X < 0.0 || projectile.velocity.X < 0.0 && projectile.oldVelocity.X > 0.0 || (projectile.velocity.Y > 0.0 && projectile.oldVelocity.Y < 0.0 || projectile.velocity.Y < 0.0 && projectile.oldVelocity.Y > 0.0)))
				projectile.netUpdate = true;
		}

		private List<Point> GetTiles(){
			int finalX = (int)(target.X / 16f);

			List<Point> tileCoords = new List<Point>();
			int currentCoordX = (int)(Main.player[projectile.owner].Center.X / 16f);
			int currentCoordY;

			bool finalXSmaller = currentCoordX > finalX;

			finalX += (finalXSmaller ? -5 : 4);
			currentCoordX -= finalXSmaller ? -5 : 4;

			//Get the coordinates
			for(; finalXSmaller ? (currentCoordX > finalX) : (currentCoordX < finalX); currentCoordX += (finalXSmaller) ? -1 : 1){
				int tileCoordsCount = tileCoords.Count;
				currentCoordY = (int)(target.Y / 16f);

				//Loop over all tiles going up to down that are solid
				for(; currentCoordY <= (int)((Main.screenPosition.Y + Main.screenHeight) / 16f); currentCoordY++){
					if(CosmivengeonUtils.TileIsSolidOrPlatform(currentCoordX, currentCoordY) && Main.tile[currentCoordX, currentCoordY].type != TileID.Platforms){
						tileCoords.Add(new Point(currentCoordX, currentCoordY));
						break;
					}
				}

				if(tileCoordsCount == tileCoords.Count){
					currentCoordY = (int)((Main.screenPosition.Y + Main.screenHeight) / 16f);
					tileCoords.Add(new Point(currentCoordX, currentCoordY));
				}
			}

			return tileCoords;
		}
	}

	public class SlitherWandProjectile_Body0 : ModProjectile{
		public override string Texture => "CosmivengeonMod/NPCs/Draek/DraekWyrmSummon_Body0";

		public Projectile Parent;

		public override void SetDefaults(){
			projectile.CloneDefaults(ModContent.ProjectileType<SlitherWandProjectile_Head>());
			projectile.width = 22;
			projectile.height = 38;
			drawOriginOffsetY = projectile.width / 2;
		}

		internal bool hasSpawned = false;
		public override void AI(){
			if(!hasSpawned){
				Parent = Main.projectile[(int)projectile.ai[0]];
				projectile.ai[0] = 0;
				hasSpawned = true;
			}

			projectile.ai[0] = Parent.ai[0];
			projectile.ai[1] = Parent.ai[1];

			//If the head has not reached the target X, then snap this segment's
			// position to the Parent's position
			//Otherwise, execute the worm body AI
			if(Parent.ai[0] == 0){
				projectile.position = Parent.position;
			}else if(Parent.ai[0] == 1){
				//Make the projectile visible
				projectile.alpha = 0;
				projectile.friendly = true;

				AI_Worm_BodyTail();
			}
		}

		internal void AI_Worm_BodyTail(){
			//Changed from NPC Worm AI:  removed first if blocks

			//Changed from NPC Worm AI:  removed "hit effect" and "life" setters as they don't exist
			if(Main.netMode != 1){
				if(!Parent.active){
					projectile.active = false;
				}
			}

			if(Parent.whoAmI < (double)Main.projectile.Length){
				float dirY = Parent.position.Y
					+ ((Parent.ai[1] == 0) ? Main.projectileTexture[Parent.type].Width : -Main.projectileTexture[projectile.type].Width);
				
				projectile.rotation = projectile.position.Y < Parent.position.Y ? MathHelper.PiOver2 : -MathHelper.PiOver2;

				// Reset the velocity of this Projectile, because we don't want it to move on its own
				projectile.velocity = Vector2.Zero;
				// And set this Projectile's position accordingly to that of this Projectle's parent projectile.
				projectile.position.X = Parent.position.X
					+ (Main.projectileTexture[Parent.type].Height - Main.projectileTexture[projectile.type].Height) / 2f;
				projectile.position.Y = dirY;
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor){
			Texture2D texture = Main.projectileTexture[projectile.type];

			Vector2 vector = projectile.Center - Main.screenPosition;

			if(projectile.alpha == 0)
				spriteBatch.Draw(texture,
					vector,
					null,
					lightColor,
					projectile.rotation,
					new Vector2(texture.Width / 2f, texture.Height / 2),
					projectile.scale,
					SpriteEffects.None,
					0
				);

			return false;
		}
	}

	public class SlitherWandProjectile_Body1 : SlitherWandProjectile_Body0{
		public override string Texture => "CosmivengeonMod/NPCs/Draek/DraekWyrmSummon_Body1";

		public override void SetDefaults(){
			projectile.CloneDefaults(ModContent.ProjectileType<SlitherWandProjectile_Body0>());
			projectile.width = 20;
			projectile.height = 26;
			drawOriginOffsetY = projectile.width / 2;
		}
	}

	public class SlitherWandProjectile_Tail : SlitherWandProjectile_Body0{
		public override string Texture => "CosmivengeonMod/NPCs/Draek/DraekWyrmSummon_Tail";

		public override void SetDefaults(){
			projectile.CloneDefaults(ModContent.ProjectileType<SlitherWandProjectile_Body0>());
			projectile.width = 14;
			projectile.height = 20;
			drawOriginOffsetY = projectile.width / 2;
		}
	}
}
