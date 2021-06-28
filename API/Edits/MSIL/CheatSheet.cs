using CosmivengeonMod.NPCs.Global;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using System;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.API.Edits.MSIL{
	public static class CheatSheet{
		private static Type NPCBrowser;
		private static ConstructorInfo NPCBrowser_ctor;
		private static MethodInfo NPCBrowser_Draw;

		public static void Load(){
			if(ModReferences.CheatSheet.Active){ 
				NPCBrowser = null;

				Assembly assembly = ModReferences.CheatSheet.GetType().Assembly;

				Type[] types = assembly.GetTypes();
				foreach(Type type in types){
					if(type.Name == "NPCBrowser"){
						NPCBrowser = type;
						break;
					}
				}

				if(NPCBrowser != null){
					NPCBrowser_ctor = NPCBrowser.GetConstructor(new Type[]{ ModReferences.CheatSheet.GetType() });
					NPCBrowser_Draw = NPCBrowser.GetMethod("Draw", BindingFlags.Public | BindingFlags.Instance);
				}

				if(NPCBrowser_Draw != null)
					ModifyDraw += PatchDraw;
				if(NPCBrowser_ctor != null)
					Modify_ctor += Patch_ctor;
			}else
				CoreMod.Instance.Logger.Error("Unable to patch CheatSheet since it doesn't exist.");
		}

		private static void Patch_ctor(ILContext il){
			FieldInfo NPCBrowser_textures = NPCBrowser.GetField("textures", BindingFlags.NonPublic | BindingFlags.Instance);
			MethodInfo Mod_GetTexture = typeof(Mod).GetMethod("GetTexture", BindingFlags.Public | BindingFlags.Instance);

			ILCursor c = new ILCursor(il);

			ILHelper.CompleteLog(c, beforeEdit: true);

			/*   First edit: modifying the 'textures' array to include the additional one for this mod
			 *   
			 *   textures = new Texture2D[]
			 *   {
			 *       mod.GetTexture("UI/NPCLifeIcon"),
			 *       mod.GetTexture("UI/NPCDamageIcon"),
			 *       mod.GetTexture("UI/NPCDefenseIcon"),
			 *       mod.GetTexture("UI/NPCKnockbackIcon"),
			 *   };
			 *   
			 *   IL_0319: ldarg.0
			 *   IL_031A: ldc.i4.4                          <== modifying this to push a '5' instead of a '4'
			 *   IL_031B: newarr    [Microsoft.Xna.Framework.Graphics]Microsoft.Xna.Framework.Graphics.Texture2D
			 *   IL_0320: dup
			 *   ...
			 *   callvirt  instance class [Microsoft.Xna.Framework.Graphics]Microsoft.Xna.Framework.Graphics.Texture2D [Terraria]Terraria.ModLoader.Mod::GetTexture(string)
			 *   IL_0357: stelem.ref
			 *                        <== inserting the edit here
			 *   IL_0358: stfld     class [Microsoft.Xna.Framework.Graphics]Microsoft.Xna.Framework.Graphics.Texture2D[] CheatSheet.Menus.NPCBrowser::textures
			 *   IL_035D: ret
			 */
			if(!c.TryGotoNext(MoveType.Before, i => i.MatchLdarg(0),
											   i => i.MatchLdcI4(4),
											   i => i.MatchNewarr(typeof(Texture2D)),
											   i => i.MatchDup()))
				goto bad_il;
			c.Index++;
			c.Instrs[c.Index].OpCode = OpCodes.Ldc_I4_5;

			if(!c.TryGotoNext(MoveType.Before, i => i.MatchCallvirt(Mod_GetTexture),
											   i => i.MatchStelemRef(),
											   i => i.MatchStfld(NPCBrowser_textures),
											   i => i.MatchRet()))
				goto bad_il;
			c.Index += 2;
			c.Emit(OpCodes.Dup);
			c.Emit(OpCodes.Ldc_I4_4);
			c.EmitDelegate<Func<Texture2D>>(() => CoreMod.Instance.GetTexture("Assets/CheatSheet_NPCDamageResistance"));
			c.Emit(OpCodes.Stelem_Ref);

			ILHelper.UpdateInstructionOffsets(c);

			ILHelper.CompleteLog(c);

			return;
			//Using a label here because it's easier
bad_il:
			CoreMod.Instance.Logger.Error("Unable to fully patch CheatSheet.NPCBrowser..ctor()");
		}

		private static void PatchDraw(ILContext il){
			FieldInfo NPCBrowser_tooltipNpc = NPCBrowser.GetField("tooltipNpc", BindingFlags.NonPublic | BindingFlags.Static);
			MethodInfo String_Format = typeof(string).GetMethod("Format", BindingFlags.Public | BindingFlags.Static, null, new Type[]{ typeof(string), typeof(object) }, null);

			ILCursor c = new ILCursor(il);

			ILHelper.CompleteLog(c, beforeEdit: true);

			/*   First edit: modifying what's printed
			 *   
			 *   string[] texts = { $"{tooltipNpc.lifeMax}", $"{tooltipNpc.defDamage}", $"{tooltipNpc.defDefense}", $"{tooltipNpc.knockBackResist:0.##}" };
			 *   
			 *   IL_0130: callvirt  instance void [Terraria]Terraria.NPC::SetDefaults(int32, float32)
			 *   IL_0135: ldc.i4.4                           <== modifying this to push a '5' instead of a '4'
			 *   IL_0136: newarr    [mscorlib]System.String
			 *   IL_013B: dup
			 *   ...
			 *   IL_01A0: box       [mscorlib]System.Single
			 *   IL_01A5: call      string [mscorlib]System.String::Format(string, object)
			 *   IL_01AA: stelem.ref
			 *                        <== inserting the edit here
			 *   IL_01AB: stloc.2
			 *   IL_01AC: ldloca.s  V_3
			 */
			if(!c.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(4),
											   i => i.MatchNewarr(typeof(string)),
											   i => i.MatchDup(),
											   i => i.MatchLdcI4(0)))
				goto bad_il;
			c.Instrs[c.Index].OpCode = OpCodes.Ldc_I4_5;

			if(!c.TryGotoNext(MoveType.Before, i => i.MatchBox(typeof(float)),
											   i => i.MatchCall(String_Format),
											   i => i.MatchStelemRef(),
											   i => i.MatchStloc(2),
											   i => i.MatchLdloca(3)))
				goto bad_il;
			c.Index += 3;
			c.Emit(OpCodes.Dup);
			c.Emit(OpCodes.Ldc_I4_4);
			c.Emit(OpCodes.Ldsfld, NPCBrowser_tooltipNpc);
			c.EmitDelegate<Func<NPC, string>>(npc => $"{npc.GetGlobalNPC<StatsNPC>().endurance :0.##}");
			c.Emit(OpCodes.Stelem_Ref);

			ILHelper.UpdateInstructionOffsets(c);

			ILHelper.CompleteLog(c);

			return;
			//Using a label here because it's easier
bad_il:
			CoreMod.Instance.Logger.Error("Unable to fully patch CheatSheet.NPCBrowser.Draw()");
		}

		public static void Unload(){
			if(NPCBrowser_Draw != null)
				ModifyDraw -= PatchDraw;
			if(NPCBrowser_ctor != null)
				Modify_ctor -= Patch_ctor;

			NPCBrowser = null;
		}

		//The method manipulators
		private static event ILContext.Manipulator ModifyDraw{
			add => HookEndpointManager.Modify(NPCBrowser_Draw, value);
			remove => HookEndpointManager.Unmodify(NPCBrowser_Draw, value);
		}
		private static event ILContext.Manipulator Modify_ctor{
			add => HookEndpointManager.Modify(NPCBrowser_ctor, value);
			remove => HookEndpointManager.Unmodify(NPCBrowser_ctor, value);
		}
	}
}
