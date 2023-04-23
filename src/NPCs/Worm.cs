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

namespace CosmivengeonMod.NPCs {
	/// <summary>
	/// The base class for all non-separating Worm enemies.
	/// </summary>
	public abstract class Worm : ModNPC {
		public bool head = false;
		public bool tail = false;
		public int minLength;
		public int maxLength;
		public int headType;
		public int bodyType;
		public int tailType;
		/// <summary>
		/// True if this worm should ignore tile collisions.
		/// </summary>
		public bool fly = false;
		/// <summary>
		/// How far away the worm can be (in pixels) before it starts ignoring tile collision.  Used in a rectangle whose dimensions are this measurement * 2 in length and width.
		/// </summary>
		public int maxDigDistance = 1000;
		public float speed;
		public float turnSpeed;
		/// <summary>
		/// If true, then SetCustomBodySegments() is run instead of the normal segment-spawing code.
		/// </summary>
		public bool customBodySegments = false;
		/// <summary>
		/// The custom target for this worm.  If set to Vector2.Zero, then default targeting will be used.
		/// </summary>
		public Vector2 CustomTarget = Vector2.Zero;

		private bool startDespawn = false;

		internal List<Worm> Segments = new List<Worm>();

		public sealed override bool PreAI() {
			if (head) {
				PreAI_Head();

				if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead) {
					NPC.TargetClosest(true);

					if (!Main.player[NPC.target].active || Main.player[NPC.target].dead && NPC.boss) {  //Couldn't get a player target, fall down fast instead
						NPC.velocity.Y += 8f;
						speed = 500f;

						if (!startDespawn) {
							startDespawn = true;
							NPC.timeLeft = (int)(0.5f * 60);
						}
					}
				}
			} else
				PreAI_BodyTail();

			return true;
		}

		internal void PreAI_Head() {
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				// So, we start the AI off by checking if npc.ai[0] is 0.
				// This is practically ALWAYS the case with a freshly spawned NPC, so this means this is the first update.
				// Since this is the first update, we can safely assume we need to spawn the rest of the worm (bodies + tail).
				if (NPC.ai[0] == 0) {
					// So, here we assigning the npc.realLife value.
					// The npc.realLife value is mainly used to determine which NPC loses life when we hit this NPC.
					// We don't want every single piece of the worm to have its own HP pool, so this is a neat way to fix that.
					NPC.realLife = NPC.whoAmI;
					// LatestNPC is going to be used later on and I'll explain it there.
					int latestNPC = NPC.whoAmI;

					// Here we determine the length of the worm.
					int randomWormLength = Main.rand.Next(minLength, maxLength + 1);

					int distance = randomWormLength - 2;

					if (customBodySegments)
						latestNPC = SetCustomBodySegments(distance);
					else {
						while (distance > 0) {
							latestNPC = NewBodySegment(bodyType, latestNPC);
							distance--;
						}
					}
					// When we're out of that loop, we want to 'close' the worm with a tail part!
					latestNPC = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, tailType, NPC.whoAmI, 0, latestNPC);
					Main.npc[latestNPC].realLife = NPC.whoAmI;
					Main.npc[latestNPC].ai[3] = NPC.whoAmI;

					//Set the Segments list
					for (int i = 0; i < Main.npc.Length; i++) {
						NPC n = Main.npc[i];
						if (n?.active == true && n.realLife == NPC.whoAmI && n.ModNPC is Worm worm)
							Segments.Add(worm);
					}

					// We're setting npc.ai[0] to 1, so that this 'if' is not triggered again.
					NPC.ai[0] = 1;
					NPC.netUpdate = true;
				}
			}

			int minTilePosX = (int)(NPC.position.X / 16.0) - 1;
			int maxTilePosX = (int)((NPC.position.X + NPC.width) / 16.0) + 2;
			int minTilePosY = (int)(NPC.position.Y / 16.0) - 1;
			int maxTilePosY = (int)((NPC.position.Y + NPC.height) / 16.0) + 2;
			if (minTilePosX < 0)
				minTilePosX = 0;
			if (maxTilePosX > Main.maxTilesX)
				maxTilePosX = Main.maxTilesX;
			if (minTilePosY < 0)
				minTilePosY = 0;
			if (maxTilePosY > Main.maxTilesY)
				maxTilePosY = Main.maxTilesY;

			bool collision = false;

			// This is the initial check for collision with tiles.
			for (int i = minTilePosX; i < maxTilePosX; ++i) {
				for (int j = minTilePosY; j < maxTilePosY; ++j) {
					if (MiscUtils.TileIsSolidOrPlatform(i, j)) {
						Vector2 vector2;
						vector2.X = (float)(i * 16);
						vector2.Y = (float)(j * 16);
						if (NPC.position.X + NPC.width > vector2.X && NPC.position.X < vector2.X + 16.0 && (NPC.position.Y + NPC.height > (double)vector2.Y && NPC.position.Y < vector2.Y + 16.0)) {
							collision = true;
							if (Main.rand.NextBool(100)&& Main.tile[i, j].HasUnactuatedTile)
								WorldGen.KillTile(i, j, true, true, false);
						}
					}
				}
			}

			// If there is no collision with tiles, we check if the distance between this NPC and its target is too large, so that we can still trigger 'collision'.
			if (!collision) {
				Rectangle rectangle1 = new Rectangle((int)NPC.position.X, (int)NPC.position.Y, NPC.width, NPC.height);
				int maxDistance = maxDigDistance;
				bool playerCollision = true;
				for (int index = 0; index < 255; ++index) {
					Rectangle rectangle2 = Rectangle.Empty;
					Player player = Main.player[index];
					if (CustomTarget != Vector2.Zero)
						rectangle2 = new Rectangle((int)CustomTarget.X - maxDistance, (int)CustomTarget.Y - maxDistance, maxDistance * 2, maxDistance * 2);
					else if (player.active)
						rectangle2 = new Rectangle((int)player.position.X - maxDistance, (int)player.position.Y - maxDistance, maxDistance * 2, maxDistance * 2);

					if (rectangle2 != Rectangle.Empty) {
						if (rectangle1.Intersects(rectangle2)) {
							playerCollision = false;
							break;
						}
					}
				}
				if (playerCollision)
					collision = true;
			}

			// speed determines the max speed at which this NPC can move.
			// Higher value = faster speed.
			float speed = this.speed;
			// acceleration is exactly what it sounds like. The speed at which this NPC accelerates.
			float acceleration = turnSpeed;

			Vector2 npcCenter = new Vector2(NPC.position.X + NPC.width * 0.5f, NPC.position.Y + NPC.height * 0.5f);
			float targetXPos, targetYPos;
			if (CustomTarget == Vector2.Zero) {
				targetXPos = Main.player[NPC.target].position.X + (Main.player[NPC.target].width / 2);
				targetYPos = Main.player[NPC.target].position.Y + (Main.player[NPC.target].height / 2);
			} else {
				targetXPos = CustomTarget.X;
				targetYPos = CustomTarget.Y;
			}

			float targetRoundedPosX = (float)((int)(targetXPos / 16.0) * 16);
			float targetRoundedPosY = (float)((int)(targetYPos / 16.0) * 16);
			npcCenter.X = (float)((int)(npcCenter.X / 16.0) * 16);
			npcCenter.Y = (float)((int)(npcCenter.Y / 16.0) * 16);
			float dirX = targetRoundedPosX - npcCenter.X;
			float dirY = targetRoundedPosY - npcCenter.Y;

			float length = (float)Math.Sqrt(dirX * dirX + dirY * dirY);

			// If we do not have any type of collision, we want the NPC to fall down and de-accelerate along the X axis.
			if (!collision && !fly) {
				NPC.TargetClosest(true);
				NPC.velocity.Y = NPC.velocity.Y + 0.11f;
				if (NPC.velocity.Y > speed)
					NPC.velocity.Y = speed;
				if (Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y) < speed * 0.4) {
					if (NPC.velocity.X < 0.0)
						NPC.velocity.X = NPC.velocity.X - acceleration * 1.1f;
					else
						NPC.velocity.X = NPC.velocity.X + acceleration * 1.1f;
				} else if (NPC.velocity.Y == speed) {
					if (NPC.velocity.X < dirX)
						NPC.velocity.X = NPC.velocity.X + acceleration;
					else if (NPC.velocity.X > dirX)
						NPC.velocity.X = NPC.velocity.X - acceleration;
				} else if (NPC.velocity.Y > 4.0) {
					if (NPC.velocity.X < 0.0)
						NPC.velocity.X = NPC.velocity.X + acceleration * 0.9f;
					else
						NPC.velocity.X = NPC.velocity.X - acceleration * 0.9f;
				}
			}
			// Else we want to play some audio (soundDelay) and move towards our target.
			else {
				if (NPC.soundDelay == 0) {
					float num1 = length / 40f;
					if (num1 < 10.0)
						num1 = 10f;
					if (num1 > 20.0)
						num1 = 20f;
					NPC.soundDelay = (int)num1;
					SoundEngine.PlaySound(SoundID.WormDig, NPC.position);
				}
				float absDirX = Math.Abs(dirX);
				float absDirY = Math.Abs(dirY);
				float newSpeed = speed / length;
				dirX *= newSpeed;
				dirY *= newSpeed;
				if (NPC.velocity.X > 0.0 && dirX > 0.0 || NPC.velocity.X < 0.0 && dirX < 0.0 || (NPC.velocity.Y > 0.0 && dirY > 0.0 || NPC.velocity.Y < 0.0 && dirY < 0.0)) {
					if (NPC.velocity.X < dirX)
						NPC.velocity.X = NPC.velocity.X + acceleration;
					else if (NPC.velocity.X > dirX)
						NPC.velocity.X = NPC.velocity.X - acceleration;
					if (NPC.velocity.Y < dirY)
						NPC.velocity.Y = NPC.velocity.Y + acceleration;
					else if (NPC.velocity.Y > dirY)
						NPC.velocity.Y = NPC.velocity.Y - acceleration;
					if (Math.Abs(dirY) < speed * 0.2 && (NPC.velocity.X > 0.0 && dirX < 0.0 || NPC.velocity.X < 0.0 && dirX > 0.0)) {
						if (NPC.velocity.Y > 0.0)
							NPC.velocity.Y = NPC.velocity.Y + acceleration * 2f;
						else
							NPC.velocity.Y = NPC.velocity.Y - acceleration * 2f;
					}
					if (Math.Abs(dirX) < speed * 0.2 && (NPC.velocity.Y > 0.0 && dirY < 0.0 || NPC.velocity.Y < 0.0 && dirY > 0.0)) {
						if (NPC.velocity.X > 0.0)
							NPC.velocity.X = NPC.velocity.X + acceleration * 2f;
						else
							NPC.velocity.X = NPC.velocity.X - acceleration * 2f;
					}
				} else if (absDirX > absDirY) {
					if (NPC.velocity.X < dirX)
						NPC.velocity.X = NPC.velocity.X + acceleration * 1.1f;
					else if (NPC.velocity.X > dirX)
						NPC.velocity.X = NPC.velocity.X - acceleration * 1.1f;
					if (Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y) < speed * 0.5) {
						if (NPC.velocity.Y > 0.0)
							NPC.velocity.Y = NPC.velocity.Y + acceleration;
						else
							NPC.velocity.Y = NPC.velocity.Y - acceleration;
					}
				} else {
					if (NPC.velocity.Y < dirY)
						NPC.velocity.Y = NPC.velocity.Y + acceleration * 1.1f;
					else if (NPC.velocity.Y > dirY)
						NPC.velocity.Y = NPC.velocity.Y - acceleration * 1.1f;
					if (Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y) < speed * 0.5) {
						if (NPC.velocity.X > 0.0)
							NPC.velocity.X = NPC.velocity.X + acceleration;
						else
							NPC.velocity.X = NPC.velocity.X - acceleration;
					}
				}
			}
			// Set the correct rotation for this NPC.
			NPC.rotation = NPC.velocity.ToRotation();

			// Some netupdate stuff (multiplayer compatibility).
			if (collision) {
				if (NPC.localAI[0] != 1)
					NPC.netUpdate = true;
				NPC.localAI[0] = 1f;
			} else {
				if (NPC.localAI[0] != 0.0)
					NPC.netUpdate = true;
				NPC.localAI[0] = 0.0f;
			}
			if ((NPC.velocity.X > 0.0 && NPC.oldVelocity.X < 0.0 || NPC.velocity.X < 0.0 && NPC.oldVelocity.X > 0.0 || (NPC.velocity.Y > 0.0 && NPC.oldVelocity.Y < 0.0 || NPC.velocity.Y < 0.0 && NPC.oldVelocity.Y > 0.0)) && !NPC.justHit)
				NPC.netUpdate = true;
		}

		internal void PreAI_BodyTail() {
			if (NPC.ai[3] > 0)
				NPC.realLife = (int)NPC.ai[3];
			if (NPC.target < 0 || NPC.target == byte.MaxValue || Main.player[NPC.target].dead)
				NPC.TargetClosest(true);
			if (Main.player[NPC.target].dead && NPC.timeLeft > 30000)
				NPC.timeLeft = 10;

			if (Main.netMode != NetmodeID.MultiplayerClient) {
				//Some of these conditions are possble if the body/tail segment was spawned individually
				if (!Main.npc[(int)NPC.ai[1]].active || Main.npc[(int)NPC.ai[1]].friendly || Main.npc[(int)NPC.ai[1]].townNPC || Main.npc[(int)NPC.ai[1]].lifeMax <= 5) {
					NPC.life = 0;
					NPC.HitEffect(0, 10.0);
					NPC.active = false;
				}
			}

			if (NPC.ai[1] < (double)Main.npc.Length) {
				// We're getting the center of this NPC.
				Vector2 npcCenter = new Vector2(NPC.position.X + (float)NPC.width * 0.5f, NPC.position.Y + (float)NPC.height * 0.5f);
				// Then using that center, we calculate the direction towards the 'parent NPC' of this NPC.
				float dirX = Main.npc[(int)NPC.ai[1]].position.X + (float)(Main.npc[(int)NPC.ai[1]].width / 2) - npcCenter.X;
				float dirY = Main.npc[(int)NPC.ai[1]].position.Y + (float)(Main.npc[(int)NPC.ai[1]].height / 2) - npcCenter.Y;
				// We then use Atan2 to get a correct rotation towards that parent NPC.
				NPC.rotation = (float)Math.Atan2(dirY, dirX);
				// We also get the length of the direction vector.
				float length = (float)Math.Sqrt(dirX * dirX + dirY * dirY);
				// We calculate a new, correct distance.
				float dist = (length - (float)NPC.width) / length;
				float posX = dirX * dist;
				float posY = dirY * dist;

				// Reset the velocity of this NPC, because we don't want it to move on its own
				NPC.velocity = Vector2.Zero;
				// And set this NPCs position accordingly to that of this NPCs parent NPC.
				NPC.position.X = NPC.position.X + posX;
				NPC.position.Y = NPC.position.Y + posY;
			}
		}

		/// <summary>
		/// Override this method to spawn custom body segments.  Returns the last NPC index created.
		/// </summary>
		public virtual int SetCustomBodySegments(int startDistance) {
			return NPC.whoAmI;  //Return the head segment index by default
		}

		/// <summary>
		/// Creates a new body segment of the specified <paramref name="type"/> that follows the NPC whose whoAmI is <paramref name="latestNPC"/>.
		/// </summary>
		/// <returns>The ID of the new latestNPC.</returns>
		public int NewBodySegment(int type, int latestNPC) {
			// We spawn a new NPC, setting latestNPC to the newer NPC, whilst also using that same variable
			// to set the parent of this new NPC. The parent of the new NPC (may it be a tail or body part)
			// will determine the movement of this new NPC.
			// Under there, we also set the realLife value of the new NPC, because of what is explained above.
			//		(in AI() method)
			latestNPC = NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y, type, NPC.whoAmI, 0, latestNPC, 1);
			Main.npc[latestNPC].realLife = NPC.whoAmI;
			Main.npc[latestNPC].ai[3] = NPC.whoAmI;
			return latestNPC;
		}

		/// <summary>
		/// Runs at the beginning of Worm.PreDraw()
		/// </summary>
		public virtual void PreDrawSafe(SpriteBatch spriteBatch, Color drawColor) { }

		public sealed override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			PreDrawSafe(spriteBatch, drawColor);

			Texture2D texture = TextureAssets.Npc[NPC.type].Value;

			Vector2 position = NPC.Center - Main.screenPosition;

			float rotation = MiscUtils.ToActualAngle(NPC.rotation);

			SpriteEffects effect = SpriteEffects.FlipVertically;

			if (rotation > MathHelper.PiOver2 && rotation < 3 * MathHelper.PiOver2)
				effect = SpriteEffects.None;

			Main.spriteBatch.Draw(texture, position, null, drawColor, NPC.rotation, texture.Size() / 2f, NPC.scale, effect, 0);
			return false;
		}

		public sealed override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) {
			return head;    //Only let the head segment have a healtbar
		}
	}
}
