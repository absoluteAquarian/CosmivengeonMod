using CosmivengeonMod.API.Edits.Detours.Desomode;
using CosmivengeonMod.Buffs.Harmful;
using CosmivengeonMod.Utility;
using CosmivengeonMod.Utility.Extensions;
using CosmivengeonMod.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.NPCs.Global {
	public class DesomodeNPC : GlobalNPC {
		public override bool InstancePerEntity => true;

		public bool EoW_Spawn;
		public int EoW_WormSegmentsCount;
		public Vector2? EoW_GrabTarget;

		public float QB_baseScale;
		public Vector2 QB_baseSize;

		public override void OnHitByProjectile(NPC npc, Projectile projectile, int damage, float knockback, bool crit) {
			//Desolation mode Eater of Worlds head segment destroys any piercing projectiles that hit it
			//Don't kill minions though because that's :notcoolandgood:
			if (!WorldEvents.desoMode)
				return;

			if (npc.type == NPCID.EaterofWorldsHead && projectile.penetrate != 1 && !projectile.minion)
				projectile.Kill();
		}

		public override void OnHitPlayer(NPC npc, Player target, int damage, bool crit) {
			if (!WorldEvents.desoMode)
				return;

			//Shorter debuff time during the panic phase
			if (npc.type == NPCID.EyeofCthulhu)
				target.AddBuff(BuffID.Obstructed, npc.ai[0] == 6f ? 60 : 3 * 60);
			//EoW and Vile Spit both inflict Rotting
			else if (npc.type == NPCID.VileSpit)
				target.AddBuff(ModContent.BuffType<Rotting>(), 5 * 60);
			else if (npc.type >= NPCID.EaterofWorldsHead && npc.type <= NPCID.EaterofWorldsTail) {
				target.AddBuff(ModContent.BuffType<Rotting>(), 15 * 60);

				if (npc.type == NPCID.EaterofWorldsHead && DetourNPCHelper.EoW_GrabbingNPC == -1)
					DetourNPCHelper.EoW_SetGrab(npc, target);
			} else if (npc.type == NPCID.SkeletronHead) {
				//Stunlocking is cringe!
				//Knockback has already been set, so let's just increase its strength
				if (!target.noKnockback && (!target.mount.Active || !target.mount.Cart))
					target.velocity = Vector2.Normalize(target.velocity) * (target.velocity.Length() + 20f);
			} else if (npc.type == NPCID.SkeletronHand) {
				//Increase the knockback received based on the hand's velocity
				if (!target.noKnockback && (!target.mount.Active || !target.mount.Cart)) {
					//Slap velocity goes up to 22f
					float kbBase = 12f * npc.velocity.Length() / 22f;
					if (kbBase > 12f)
						kbBase = 12f;

					target.velocity = Vector2.Normalize(target.velocity) * (target.velocity.Length() + kbBase);
				}
			}
		}

		public override bool PreAI(NPC npc) {
			if (!WorldEvents.desoMode || Main.wof < 0 || Main.wof >= Main.maxNPCs)
				return true;

			//If the Wall of Flesh was snapped fowards, keep "The Hungry"s in front of it
			NPC wof = Main.npc[Main.wof];
			if (npc.type == NPCID.TheHungry && wof.Helper().Flag2) {
				float targetX = wof.Center.X + wof.direction * 6 * 16;

				if ((wof.direction == 1 && npc.Center.X < targetX) || (wof.direction == -1 && npc.Center.X > targetX))
					npc.Center = new Vector2(targetX, npc.Center.Y);
			}

			return true;
		}

		public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			if (!WorldEvents.desoMode)
				return true;

			if (npc.type == NPCID.EyeofCthulhu) {
				bool prePanicPhaseQuickBackdash = npc.ai[0] == 6f && npc.ai[3] == -1f && npc.ai[1] == 1f;
				bool panicPhase = npc.ai[0] == 6f && npc.ai[3] == 0f;

				if (prePanicPhaseQuickBackdash || (npc.alpha == 0 && panicPhase)) {
					Texture2D texture = TextureAssets.Npc[npc.type].Value;
					int frameCount = Main.npcFrameCount[npc.type];
					Vector2 animationFrameCenter = new Vector2(texture.Width / 2, texture.Height / frameCount / 2);
					SpriteEffects spriteEffects = SpriteEffects.None;
					if (npc.spriteDirection == 1)
						spriteEffects = SpriteEffects.FlipHorizontally;

					//Copied from Draek code and edited
					for (int i = 0; i < npc.oldPos.Length; i++) {
						Vector2 drawPos = npc.oldPos[i] - Main.screenPosition + npc.Size / 2f;

						Color color = npc.GetAlpha(drawColor) * (((float)npc.oldPos.Length - i) / npc.oldPos.Length);
						color.A = (byte)(0.75f * 255f * (npc.oldPos.Length - i) / npc.oldPos.Length);   //Apply transparency

						spriteBatch.Draw(texture, drawPos, npc.frame, color, npc.rotation, animationFrameCenter, npc.scale, spriteEffects, 0f);
					}
				}
			} else if (npc.type >= NPCID.EaterofWorldsHead && npc.type <= NPCID.EaterofWorldsTail && Debug.debug_showEoWOutlines) {
				if (npc.type == NPCID.EaterofWorldsHead)
					EoW_DrawOutline(spriteBatch, npc, "head");
				else if (npc.type == NPCID.EaterofWorldsBody)
					EoW_DrawOutline(spriteBatch, npc, "body");
				else if (npc.type == NPCID.EaterofWorldsTail)
					EoW_DrawOutline(spriteBatch, npc, "tail");
			} else if (npc.type == NPCID.QueenBee && npc.Helper().Flag) {
				drawColor = Color.Red;

				float offset = ((float)Math.Sin(MathHelper.ToRadians(npc.Helper().Timer * 2.5f * 6f)) + 1f) / 2f;
				offset *= 32f;

				//Draw the auras
				Texture2D texture = TextureAssets.Npc[npc.type].Value;
				SpriteEffects effects = npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
				Vector2 draw = npc.Top - Main.screenPosition;
				for (int i = 0; i < 4; i++) {
					Vector2 dir = Vector2.UnitX.RotateDegrees(rotateByDegrees: 90 * i, rotateByRandomDegrees: 0) * offset;

					spriteBatch.Draw(texture, draw + dir, npc.frame, drawColor * 0.5f, npc.rotation, npc.frame.Size() / 2f, npc.scale, effects, 0);
				}

				//Then the original NPC
				spriteBatch.Draw(texture, draw, npc.frame, drawColor, npc.rotation, npc.frame.Size() / 2f, npc.scale, effects, 0);

				return false;
			}

			return true;
		}

		private void EoW_DrawOutline(SpriteBatch spriteBatch, NPC npc, string segment) {
			Texture2D texture = ModContent.GetTexture($"CosmivengeonMod/NPCs/Desomode/EoW outline {segment}");
			spriteBatch.Draw(texture, npc.Center - Main.screenPosition, null, EoW_GetOutlineColor(npc), npc.rotation, texture.Size() / 2f, npc.scale, SpriteEffects.None, 0);
		}

		private Color EoW_GetOutlineColor(NPC npc) {
			if (npc.type < NPCID.EaterofWorldsHead || npc.type > NPCID.EaterofWorldsTail)
				return Color.Transparent;

			DetourNPCHelper helper = npc.Helper();
			switch (helper.EoW_SegmentType) {
				case DesolationModeBossAI.EoW_SegmentType_SpawnEaters:
					return Color.Red;
				case DesolationModeBossAI.EoW_SegmentType_SpitCursedFlames:
					return Color.LimeGreen;
				default:
					return Color.Yellow;
			}
		}

		public override bool CheckDead(NPC npc) {
			//Only deactivate the shader if this is the last EoC npc alive
			if (npc.type == NPCID.EyeofCthulhu) {
				if (FilterCollection.Screen_EoC.Active && NPC.CountNPCS(NPCID.EyeofCthulhu) == 1)
					FilterCollection.Screen_EoC.Deactivate();

				if (DetourNPCHelper.EoC_FirstBloodWallNPC == npc.whoAmI)
					DetourNPCHelper.EoC_FirstBloodWallNPC = -1;
			}

			if (!WorldEvents.desoMode)
				return true;

			//EoW segments can either spawn eaters, spit cursed flames or just do nothing
			//Only spawn eaters if >= 8 segments are left in this worm
			if (npc.type >= NPCID.EaterofWorldsHead && npc.type <= NPCID.EaterofWorldsTail) {
				if (npc.Helper().EoW_SegmentType == DesolationModeBossAI.EoW_SegmentType_SpawnEaters) {
					int spawns = Main.rand.Next(0, 4);
					for (int i = 0; i < spawns; i++) {
						int index = MiscUtils.SpawnNPCSynced(npc.Center, NPCID.EaterofSouls);
						if (index != Main.maxNPCs) {
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
