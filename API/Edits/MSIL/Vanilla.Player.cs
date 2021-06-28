using CosmivengeonMod.Players;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Reflection;
using Terraria;
using Terraria.ID;

namespace CosmivengeonMod.API.Edits.MSIL{
	public static partial class Vanilla{
		public static void Player_GrappleMovement(ILContext il){
			FieldInfo Player_doubleJumpUnicorn = typeof(Player).GetField("doubleJumpUnicorn", BindingFlags.Public | BindingFlags.Instance);
			FieldInfo Player_jumpAgainUnicorn = typeof(Player).GetField("jumpAgainUnicorn", BindingFlags.Public | BindingFlags.Instance);

			ILCursor c = new ILCursor(il);

			ILHelper.CompleteLog(c, beforeEdit: true);

			/*  First and only patch: updating some flags, yet again
			 *  
			 *  if (this.doubleJumpUnicorn)
			 *  {
			 *      this.jumpAgainUnicorn = true;
			 *  }
			 *       <== here
			 *  this.grappling[0] = 0;
			 *  
			 *  IL_08A5: ldarg.0
			 *  IL_08A6: ldfld     bool Terraria.Player::doubleJumpUnicorn
			 *  IL_08AB: brfalse.s IL_08B4                                 (replacing this branch target)
			 *  IL_08AD: ldarg.0
			 *  IL_08AE: ldc.i4.1
			 *  IL_08AF: stfld     bool Terraria.Player::jumpAgainUnicorn
			 *       <== here
			 *  IL_08B4: ldarg.0
			 *  IL_08B5: ldfld     int32[] Terraria.Player::grappling
			 */
			ILLabel afterBlock = null;
			if(!c.TryGotoNext(MoveType.After, i => i.MatchLdarg(0),
											  i => i.MatchLdfld(Player_doubleJumpUnicorn),
											  i => i.MatchBrfalse(out afterBlock),
											  i => i.MatchLdarg(0),
											  i => i.MatchLdcI4(1),
											  i => i.MatchStfld(Player_jumpAgainUnicorn)))
				goto bad_il;
			ILLabel emitStart = c.MarkLabel();
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Func<Player, bool>>(player => player.GetModPlayer<AccessoriesPlayer>().oronitusJump.abilityActive);
			c.Emit(OpCodes.Brfalse_S, afterBlock);
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Action<Player>>(player => player.GetModPlayer<AccessoriesPlayer>().oronitusJump.jumpAgain = true);
			//Move back and replace the branch target
			c.Index = 0;
			if(!c.TryGotoNext(MoveType.After, i => i.MatchLdarg(0),
											  i => i.MatchLdfld(Player_doubleJumpUnicorn),
											  i => i.MatchBrfalse(out _)))
				goto bad_il;
			c.Index--;
			c.Instrs[c.Index].Operand = emitStart;

			ILHelper.UpdateInstructionOffsets(c);

			ILHelper.CompleteLog(c);

			return;
bad_il:
			CoreMod.Instance.Logger.Error("Unable to fully patch Terraria.Player.GrappleMovement()");
		}

		public static void Player_CarpetMovement(ILContext il){
			FieldInfo Player_jumpAgainUnicorn = typeof(Player).GetField("jumpAgainUnicorn", BindingFlags.Public | BindingFlags.Instance);

			ILCursor c = new ILCursor(il);

			ILHelper.CompleteLog(c, beforeEdit: true);

			/*  First an only patch:  adding a check for the Jewel of Oronitus jump again flag
			 *  
			 *  !this.jumpAgainBlizzard && !this.jumpAgainFart && !this.jumpAgainSail && !this.jumpAgainUnicorn
			 *	     <== here
			 *  && this.jump == 0 && this.velocity.Y != 0f
			 *  
			 *  IL_0052: ldarg.0
			 *  IL_0053: ldfld     bool Terraria.Player::jumpAgainUnicorn
			 *  IL_0058: brtrue    IL_01E7
			 *       <== here
			 *  IL_005D: ldarg.0
			 *  IL_005E: ldfld     int32 Terraria.Player::jump
			 */
			ILLabel afterBlock = null;
			if(!c.TryGotoNext(MoveType.After, i => i.MatchLdarg(0),
											  i => i.MatchLdfld(Player_jumpAgainUnicorn),
											  i => i.MatchBrtrue(out afterBlock)))
				goto bad_il;
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Func<Player, bool>>(player => player.GetModPlayer<AccessoriesPlayer>().oronitusJump.jumpAgain);
			c.Emit(OpCodes.Brtrue, afterBlock);

			ILHelper.UpdateInstructionOffsets(c);

			ILHelper.CompleteLog(c);

			return;
bad_il:
			CoreMod.Instance.Logger.Error("Unable to fully patch Terraria.Player.CarpetMovement()");
		}

		public static void Player_JumpMovement(ILContext il){
			//This is where the actual jump happens
			FieldInfo Player_jumpAgainUnicorn = typeof(Player).GetField("jumpAgainUnicorn", BindingFlags.Public | BindingFlags.Instance);
			FieldInfo Player_jumpAgainBlizzard = typeof(Player).GetField("jumpAgainBlizzard", BindingFlags.Public | BindingFlags.Instance);
			FieldInfo Player_doubleJumpUnicorn = typeof(Player).GetField("doubleJumpUnicorn", BindingFlags.Public | BindingFlags.Instance);
			FieldInfo Player_autoJump = typeof(Player).GetField("autoJump", BindingFlags.Public | BindingFlags.Instance);
			FieldInfo Player_justJumped = typeof(Player).GetField("justJumped", BindingFlags.Public | BindingFlags.Instance);
			FieldInfo Vector2_X = typeof(Vector2).GetField("X", BindingFlags.Public | BindingFlags.Instance);

			ILCursor c = new ILCursor(il);

			ILHelper.CompleteLog(c, beforeEdit: true);

			/*  First edit:  inserting the 'jumpAgain' check for the custom jump
			 *  
			 *  || this.jumpAgainFart || this.jumpAgainSail || this.jumpAgainUnicorn
			 *          <== here
			 *  || (this.wet &&
			 *  
			 *  IL_02B1: brtrue.s  IL_02EE
			 *  IL_02B3: ldarg.0
			 *  IL_02B4: ldfld     bool Terraria.Player::jumpAgainUnicorn
			 *  IL_02B9: brtrue.s  IL_02EE
			 *          <== here
			 *  IL_02BB: ldarg.0
			 */
			ILLabel afterWetCheck = null;
			if(!c.TryGotoNext(MoveType.After, i => i.MatchBrtrue(out _),
											  i => i.MatchLdarg(0),
											  i => i.MatchLdfld(Player_jumpAgainUnicorn),
											  i => i.MatchBrtrue(out afterWetCheck)))
				goto bad_il;
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Func<Player, bool>>(player => player.GetModPlayer<AccessoriesPlayer>().oronitusJump.jumpAgain);
			c.Emit(OpCodes.Brtrue_S, afterWetCheck);

			/*  Second edit:  resetting the 'flag' field for the custom jump
			 *  
			 *  bool flag6 = false;
			 *  bool flag7 = false;
			 *       <== here
			 *  if (!flag)
			 *  
			 *  IL_036E: ldc.i4.0
			 *  IL_036F: stloc.s   V_12
			 *  IL_0371: ldc.i4.0
			 *  IL_0372: stloc.s   V_13
			 *       <== here
			 *  IL_0374: ldloc.s   V_7
			 */
			if(!c.TryGotoNext(MoveType.After, i => i.MatchLdcI4(0),
											  i => i.MatchStloc(12),
											  i => i.MatchLdcI4(0),
											  i => i.MatchStloc(13)))
				goto bad_il;
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Action<Player>>(player => player.GetModPlayer<AccessoriesPlayer>().oronitusJump.flag = false);

			/*  Third edit:  inserting the 'flag' field check
			 *  
			 *  else if (this.jumpAgainBlizzard)
			 *  {
			 *      flag4 = true;
			 *      this.jumpAgainBlizzard = false;
			 *  }
			 *       <== here
			 *  else if (this.jumpAgainFart)
			 *  
			 *  IL_03A0: ldarg.0
			 *  IL_03A1: ldfld     bool Terraria.Player::jumpAgainBlizzard
			 *  IL_03A6: brfalse.s IL_03B4                                    (replacing this branch target)
			 *  IL_03A8: ldc.i4.1
			 *  IL_03A9: stloc.s   V_10
			 *  IL_03AB: ldarg.0
			 *  IL_03AC: ldc.i4.0
			 *  IL_03AD: stfld     bool Terraria.Player::jumpAgainBlizzard
			 *  IL_03B2: br.s      IL_03E3
			 *       <== here
			 *  IL_03B4: ldarg.0
			 */
			ILLabel branchToFartJumpCheck = null, branchAfterChecks = null;
			if(!c.TryGotoNext(MoveType.After, i => i.MatchLdarg(0),
											  i => i.MatchLdfld(Player_jumpAgainBlizzard),
											  i => i.MatchBrfalse(out branchToFartJumpCheck),
											  i => i.MatchLdcI4(1),
											  i => i.MatchStloc(10),
											  i => i.MatchLdarg(0),
											  i => i.MatchLdcI4(0),
											  i => i.MatchStfld(Player_jumpAgainBlizzard),
											  i => i.MatchBr(out branchAfterChecks)))
				goto bad_il;
			ILLabel emitStart = c.MarkLabel();
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Func<Player, bool>>(player => player.GetModPlayer<AccessoriesPlayer>().oronitusJump.jumpAgain);
			c.Emit(OpCodes.Brfalse_S, branchToFartJumpCheck);
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Action<Player>>(player => {
				player.GetModPlayer<AccessoriesPlayer>().oronitusJump.flag = true;
				player.GetModPlayer<AccessoriesPlayer>().oronitusJump.jumpAgain = false;
			});
			c.Emit(OpCodes.Br_S, branchAfterChecks);
			//Move back to the 'brfalse.s' instruction used in the search and modify its jump target
			c.Index = 0;
			if(!c.TryGotoNext(MoveType.After, i => i.MatchLdarg(0),
											  i => i.MatchLdfld(Player_jumpAgainBlizzard),
											  i => i.MatchBrfalse(out branchToFartJumpCheck),
											  i => i.MatchLdcI4(1),
											  i => i.MatchStloc(10)))
				goto bad_il;
			c.Index -= 3;
			c.Instrs[c.Index].Operand = emitStart;

			/*  Fourth edit:  setting the 'jumpAgain' field again if autojumping
			 *  
			 *  if ((this.velocity.Y == 0f || this.sliding || (this.autoJump && this.justJumped)) && this.doubleJumpUnicorn)
			 *  {
			 *      this.jumpAgainUnicorn = true;
			 *  }
			 *       <== here
			 *  if (this.velocity.Y == 0f || flag2 || this.sliding || flag)
			 *  
			 *  IL_0529: ldfld     bool Terraria.Player::autoJump
			 *  IL_052E: brfalse.s IL_0547                            (replacing this branch target)
			 *  IL_0530: ldarg.0
			 *  IL_0531: ldfld     bool Terraria.Player::justJumped
			 *  IL_0536: brfalse.s IL_0547                            (replacing this branch target)
			 *  IL_0538: ldarg.0
			 *  IL_0539: ldfld     bool Terraria.Player::doubleJumpUnicorn
			 *  IL_053E: brfalse.s IL_0547                            (replacing this branch target)
			 *  IL_0540: ldarg.0
			 *  IL_0541: ldc.i4.1
			 *  IL_0542: stfld     bool Terraria.Player::jumpAgainUnicorn
			 *       <== here
			 *  IL_0547: ldarg.0
			 */
			ILLabel velocityCheckAfterDJumpSetting = null;
			if(!c.TryGotoNext(MoveType.After, i => i.MatchLdarg(0),
											  i => i.MatchLdfld(Player_doubleJumpUnicorn),
											  i => i.MatchBrfalse(out velocityCheckAfterDJumpSetting),
											  i => i.MatchLdarg(0),
											  i => i.MatchLdcI4(1),
											  i => i.MatchStfld(Player_jumpAgainUnicorn)))
				goto bad_il;
			ILLabel secondEmitStart = c.MarkLabel();
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Func<Player, bool>>(player
				=> (player.velocity.Y == 0f || player.sliding || (player.autoJump && player.justJumped)) && player.GetModPlayer<AccessoriesPlayer>().oronitusJump.abilityActive);
			c.Emit(OpCodes.Brfalse_S, velocityCheckAfterDJumpSetting);
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Action<Player>>(player => player.GetModPlayer<AccessoriesPlayer>().oronitusJump.jumpAgain = true);
			//Move back and edit those three branch instructions' operands
			c.Index = 0;
			if(!c.TryGotoNext(MoveType.Before, i => i.MatchLdfld(Player_autoJump),
											   i => i.MatchBrfalse(out _),
											   i => i.MatchLdarg(0),
											   i => i.MatchLdfld(Player_justJumped),
											   i => i.MatchBrfalse(out _),
											   i => i.MatchLdarg(0),
											   i => i.MatchLdfld(Player_doubleJumpUnicorn),
											   i => i.MatchBrfalse(out _),
											   i => i.MatchLdarg(0),
											   i => i.MatchLdcI4(1),
											   i => i.MatchStfld(Player_jumpAgainUnicorn)))
				goto bad_il;
			c.Index++;
			c.Instrs[c.Index].Operand = secondEmitStart;
			c.Index += 3;
			c.Instrs[c.Index].Operand = secondEmitStart;
			c.Index += 3;
			c.Instrs[c.Index].Operand = secondEmitStart;

			/*  Fifth edit:  signifying that the effect code should run and actually applying the double jump
			 *  
			 *  if (this.velocity.Y == 0f || flag2 || this.sliding || flag)
			 *  {
			 *      this.velocity.Y = (0f - Player.jumpSpeed) * this.gravDir;
			 *      this.jump = Player.jumpHeight;
			 *      if (this.sliding)
			 *      {
			 *          this.velocity.X = (float)(3 * -(float)this.slideDir);
			 *      }
			 *  }
			 *       <== here
			 *  else if (flag3)
			 *  
			 *  IL_0567: ldloc.s   V_7
			 *  IL_0569: or
			 *  IL_056A: brfalse.s IL_05B9                        (replacing this branch target)
			 *  IL_056C: ldarg.0
			 *   ...
			 *  IL_05AD: mul
			 *  IL_05AE: conv.r4
			 *  IL_05AF: stfld     float32 [Microsoft.Xna.Framework]Microsoft.Xna.Framework.Vector2::X
			 *  IL_05B4: br        IL_1337
			 *       <== here
			 *  IL_05B9: ldloc.s   V_9
			 */
			ILLabel nextBranch = null, blockEnd = null;
			if(!c.TryGotoNext(MoveType.After, i => i.MatchLdloc(7),
											  i => i.MatchOr(),
											  i => i.MatchBrfalse(out nextBranch),
											  i => i.MatchLdarg(0)))
				goto bad_il;
			if(!c.TryGotoNext(MoveType.After, i => i.MatchMul(),
											  i => i.MatchConvR4(),
											  i => i.MatchStfld(Vector2_X),
											  i => i.MatchBr(out blockEnd)))
				goto bad_il;
			ILLabel finalEmitStart = c.MarkLabel();
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Func<Player, bool>>(player => player.GetModPlayer<AccessoriesPlayer>().oronitusJump.flag);
			c.Emit(OpCodes.Brfalse_S, nextBranch);
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Action<Player>>(player => {
				player.GetModPlayer<AccessoriesPlayer>().oronitusJump.jumpEffectActive = true;
				int height2 = player.height;
				float num3 = player.gravDir;
				Main.PlaySound(SoundID.DoubleJump, player.position);
				player.velocity.Y = -Player.jumpSpeed * player.gravDir;
				//Blizzard double jump uses a factor of 1.5f
				player.jump = (int)(Player.jumpHeight * 1.6f);
			});
			c.Emit(OpCodes.Br_S, blockEnd);
			//Replace the jump target that we got 'nextBranch' from to 'finalEmitStart'
			c.Index = 0;
			if(!c.TryGotoNext(MoveType.After, i => i.MatchLdloc(7),
											  i => i.MatchOr(),
											  i => i.MatchBrfalse(out nextBranch),
											  i => i.MatchLdarg(0)))
				goto bad_il;
			c.Index -= 2;
			c.Instrs[c.Index].Operand = finalEmitStart;

			ILHelper.UpdateInstructionOffsets(c);

			ILHelper.CompleteLog(c);

			return;
bad_il:
			CoreMod.Instance.Logger.Error("Unable to fully patch Terraria.Player.Update()");
		}

		public static void Player_Update(ILContext il){
			//This is where 'CosmivengeonPlayer.jumpAgain_JewelOfOronitus' is set or cleared
			FieldInfo Player_jumpAgainUnicorn =   typeof(Player).GetField("jumpAgainUnicorn",   BindingFlags.Public | BindingFlags.Instance);
			FieldInfo Player_doubleJumpUnicorn =  typeof(Player).GetField("doubleJumpUnicorn",  BindingFlags.Public | BindingFlags.Instance);
			FieldInfo Player_dJumpEffectUnicorn = typeof(Player).GetField("dJumpEffectUnicorn", BindingFlags.Public | BindingFlags.Instance);
			FieldInfo Player_maxRunSpeed =        typeof(Player).GetField("maxRunSpeed",        BindingFlags.Public | BindingFlags.Instance);
			FieldInfo Player_sandStorm =          typeof(Player).GetField("sandStorm",          BindingFlags.Public | BindingFlags.Instance);
			FieldInfo Player_runAcceleration =    typeof(Player).GetField("runAcceleration",    BindingFlags.Public | BindingFlags.Instance);
			FieldInfo Entity_position =           typeof(Entity).GetField("position",           BindingFlags.Public | BindingFlags.Instance);
			MethodInfo Player_DashMovement =      typeof(Player).GetMethod("DashMovement",      BindingFlags.Public | BindingFlags.Instance);

			ILCursor c = new ILCursor(il);

			ILHelper.CompleteLog(c, beforeEdit: true);

			/*  First edit:  Some mounts block double jumps altogether. This should happen for this accessory too
			 *  
			 *  this.jumpAgainFart = false;
			 *  this.jumpAgainSail = false;
			 *  this.jumpAgainUnicorn = false;
			 *                                 <== here
			 *  
			 *  IL_2FB0: stfld     bool Terraria.Player::jumpAgainFart
			 *  IL_2FB5: ldarg.0
			 *  IL_2FB6: ldc.i4.0
			 *  IL_2FB7: stfld     bool Terraria.Player::jumpAgainSail
			 *  IL_2FBC: ldarg.0
			 *  IL_2FBD: ldc.i4.0
			 *  IL_2FBE: stfld     bool Terraria.Player::jumpAgainUnicorn
			 *             <== here
			 *  IL_2FC3: br        IL_30F4
			 */
			if(!c.TryGotoNext(MoveType.After, i => i.MatchLdarg(0),
			                                  i => i.MatchLdcI4(0),
			                                  i => i.MatchStfld(Player_jumpAgainUnicorn),
			                                  i => i.MatchBr(out _)))
				goto bad_il;
			c.Index--;
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Action<Player>>(player => player.GetModPlayer<AccessoriesPlayer>().oronitusJump.jumpAgain = false);

			/*  Second edit:  Toggling the jumpAgain flag
			 *  
			 *  else if (this.velocity.Y == 0f || this.sliding)
			 *  {
			 *      this.jumpAgainUnicorn = true;
			 *  }
			 *         <== here
			 *  
			 *  IL_30C3: ldfld     bool Terraria.Player::doubleJumpUnicorn
			 *  IL_30C8: brtrue.s  IL_30D3
			 *  IL_30CA: ldarg.0
			 *  IL_30CB: ldc.i4.0
			 *  IL_30CC: stfld     bool Terraria.Player::jumpAgainUnicorn
			 *  IL_30D1: br.s      IL_30F4                        (replacing this branch target)
			 *   ...
			 *  IL_30E6: ldfld     bool Terraria.Player::sliding
			 *  IL_30EB: brfalse.s IL_30F4                        (replacing this branch target)
			 *  IL_30ED: ldarg.0
			 *  IL_30EE: ldc.i4.1
			 *  IL_30EF: stfld     bool Terraria.Player::jumpAgainUnicorn
			 *         <== here
			 */
			if(!c.TryGotoNext(MoveType.After, i => i.MatchBrfalse(out _),
			                                  i => i.MatchLdarg(0),
			                                  i => i.MatchLdcI4(1),
			                                  i => i.MatchStfld(Player_jumpAgainUnicorn)))
				goto bad_il;
			ILLabel emitStart = c.MarkLabel();
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Action<Player>>(player => {
				AccessoriesPlayer mp = player.GetModPlayer<AccessoriesPlayer>();
				if(!mp.oronitusJump.abilityActive)
					mp.oronitusJump.jumpAgain = false;
				else if(player.velocity.Y == 0f || player.sliding)
					mp.oronitusJump.jumpAgain = true;
			});
			//Go back and modify the branch targets to 'emitStart'
			c.Index = 0;
			if(!c.TryGotoNext(MoveType.After, i => i.MatchLdfld(Player_doubleJumpUnicorn),
											  i => i.MatchBrtrue(out _),
											  i => i.MatchLdarg(0),
											  i => i.MatchLdcI4(0),
											  i => i.MatchStfld(Player_jumpAgainUnicorn),
											  i => i.MatchBr(out _)))
				goto bad_il;
			c.Index--;
			c.Instrs[c.Index].Operand = emitStart;
			if(!c.TryGotoNext(MoveType.After, i => i.MatchBrfalse(out _),
			                                  i => i.MatchLdarg(0),
			                                  i => i.MatchLdcI4(1),
			                                  i => i.MatchStfld(Player_jumpAgainUnicorn)))
				goto bad_il;
			c.Index -= 4;
			c.Instrs[c.Index].Operand = emitStart;

			/*  Third edit: resetting the "effect" flag
			 *  
			 *  this.dJumpEffectSail = false;
			 *  this.dJumpEffectUnicorn = false;
			 *       <== here
			 *  int num31 = (int)(this.position.X + (float)(this.width / 2)) / 16;
			 *  
			 *  IL_321D: ldarg.0
			 *  IL_321E: ldc.i4.0
			 *  IL_321F: stfld     bool Terraria.Player::dJumpEffectUnicorn
			 *       <== here
			 *  IL_3224: ldarg.0
			 *  IL_3225: ldflda    valuetype [Microsoft.Xna.Framework]Microsoft.Xna.Framework.Vector2 Terraria.Entity::position
			 */
			if(!c.TryGotoNext(MoveType.After, i => i.MatchLdarg(0),
			                                  i => i.MatchLdcI4(0),
			                                  i => i.MatchStfld(Player_dJumpEffectUnicorn),
			                                  i => i.MatchLdarg(0),
											  i => i.MatchLdflda(Entity_position)))
				goto bad_il;
			c.Index -= 2;
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Action<Player>>(player => player.GetModPlayer<AccessoriesPlayer>().oronitusJump.jumpEffectActive = false);

			/*  Fourth edit: making the player move faster during the custom double jump
			 *  
			 *      this.maxRunSpeed *= 2f;
			 *  }
			 *       <== here
			 *  if (this.dJumpEffectBlizzard && this.doubleJumpBlizzard)
			 *  {
			 *      this.runAcceleration *= 3f;
			 *  
			 *  IL_4389: ldarg.0
			 *  IL_438A: ldfld     bool Terraria.Player::sandStorm
			 *  IL_438F: brfalse.s IL_43B5                          (replacing this branch target)
			 *  IL_4391: ldarg.0
			 *  IL_4392: ldarg.0
			 *  IL_4393: ldfld     float32 Terraria.Player::runAcceleration
			 *  IL_4398: ldc.r4    1.5
			 *  IL_439D: mul
			 *  IL_439E: stfld     float32 Terraria.Player::runAcceleration
			 *  IL_43A3: ldarg.0
			 *  IL_43A4: ldarg.0
			 *  IL_43A5: ldfld     float32 Terraria.Player::maxRunSpeed
			 *  IL_43AA: ldc.r4    2
			 *  IL_43AF: mul
			 *  IL_43B0: stfld     float32 Terraria.Player::maxRunSpeed
			 *       <== here
			 *  IL_43B5: ldarg.0
			 *  IL_43B6: ldfld     bool Terraria.Player::dJumpEffectBlizzard
			 */
			ILLabel nextBlock = null;
			if(!c.TryGotoNext(MoveType.After, i => i.MatchLdarg(0),
											  i => i.MatchLdfld(Player_sandStorm),
											  i => i.MatchBrfalse(out nextBlock),
											  i => i.MatchLdarg(0)))
				goto bad_il;
			int branchIndex = c.Index - 2;
			if(!c.TryGotoNext(MoveType.After, i => i.MatchLdarg(0),
											  i => i.MatchLdfld(Player_maxRunSpeed),
											  i => i.MatchLdcR4(2),
											  i => i.MatchMul(),
											  i => i.MatchStfld(Player_maxRunSpeed)))
				goto bad_il;
			ILLabel label = c.MarkLabel();
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Func<Player, bool>>(player => player.GetModPlayer<AccessoriesPlayer>().oronitusJump.jumpEffectActive && player.GetModPlayer<AccessoriesPlayer>().oronitusJump.abilityActive);
			c.Emit(OpCodes.Brfalse_S, nextBlock);
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Action<Player>>(player => {
				player.runAcceleration *= 2.5f;
				player.maxRunSpeed *= 2f;
			});
			//Move back and update the branch target to the emitted code
			c.Index = branchIndex;
			c.Instrs[c.Index].Operand = label;

			/*  Fifth edit:  same as the third edit, but in a different location
			 *  
			 *      this.dJumpEffectFart = false;
			 *      this.dJumpEffectSail = false;
			 *      this.dJumpEffectUnicorn = false;
			 *           <== here
			 *  }
			 *  this.DashMovement();
			 *  
			 *  IL_48B8: stfld     bool Terraria.Player::dJumpEffectFart
			 *  IL_48BD: ldarg.0
			 *  IL_48BE: ldc.i4.0
			 *  IL_48BF: stfld     bool Terraria.Player::dJumpEffectSail
			 *  IL_48C4: ldarg.0
			 *  IL_48C5: ldc.i4.0
			 *  IL_48C6: stfld     bool Terraria.Player::dJumpEffectUnicorn
			 *       <== here
			 *  IL_48CB: ldarg.0
			 *  IL_48CC: call      instance void Terraria.Player::DashMovement()
			 */
			if(!c.TryGotoNext(MoveType.After, i => i.MatchLdarg(0),
			                                  i => i.MatchLdcI4(0),
			                                  i => i.MatchStfld(Player_dJumpEffectUnicorn),
											  i => i.MatchLdarg(0),
			                                  i => i.MatchCall(Player_DashMovement)))
				goto bad_il;
			c.Index -= 2;
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Action<Player>>(player => player.GetModPlayer<AccessoriesPlayer>().oronitusJump.jumpEffectActive = false);

			/*  Sixth edit:  the same as the third edit, but in a different location, YET AGAIN
			 *  
			 *      this.dJumpEffectFart = false;
			 *      this.dJumpEffectSail = false;
			 *      this.dJumpEffectUnicorn = false;
			 *           <== here
			 *  }
			 *  else
			 *  
			 *  IL_4AFF: ldarg.0
			 *  IL_4B00: ldc.i4.0
			 *  IL_4B01: stfld     bool Terraria.Player::dJumpEffectUnicorn
			 *       <== here
			 *  IL_4B06: br        IL_882E
			 */
			if(!c.TryGotoNext(MoveType.After, i => i.MatchLdarg(0),
			                                  i => i.MatchLdcI4(0),
			                                  i => i.MatchStfld(Player_dJumpEffectUnicorn),
			                                  i => i.MatchBr(out _)))
				goto bad_il;
			c.Index--;
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Action<Player>>(player => player.GetModPlayer<AccessoriesPlayer>().oronitusJump.jumpEffectActive = false);

			ILHelper.UpdateInstructionOffsets(c);

			ILHelper.CompleteLog(c);

			return;
bad_il:
			CoreMod.Instance.Logger.Error("Unable to fully patch Terraria.Player.Update()");
		}

		public static void Player_PlayerFrame(ILContext il){
			FieldInfo Player_dJumpEffectUnicorn = typeof(Player).GetField("dJumpEffectUnicorn", BindingFlags.Public | BindingFlags.Instance);

			ILCursor c = new ILCursor(il);

			/*  First and only edit: resetting the flag
			 *  
			 *  this.dJumpEffectSail = false;
			 *  this.dJumpEffectUnicorn = false;
			 *       <== here
			 *  Vector2 vector = new Vector2(this.position.X + (float)this.width * 0.5f, this.position.Y + (float)this.height * 0.5f);
			 *  
			 *  IL_2430: ldarg.0
			 *  IL_2431: ldc.i4.0
			 *  IL_2432: stfld     bool Terraria.Player::dJumpEffectUnicorn
			 *  IL_2437: ldloca.s  V_27
			 *  IL_2439: ldarg.0
			 *  
			 */
			if(!c.TryGotoNext(MoveType.After, i => i.MatchLdarg(0),
			                                  i => i.MatchLdcI4(0),
			                                  i => i.MatchStfld(Player_dJumpEffectUnicorn),
			                                  i => i.MatchLdloca(27)))
				goto bad_il;
			c.Index--;
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Action<Player>>(player => player.GetModPlayer<AccessoriesPlayer>().oronitusJump.jumpEffectActive = false);

			ILHelper.UpdateInstructionOffsets(c);

			ILHelper.CompleteLog(c);

			return;
bad_il:
			CoreMod.Instance.Logger.Error("Unable to fully patch Terraria.Player.PlayerFrame()");
		}
	}
}
