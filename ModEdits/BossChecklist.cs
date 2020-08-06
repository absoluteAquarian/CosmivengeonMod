using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.ID;

namespace CosmivengeonMod.ModEdits{
	public static class BossChecklist{
		private static Type BossRadarUI;
		private static MethodInfo BossRadarUI_SetDrawPos;

		public static void Load(){
			if(ModReferences.BossChecklistActive){
				BossRadarUI = null;
				Assembly assembly = ModReferences.BossChecklist.GetType().Assembly;

				//Get the class
				Type[] types = assembly.GetTypes();
				foreach(Type t in types){
					if(t.Name == "BossRadarUI"){
						BossRadarUI = t;
						break;
					}
				}

				//Then the methods
				if(BossRadarUI != null){
					BossRadarUI_SetDrawPos = BossRadarUI.GetMethod("SetDrawPos", BindingFlags.Instance | BindingFlags.NonPublic);
				}

				//And patch the method
				if(BossRadarUI_SetDrawPos != null)
					ModifySetDrawPos += PatchSetDrawPos;
			}
		}

		private static void PatchSetDrawPos(ILContext il){
			//Reflection stuff to make this easier
			FieldInfo NPC_active = typeof(NPC).GetField("active", BindingFlags.Instance | BindingFlags.Public);

			ILCursor c = new ILCursor(il);
			ILLabel end = c.DefineLabel();

			/*   Inserting this code here:
			 *   
			 *   NPC npc = Main.npc[i];
			 *   if (BossRadarUI.type.Count < 20 && npc.active
			 *       && !(npc.type == NPCID.EyeOfCthulhu && npc.ai[0] == 6f && npc.ai[3] == 0f)   <== here
			 *       && Array.BinarySearch<int>(BossRadarUI.whitelistNPCs, npc.type) > -1)
			 *   
			 *   IL_005C: ldloc.1
			 *   IL_005D: ldfld     bool [Terraria]Terraria.Entity::active
			 *   IL_0062: brfalse   IL_03DF
			 *                               <== here
			 */
			if(!c.TryGotoNext(MoveType.After,	i => i.MatchLdloc(1),
												i => i.MatchLdfld(NPC_active),
												i => i.MatchBrfalse(out end)))
				goto bad_il;
			c.Emit(OpCodes.Ldloc_1);
			c.EmitDelegate<Func<NPC, bool>>(npc => npc.type == NPCID.EyeofCthulhu && npc.ai[0] == 6f && npc.ai[3] == 0f);
			c.Emit(OpCodes.Brtrue, end);

			ILHelper.UpdateInstructionOffsets(c);

			ILHelper.CompleteLog(c);

			return;
			//Using a label here because it's easier
bad_il:
			CosmivengeonMod.Instance.Logger.Error("Unable to fully patch BossChecklist.BossRadarUI.SetDrawPos()");
		}

		public static void Unload(){
			if(BossRadarUI_SetDrawPos != null)
				ModifySetDrawPos -= PatchSetDrawPos;
		}

		//The method manipulators
		private static event ILContext.Manipulator ModifySetDrawPos{
			add => HookEndpointManager.Modify(BossRadarUI_SetDrawPos, value);
			remove => HookEndpointManager.Unmodify(BossRadarUI_SetDrawPos, value);
		}
	}
}
