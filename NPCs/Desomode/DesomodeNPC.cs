using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.NPCs.Desomode{
	public class DesomodeNPC : GlobalNPC{
		public override void OnHitPlayer(NPC npc, Player target, int damage, bool crit){
			//Shorter debuff time during the panic phase
			if(CosmivengeonWorld.desoMode && npc.type == NPCID.EyeofCthulhu)
				target.AddBuff(BuffID.Obstructed, npc.ai[0] == 6f ? 60 : 3 * 60);
		}

		public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor){
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
			}

			return true;
		}

		public override bool CheckDead(NPC npc){
			//Only deactivate the shader if this is the last EoC npc alive
			if(npc.type == NPCID.EyeofCthulhu && FilterCollection.Screen_EoC.Active && NPC.CountNPCS(NPCID.EyeofCthulhu) == 1)
				FilterCollection.Screen_EoC.Deactivate();

			if(DetourNPCHelper.EoC_FirstBloodWallNPC == npc.whoAmI)
				DetourNPCHelper.EoC_FirstBloodWallNPC = -1;

			return true;
		}
	}
}
