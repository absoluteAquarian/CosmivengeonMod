using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.NPCs{
	/// <summary>
	/// The base class for all non-separating Worm enemies.
	/// </summary>
	public abstract class Worm : ModNPC{
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
		public int maxDigDistance;
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

		public override bool PreAI(){
			if(head)
				PreAI_Head();
			else
				PreAI_BodyTail();
			
			return true;
		}

		internal void PreAI_Head(){			
			if(Main.netMode != 1){
				// So, we start the AI off by checking if npc.ai[0] is 0.
				// This is practically ALWAYS the case with a freshly spawned NPC, so this means this is the first update.
				// Since this is the first update, we can safely assume we need to spawn the rest of the worm (bodies + tail).
				if(npc.ai[0] == 0){
					// So, here we assigning the npc.realLife value.
					// The npc.realLife value is mainly used to determine which NPC loses life when we hit this NPC.
					// We don't want every single piece of the worm to have its own HP pool, so this is a neat way to fix that.
					npc.realLife = npc.whoAmI;
					// LatestNPC is going to be used later on and I'll explain it there.
					int latestNPC = npc.whoAmI;

					// Here we determine the length of the worm.
					int randomWormLength = Main.rand.Next(minLength, maxLength + 1);

					int distance = randomWormLength - 1;

					distance--;

					if(customBodySegments)
						latestNPC = SetCustomBodySegments(distance);
					else{
						while(distance > 0){
							latestNPC = NewBodySegment(bodyType, latestNPC);
						}
					}
					// When we're out of that loop, we want to 'close' the worm with a tail part!
					latestNPC = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, tailType, npc.whoAmI, 0, latestNPC);
					Main.npc[latestNPC].realLife = npc.whoAmI;
					Main.npc[latestNPC].ai[3] = npc.whoAmI;
 
					// We're setting npc.ai[0] to 1, so that this 'if' is not triggered again.
					npc.ai[0] = 1;
					npc.netUpdate = true;
				}
			}
 
			int minTilePosX = (int)(npc.position.X / 16.0) - 1;
			int maxTilePosX = (int)((npc.position.X + npc.width) / 16.0) + 2;
			int minTilePosY = (int)(npc.position.Y / 16.0) - 1;
			int maxTilePosY = (int)((npc.position.Y + npc.height) / 16.0) + 2;
			if(minTilePosX < 0)
				minTilePosX = 0;
			if(maxTilePosX > Main.maxTilesX)
				maxTilePosX = Main.maxTilesX;
			if(minTilePosY < 0)
				minTilePosY = 0;
			if(maxTilePosY > Main.maxTilesY)
				maxTilePosY = Main.maxTilesY;
 
			bool collision = false;

			// This is the initial check for collision with tiles.
			for(int i = minTilePosX; i < maxTilePosX; ++i){
				for(int j = minTilePosY; j < maxTilePosY; ++j){
					if(CosmivengeonMod.TileIsSolidOrPlatform(i, j)){
						Vector2 vector2;
						vector2.X = (float)(i * 16);
						vector2.Y = (float)(j * 16);
						if(npc.position.X + npc.width > vector2.X && npc.position.X < vector2.X + 16.0 && (npc.position.Y + npc.height > (double)vector2.Y && npc.position.Y < vector2.Y + 16.0)){
							collision = true;
							if(Main.rand.Next(100) == 0 && Main.tile[i, j].nactive())
								WorldGen.KillTile(i, j, true, true, false);
						}
					}
				}
			}

			// If there is no collision with tiles, we check if the distance between this NPC and its target is too large, so that we can still trigger 'collision'.
			if(!collision){
				Rectangle rectangle1 = new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height);
				int maxDistance = maxDigDistance;
				bool playerCollision = true;
				for(int index = 0; index < 255; ++index){
					Rectangle rectangle2 = Rectangle.Empty;
					Player player = Main.player[index];
					if(CustomTarget != Vector2.Zero)
						rectangle2 = new Rectangle((int)CustomTarget.X - maxDistance, (int)CustomTarget.Y - maxDistance, maxDistance * 2, maxDistance * 2);
					else if(player.active)
						rectangle2 = new Rectangle((int)player.position.X - maxDistance, (int)player.position.Y - maxDistance, maxDistance * 2, maxDistance * 2);
					
					if(rectangle2 != Rectangle.Empty){
						if(rectangle1.Intersects(rectangle2)){
							playerCollision = false;
							break;
						}
					}
				}
				if(playerCollision)
					collision = true;
			}
 
			// speed determines the max speed at which this NPC can move.
			// Higher value = faster speed.
			float speed = this.speed;
			// acceleration is exactly what it sounds like. The speed at which this NPC accelerates.
			float acceleration = turnSpeed;
 
			Vector2 npcCenter = new Vector2(npc.position.X + npc.width * 0.5f, npc.position.Y + npc.height * 0.5f);
			float targetXPos, targetYPos;
			if(CustomTarget == Vector2.Zero){
				targetXPos = Main.player[npc.target].position.X + (Main.player[npc.target].width / 2);
				targetYPos = Main.player[npc.target].position.Y + (Main.player[npc.target].height / 2);
			}else{
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
			if(!collision && !fly){
				npc.TargetClosest(true);
				npc.velocity.Y = npc.velocity.Y + 0.11f;
				if(npc.velocity.Y > speed)
					npc.velocity.Y = speed;
				if(Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y) < speed * 0.4){
					if(npc.velocity.X < 0.0)
						npc.velocity.X = npc.velocity.X - acceleration * 1.1f;
					else
						npc.velocity.X = npc.velocity.X + acceleration * 1.1f;
				}else if(npc.velocity.Y == speed){
					if(npc.velocity.X < dirX)
						npc.velocity.X = npc.velocity.X + acceleration;
					else if(npc.velocity.X > dirX)
						npc.velocity.X = npc.velocity.X - acceleration;
				}else if(npc.velocity.Y > 4.0){
					if(npc.velocity.X < 0.0)
						npc.velocity.X = npc.velocity.X + acceleration * 0.9f;
					else
						npc.velocity.X = npc.velocity.X - acceleration * 0.9f;
				}
			}
			// Else we want to play some audio (soundDelay) and move towards our target.
			else{
				if(npc.soundDelay == 0){
					float num1 = length / 40f;
					if(num1 < 10.0)
						num1 = 10f;
					if(num1 > 20.0)
						num1 = 20f;
					npc.soundDelay = (int)num1;
					Main.PlaySound(15, (int)npc.position.X, (int)npc.position.Y, 1);
				}
				float absDirX = Math.Abs(dirX);
				float absDirY = Math.Abs(dirY);
				float newSpeed = speed / length;
				dirX *= newSpeed;
				dirY *= newSpeed;
				if(npc.velocity.X > 0.0 && dirX > 0.0 || npc.velocity.X < 0.0 && dirX < 0.0 || (npc.velocity.Y > 0.0 && dirY > 0.0 || npc.velocity.Y < 0.0 && dirY < 0.0)){
					if(npc.velocity.X < dirX)
						npc.velocity.X = npc.velocity.X + acceleration;
					else if(npc.velocity.X > dirX)
						npc.velocity.X = npc.velocity.X - acceleration;
					if(npc.velocity.Y < dirY)
						npc.velocity.Y = npc.velocity.Y + acceleration;
					else if(npc.velocity.Y > dirY)
						npc.velocity.Y = npc.velocity.Y - acceleration;
					if(Math.Abs(dirY) < speed * 0.2 && (npc.velocity.X > 0.0 && dirX < 0.0 || npc.velocity.X < 0.0 && dirX > 0.0)){
						if(npc.velocity.Y > 0.0)
							npc.velocity.Y = npc.velocity.Y + acceleration * 2f;
						else
							npc.velocity.Y = npc.velocity.Y - acceleration * 2f;
					}
					if(Math.Abs(dirX) < speed * 0.2 && (npc.velocity.Y > 0.0 && dirY < 0.0 || npc.velocity.Y < 0.0 && dirY > 0.0)){
						if(npc.velocity.X > 0.0)
							npc.velocity.X = npc.velocity.X + acceleration * 2f;
						else
							npc.velocity.X = npc.velocity.X - acceleration * 2f;
					}
				}else if(absDirX > absDirY){
					if(npc.velocity.X < dirX)
						npc.velocity.X = npc.velocity.X + acceleration * 1.1f;
					else if(npc.velocity.X > dirX)
						npc.velocity.X = npc.velocity.X - acceleration * 1.1f;
					if(Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y) < speed * 0.5){
						if(npc.velocity.Y > 0.0)
							npc.velocity.Y = npc.velocity.Y + acceleration;
						else
							npc.velocity.Y = npc.velocity.Y - acceleration;
					}
				}else{
					if(npc.velocity.Y < dirY)
						npc.velocity.Y = npc.velocity.Y + acceleration * 1.1f;
					else if(npc.velocity.Y > dirY)
						npc.velocity.Y = npc.velocity.Y - acceleration * 1.1f;
					if(Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y) < speed * 0.5){
						if(npc.velocity.X > 0.0)
							npc.velocity.X = npc.velocity.X + acceleration;
						else
							npc.velocity.X = npc.velocity.X - acceleration;
					}
				}
			}
			// Set the correct rotation for this NPC.
			npc.rotation = npc.velocity.ToRotation();
		   
			// Some netupdate stuff (multiplayer compatibility).
			if(collision){
				if(npc.localAI[0] != 1)
					npc.netUpdate = true;
				npc.localAI[0] = 1f;
			}else{
				if(npc.localAI[0] != 0.0)
					npc.netUpdate = true;
				npc.localAI[0] = 0.0f;
			}
			if((npc.velocity.X > 0.0 && npc.oldVelocity.X < 0.0 || npc.velocity.X < 0.0 && npc.oldVelocity.X > 0.0 || (npc.velocity.Y > 0.0 && npc.oldVelocity.Y < 0.0 || npc.velocity.Y < 0.0 && npc.oldVelocity.Y > 0.0)) && !npc.justHit)
				npc.netUpdate = true;
		}

		internal void PreAI_BodyTail(){
			if(npc.ai[3] > 0)
				npc.realLife = (int)npc.ai[3];
			if(npc.target < 0 || npc.target == byte.MaxValue || Main.player[npc.target].dead)
				npc.TargetClosest(true);
			if(Main.player[npc.target].dead && npc.timeLeft > 30000)
				npc.timeLeft = 10;
 
			if(Main.netMode != 1){
				if(!Main.npc[(int)npc.ai[1]].active){
					npc.life = 0;
					npc.HitEffect(0, 10.0);
					npc.active = false;
				}
			}
 
			if(npc.ai[1] < (double)Main.npc.Length){
				// We're getting the center of this NPC.
				Vector2 npcCenter = new Vector2(npc.position.X + (float)npc.width * 0.5f, npc.position.Y + (float)npc.height * 0.5f);
				// Then using that center, we calculate the direction towards the 'parent NPC' of this NPC.
				float dirX = Main.npc[(int)npc.ai[1]].position.X + (float)(Main.npc[(int)npc.ai[1]].width / 2) - npcCenter.X;
				float dirY = Main.npc[(int)npc.ai[1]].position.Y + (float)(Main.npc[(int)npc.ai[1]].height / 2) - npcCenter.Y;
				// We then use Atan2 to get a correct rotation towards that parent NPC.
				npc.rotation = (float)Math.Atan2(dirY, dirX);
				// We also get the length of the direction vector.
				float length = (float)Math.Sqrt(dirX * dirX + dirY * dirY);
				// We calculate a new, correct distance.
				float dist = (length - (float)npc.width) / length;
				float posX = dirX * dist;
				float posY = dirY * dist;
 
				// Reset the velocity of this NPC, because we don't want it to move on its own
				npc.velocity = Vector2.Zero;
				// And set this NPCs position accordingly to that of this NPCs parent NPC.
				npc.position.X = npc.position.X + posX;
				npc.position.Y = npc.position.Y + posY;
			}
		}

		/// <summary>
		/// Override this method to spawn custom body segments.  Returns the last NPC index created.
		/// </summary>
		public virtual int SetCustomBodySegments(int startDistance){
			return npc.whoAmI;	//Return the head segment index by default
		}

		/// <summary>
		/// Creates a new body segment of the specified type that follows this segment NPC.  Also decrements "distance".
		/// </summary>
		/// <returns>The ID of the new latestNPC.</returns>
		public int NewBodySegment(int type, int latestNPC){
			// We spawn a new NPC, setting latestNPC to the newer NPC, whilst also using that same variable
			// to set the parent of this new NPC. The parent of the new NPC (may it be a tail or body part)
			// will determine the movement of this new NPC.
			// Under there, we also set the realLife value of the new NPC, because of what is explained above.
			//		(in AI() method)
			latestNPC = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, type, npc.whoAmI, 0, latestNPC, 1);
			Main.npc[latestNPC].realLife = npc.whoAmI;
			Main.npc[latestNPC].ai[3] = npc.whoAmI;
			return latestNPC;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor){
			Texture2D texture = Main.npcTexture[npc.type];
			Vector2 origin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);

			Vector2 position = npc.position - Main.screenPosition;
			position.Y += texture.Height / 2f;
			position.X += texture.Width / 2f;

			float rotation = CosmivengeonMod.ToActualAngle(npc.rotation);

			SpriteEffects effect = SpriteEffects.FlipVertically;

			if(rotation > MathHelper.PiOver2 && rotation < 3 * MathHelper.PiOver2)
				effect = SpriteEffects.None;

			Main.spriteBatch.Draw(texture, position, new Rectangle?(), drawColor, npc.rotation, origin, npc.scale, effect, 0);
			return false;
		}
		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position){
			return head;	//Only let the head segment have a healtbar
		}
		public override bool CheckActive(){
			return !head;
		}
	}
}
