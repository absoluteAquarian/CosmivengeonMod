using CosmivengeonMod.Utility;
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
	public class SlitherWandProjectile_Head : ModProjectile {
		public override string Texture => "CosmivengeonMod/NPCs/Bosses/DraekBoss/Summons/DraekWyrmSummon_Head";

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
		private float BurrowSpeed => (16f * 16f) / (1f * 60f);  //(distance * pixelsPerTile) / (secondCount * framesPerSecond)

		private int BurrowTimer = 0;

		private List<Point> TilesToTarget = null;

		private int[] SegmentIDs { get; } = new int[4] { 0, 0, 0, 0 };

		public override void SetDefaults() {
			Projectile.tileCollide = false;
			//These are false until the snake pops up from the ground
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.alpha = 255;

			Projectile.width = 28;
			Projectile.height = 32;

			Projectile.penetrate = -1;

			speed = 11f;
			gravity = 0.53f;
		}

		private Point previousPoint = new Point();

		private Vector2 DespawnPoint = Vector2.Zero;
		private int DespawnDelay = 40;
		private bool AllowDespawnDelayDecay = false;

		public override void AI() {
			bool prevReachedCursor = ReachedCursor;
			ReachedCursor = Math.Abs(Projectile.Center.X - target.X) < 8;

			//If the value has changed, then set the current (X, Y) tile to the "despawn point" location
			//Also set some other values
			if (!AllowDespawnDelayDecay && prevReachedCursor != ReachedCursor) {
				DespawnPoint = Projectile.position;
				AllowDespawnDelayDecay = true;

				Projectile.alpha = 0;
				Projectile.friendly = true;
			}

			if (!hasSpawned) {
				//Spawn the other segments
				hasSpawned = true;

				currentX = Projectile.Center.X;
				target = Main.MouseWorld;

				finalTileXSmaller = currentX > target.X;
				Projectile.Center = new Vector2(Projectile.Center.X - 16f * (finalTileXSmaller ? -5 : 4), Projectile.Center.Y);

				currentX = Projectile.Center.X;

				Projectile.ai = new float[] { 0f, 0f };

				//Only spawn the segments if we're in a multiplayer server or singleplayer game
				if (Main.netMode != NetmodeID.MultiplayerClient) {
					SegmentIDs[0] = Projectile.whoAmI;
					int proj = Projectile.NewProjectile(
						Projectile.GetSource_FromAI(),
						Projectile.Center,
						Vector2.Zero,
						ModContent.ProjectileType<SlitherWandProjectile_Body0>(),
						Projectile.damage,
						Projectile.knockBack,
						Projectile.owner,
						Projectile.whoAmI,
						1
					);
					SegmentIDs[1] = proj;
					proj = Projectile.NewProjectile(
						Projectile.GetSource_FromAI(),
						Projectile.Center,
						Vector2.Zero,
						ModContent.ProjectileType<SlitherWandProjectile_Body1>(),
						Projectile.damage,
						Projectile.knockBack,
						Projectile.owner,
						proj,
						1
					);
					SegmentIDs[2] = proj;
					proj = Projectile.NewProjectile(
						Projectile.GetSource_FromAI(),
						Projectile.Center,
						Vector2.Zero,
						ModContent.ProjectileType<SlitherWandProjectile_Tail>(),
						Projectile.damage,
						Projectile.knockBack,
						Projectile.owner,
						proj,
						1
					);
					SegmentIDs[3] = proj;
				}

				//Finally, get all tiles that are in the way of the target
				TilesToTarget = GetTiles();

				if (TilesToTarget.Count > 0)
					previousPoint = TilesToTarget[0];
				else
					previousPoint = new Point((int)(Main.player[Projectile.owner].Center.X / 16f),
						(int)(Main.player[Projectile.owner].Bottom.Y / 16f) + 1);
			}

			//Wait until we are under the coordinates given by AI
			if (!ReachedCursor) {
				bool nextTileReached = (finalTileXSmaller) ? (currentX / 16f < previousPoint.X) : (currentX / 16f > previousPoint.X);

				currentX += finalTileXSmaller ? -BurrowSpeed : BurrowSpeed;

				Point point = Point.Zero;

				if (!finalTileXSmaller && nextTileReached)
					point = TilesToTarget.Find(p => p.X > (int)(currentX / 16f));
				else if (finalTileXSmaller && nextTileReached) {
					point = TilesToTarget.Find(p => p.X < (int)(currentX / 16f));
					point.X--;
				} else
					point = previousPoint;

				if (BurrowTimer % 5 == 0)
					WorldGen.KillTile((int)(Projectile.position.X / 16f), (int)(Projectile.position.Y / 16f), true, true, false);

				previousPoint = point;

				BurrowTimer++;

				Projectile.position = new Vector2(currentX, point.Y * 16);
			} else {
				playSound = false;

				//Move down if we've reached the initial target
				if (Math.Abs(Projectile.position.Y - target.Y) < 16) {
					fall = true;
				}

				//If we've reached the despawn point, kill the projectile
				if (DespawnDelay == 0 && Main.projectile[SegmentIDs[3]].position.Y > DespawnPoint.Y) {
					Projectile.active = false;
				}

				//We've reached the coordinates.  Jump up, do other things, then despawn
				AI_Worm_Head();
			}

			Projectile.velocity.Y.Clamp(-speed, speed);

			//Set ai[0] to 0/1 if we have not/have reached the target X
			Projectile.ai[0] = ReachedCursor ? 1 : 0;

			//Set ai[1] to 0 if rising, 1 if falling
			float oldAI = Projectile.ai[1];
			Projectile.ai[1] = Projectile.velocity.Y > 0 ? 1 : 0;

			if (oldAI != Projectile.ai[1] && Projectile.ai[1] == 1) {
				//Get the ratio of each segment's position relative to the entire worm
				float[] ratios = new float[4];

				float entireLength = 0f;
				float topPos = Projectile.position.Y;

				//Get the length
				for (int id = 0; id < 4; id++)
					entireLength += TextureAssets.Projectile[Main.projectile[SegmentIDs[id]].type].Value.Height;

				//Get the ratios
				for (int r = 0; r < 3; r++)
					ratios[r] = TextureAssets.Projectile[Main.projectile[SegmentIDs[r]].type].Value.Height / entireLength;
				ratios[3] = 1;
				ratios[2] += ratios[1] + ratios[0];
				ratios[1] += ratios[0];

				//Finally, set each segment's position to its inverse ratio (1 - ratios[r])
				for (int i = 0; i < 4; i++) {
					Main.projectile[SegmentIDs[i]].position.Y = topPos + (1 - ratios[i]) * entireLength;
				}
			}

			if (AllowDespawnDelayDecay && DespawnDelay > 0)
				DespawnDelay--;

			if (playSound) {
				if (Projectile.soundDelay == 0) {
					float targetRoundedPosX = (int)(target.X / 16.0) * 16;
					float length = targetRoundedPosX - Projectile.Center.X; //Only using x-direction for sound delay

					float delay = length / 40f;
					if (delay < 10.0)
						delay = 10f;
					if (delay > 20.0)
						delay = 20f;
					Projectile.soundDelay = (int)delay;
					SoundEngine.PlaySound(SoundID.WormDig, Projectile.position);
				}
			}
		}

		private void AI_Worm_Head() {
			// acceleration is exactly what it sounds like. The speed at which this projectile accelerates.
			if (fall)
				Projectile.velocity.Y += gravity;
			else
				Projectile.velocity.Y = -speed;     //Move upwards at maximum velocity

			// Set the correct rotation for this projectile.
			Projectile.rotation = Projectile.velocity.ToRotation();

			//Changed from NPC Worm AI:  "npc.justHit" piece removed
			if ((Projectile.velocity.X > 0.0 && Projectile.oldVelocity.X < 0.0 || Projectile.velocity.X < 0.0 && Projectile.oldVelocity.X > 0.0 || (Projectile.velocity.Y > 0.0 && Projectile.oldVelocity.Y < 0.0 || Projectile.velocity.Y < 0.0 && Projectile.oldVelocity.Y > 0.0)))
				Projectile.netUpdate = true;
		}

		private List<Point> GetTiles() {
			int finalX = (int)(target.X / 16f);

			List<Point> tileCoords = new List<Point>();
			int currentCoordX = (int)(Main.player[Projectile.owner].Center.X / 16f);
			int currentCoordY;

			bool finalXSmaller = currentCoordX > finalX;

			finalX += (finalXSmaller ? -5 : 4);
			currentCoordX -= finalXSmaller ? -5 : 4;

			//Get the coordinates
			for (; finalXSmaller ? (currentCoordX > finalX) : (currentCoordX < finalX); currentCoordX += (finalXSmaller) ? -1 : 1) {
				int tileCoordsCount = tileCoords.Count;
				currentCoordY = (int)(target.Y / 16f);

				//Loop over all tiles going up to down that are solid
				for (; currentCoordY <= (int)((Main.screenPosition.Y + Main.screenHeight) / 16f); currentCoordY++) {
					if (MiscUtils.TileIsSolidOrPlatform(currentCoordX, currentCoordY) && Main.tile[currentCoordX, currentCoordY].TileType != TileID.Platforms) {
						tileCoords.Add(new Point(currentCoordX, currentCoordY));
						break;
					}
				}

				if (tileCoordsCount == tileCoords.Count) {
					currentCoordY = (int)((Main.screenPosition.Y + Main.screenHeight) / 16f);
					tileCoords.Add(new Point(currentCoordX, currentCoordY));
				}
			}

			return tileCoords;
		}
	}

	public class SlitherWandProjectile_Body0 : ModProjectile {
		public override string Texture => "CosmivengeonMod/NPCs/Bosses/DraekBoss/Summons/DraekWyrmSummon_Body0";

		public Projectile Parent;

		public override void SetDefaults() {
			Projectile.CloneDefaults(ModContent.ProjectileType<SlitherWandProjectile_Head>());
			Projectile.width = 22;
			Projectile.height = 38;
			DrawOriginOffsetY = Projectile.width / 2;
		}

		internal bool hasSpawned = false;
		public override void AI() {
			if (!hasSpawned) {
				Parent = Main.projectile[(int)Projectile.ai[0]];
				Projectile.ai[0] = 0;
				hasSpawned = true;
			}

			Projectile.ai[0] = Parent.ai[0];
			Projectile.ai[1] = Parent.ai[1];

			//If the head has not reached the target X, then snap this segment's
			// position to the Parent's position
			//Otherwise, execute the worm body AI
			if (Parent.ai[0] == 0) {
				Projectile.position = Parent.position;
			} else if (Parent.ai[0] == 1) {
				//Make the projectile visible
				Projectile.alpha = 0;
				Projectile.friendly = true;

				AI_Worm_BodyTail();
			}
		}

		internal void AI_Worm_BodyTail() {
			//Changed from NPC Worm AI:  removed first if blocks

			//Changed from NPC Worm AI:  removed "hit effect" and "life" setters as they don't exist
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				if (!Parent.active) {
					Projectile.active = false;
				}
			}

			if (Parent.whoAmI < (double)Main.projectile.Length) {
				float dirY = Parent.position.Y
					+ ((Parent.ai[1] == 0) ? TextureAssets.Projectile[Parent.type].Value.Width : -TextureAssets.Projectile[Projectile.type].Value.Width);

				Projectile.rotation = Projectile.position.Y < Parent.position.Y ? MathHelper.PiOver2 : -MathHelper.PiOver2;

				// Reset the velocity of this Projectile, because we don't want it to move on its own
				Projectile.velocity = Vector2.Zero;
				// And set this Projectile's position accordingly to that of this Projectle's parent projectile.
				Projectile.position.X = Parent.position.X
					+ (TextureAssets.Projectile[Parent.type].Value.Height - TextureAssets.Projectile[Projectile.type].Value.Height) / 2f;
				Projectile.position.Y = dirY;
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

			Vector2 vector = Projectile.Center - Main.screenPosition;

			if (Projectile.alpha == 0)
				Main.EntitySpriteDraw(texture,
					vector,
					null,
					lightColor,
					Projectile.rotation,
					new Vector2(texture.Width / 2f, texture.Height / 2),
					Projectile.scale,
					SpriteEffects.None,
					0
				);

			return false;
		}
	}

	public class SlitherWandProjectile_Body1 : SlitherWandProjectile_Body0 {
		public override string Texture => "CosmivengeonMod/NPCs/Bosses/DraekBoss/Summons/DraekWyrmSummon_Body1";

		public override void SetDefaults() {
			Projectile.CloneDefaults(ModContent.ProjectileType<SlitherWandProjectile_Body0>());
			Projectile.width = 20;
			Projectile.height = 26;
			DrawOriginOffsetY = Projectile.width / 2;
		}
	}

	public class SlitherWandProjectile_Tail : SlitherWandProjectile_Body0 {
		public override string Texture => "CosmivengeonMod/NPCs/Bosses/DraekBoss/Summons/DraekWyrmSummon_Tail";

		public override void SetDefaults() {
			Projectile.CloneDefaults(ModContent.ProjectileType<SlitherWandProjectile_Body0>());
			Projectile.width = 14;
			Projectile.height = 20;
			DrawOriginOffsetY = Projectile.width / 2;
		}
	}
}
