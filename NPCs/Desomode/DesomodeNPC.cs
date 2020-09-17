using CosmivengeonMod.Buffs;
using CosmivengeonMod.Detours;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.NPCs.Desomode{
	public class DesomodeNPC : GlobalNPC{
		public override bool InstancePerEntity => true;

		public bool EoW_Spawn;
		public int EoW_WormSegmentsCount;
		public Vector2? EoW_GrabTarget;

		public override void OnHitByProjectile(NPC npc, Projectile projectile, int damage, float knockback, bool crit){
			//Desolation mode Eater of Worlds head segment destroys any piercing projectiles that hit it
			//Don't kill minions though because that's :notcoolandgood:
			if(!CosmivengeonWorld.desoMode)
				return;

			if(npc.type == NPCID.EaterofWorldsHead && projectile.penetrate != 1 && !projectile.minion)
				projectile.Kill();
		}

		public override void OnHitPlayer(NPC npc, Player target, int damage, bool crit){
			if(!CosmivengeonWorld.desoMode)
				return;

			//Shorter debuff time during the panic phase
			if(npc.type == NPCID.EyeofCthulhu)
				target.AddBuff(BuffID.Obstructed, npc.ai[0] == 6f ? 60 : 3 * 60);
			//EoW and Vile Spit both inflict Rotting
			else if(npc.type == NPCID.VileSpit)
				target.AddBuff(ModContent.BuffType<Rotting>(), 5 * 60);
			else if(npc.type >= NPCID.EaterofWorldsHead && npc.type <= NPCID.EaterofWorldsTail){
				target.AddBuff(ModContent.BuffType<Rotting>(), 15 * 60);

				if(npc.type == NPCID.EaterofWorldsHead && DetourNPCHelper.EoW_GrabbingNPC == -1)
					DetourNPCHelper.EoW_SetGrab(npc, target);
			}
		}

		public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor){
			if(!CosmivengeonWorld.desoMode)
				return true;

			if(npc.type == NPCID.EyeofCthulhu){
				bool prePanicPhaseQuickBackdash = npc.ai[0] == 6f && npc.ai[3] == -1f && npc.ai[1] == 1f;
				bool panicPhase = npc.ai[0] == 6f && npc.ai[3] == 0f;

				if(prePanicPhaseQuickBackdash || (npc.alpha == 0 && panicPhase)){
					Texture2D texture = Main.npcTexture[npc.type];
					int frameCount = Main.npcFrameCount[npc.type];
					Vector2 animationFrameCenter = new Vector2(texture.Width / 2, texture.Height / frameCount / 2);
					SpriteEffects spriteEffects = SpriteEffects.None;
					if(npc.spriteDirection == 1)
						spriteEffects = SpriteEffects.FlipHorizontally;

					//Copied from Draek code and edited
					for(int i = 0; i < npc.oldPos.Length; i++){
						Vector2 drawPos = npc.oldPos[i] - Main.screenPosition + npc.Size / 2f;
					
						Color color = npc.GetAlpha(drawColor) * (((float)npc.oldPos.Length - i) / npc.oldPos.Length);
						color.A = (byte)(0.75f * 255f * (npc.oldPos.Length - i) / npc.oldPos.Length);	//Apply transparency

						spriteBatch.Draw(texture, drawPos, npc.frame, color, npc.rotation, animationFrameCenter, npc.scale, spriteEffects, 0f);
					}
				}
			}else if(npc.type >= NPCID.EaterofWorldsHead && npc.type <= NPCID.EaterofWorldsTail && CosmivengeonMod.debug_showEoWOutlines){
				if(npc.type == NPCID.EaterofWorldsHead)
					EoW_DrawOutline(spriteBatch, npc, "head");
				else if(npc.type == NPCID.EaterofWorldsBody)
					EoW_DrawOutline(spriteBatch, npc, "body");
				else if(npc.type == NPCID.EaterofWorldsTail)
					EoW_DrawOutline(spriteBatch, npc, "tail");
			}

			return true;
		}

		private void EoW_DrawOutline(SpriteBatch spriteBatch, NPC npc, string segment){
			Texture2D texture = ModContent.GetTexture($"CosmivengeonMod/NPCs/Desomode/EoW outline {segment}");
			spriteBatch.Draw(texture, npc.Center - Main.screenPosition, null, EoW_GetOutlineColor(npc), npc.rotation, texture.Size() / 2f, npc.scale, SpriteEffects.None, 0);
		}

		private Color EoW_GetOutlineColor(NPC npc){
			if(npc.type < NPCID.EaterofWorldsHead || npc.type > NPCID.EaterofWorldsTail)
				return Color.Transparent;

			DetourNPCHelper helper = npc.Helper();
			switch(helper.EoW_SegmentType){
				case DesolationModeBossAI.EoW_SegmentType_SpawnEaters:
					return Color.Red;
				case DesolationModeBossAI.EoW_SegmentType_SpitCursedFlames:
					return Color.LimeGreen;
				default:
					return Color.Yellow;
			}
		}

		public override bool CheckDead(NPC npc){
			//Only deactivate the shader if this is the last EoC npc alive
			if(npc.type == NPCID.EyeofCthulhu){
				if(FilterCollection.Screen_EoC.Active && NPC.CountNPCS(NPCID.EyeofCthulhu) == 1)
					FilterCollection.Screen_EoC.Deactivate();

				if(DetourNPCHelper.EoC_FirstBloodWallNPC == npc.whoAmI)
					DetourNPCHelper.EoC_FirstBloodWallNPC = -1;
			}

			if(!CosmivengeonWorld.desoMode)
				return true;

			//EoW segments can either spawn eaters, spit cursed flames or just do nothing
			//Only spawn eaters if >= 8 segments are left in this worm
			if(npc.type >= NPCID.EaterofWorldsHead && npc.type <= NPCID.EaterofWorldsTail){
				if(npc.Helper().EoW_SegmentType == DesolationModeBossAI.EoW_SegmentType_SpawnEaters){
					int spawns = Main.rand.Next(0, 4);
					for(int i = 0; i < spawns; i++){
						int index = CosmivengeonUtils.SpawnNPCSynced(npc.Center, NPCID.EaterofSouls);
						if(index != Main.maxNPCs){
							NPC spawn = Main.npc[index];
							spawn.noTileCollide = true;

							spawn.velocity = Vector2.UnitX.RotateDegrees(rotateByDegrees: 0, rotateByRandomDegrees: 360) * 6f;

							spawn.Desomode().EoW_Spawn = true;
						}
					}
				}

				DetourNPCHelper.EoW_ResetGrab(npc, npc.Target());
			}

			return true;
		}
	}
}
