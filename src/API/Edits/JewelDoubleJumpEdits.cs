using CosmivengeonMod.Players;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using SerousCommonLib.API;
using System;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace CosmivengeonMod.API.Edits {
	internal class JewelDoubleJumpEdits : Edit {
		private static Player editPlayer;

		private static readonly FieldInfo editPlayer_Field = typeof(JewelDoubleJumpEdits).GetField(nameof(editPlayer), BindingFlags.NonPublic | BindingFlags.Static);

		public override void LoadEdits() {
			IL.Terraria.Player.CarpetMovement += Player_CarpetMovement;
			IL.Terraria.Player.JumpMovement += Player_JumpMovement;
			IL.Terraria.Player.Update += Player_Update;
			On.Terraria.Player.RefreshDoubleJumps += Player_RefreshDoubleJumps;
			On.Terraria.Player.CancelAllJumpVisualEffects += Player_CancelAllJumpVisualEffects;
			On.Terraria.Player.DoubleJumpVisuals += Player_DoubleJumpVisuals;
		}

		public override void UnloadEdits() {
			IL.Terraria.Player.CarpetMovement -= Player_CarpetMovement;
			IL.Terraria.Player.JumpMovement -= Player_JumpMovement;
			IL.Terraria.Player.Update -= Player_Update;
			On.Terraria.Player.RefreshDoubleJumps -= Player_RefreshDoubleJumps;
			On.Terraria.Player.CancelAllJumpVisualEffects -= Player_CancelAllJumpVisualEffects;
			On.Terraria.Player.DoubleJumpVisuals -= Player_DoubleJumpVisuals;
		}

		private static void Player_CarpetMovement(ILContext il) {
			ILHelper.CommonPatchingWrapper(il, CoreMod.Instance, CarpetMovementPatch);
		}

		private static void Player_JumpMovement(ILContext il) {
			ILHelper.CommonPatchingWrapper(il, CoreMod.Instance, JumpMovementPatch);
		}

		private static void Player_Update(ILContext il) {
			ILHelper.CommonPatchingWrapper(il, CoreMod.Instance, UpdatePatch);
		}

		private static void Player_RefreshDoubleJumps(On.Terraria.Player.orig_RefreshDoubleJumps orig, Player self) {
			orig(self);

			var mp = self.GetModPlayer<AccessoriesPlayer>();

			if (mp.oronitusJump.hasJumpOption)
				mp.oronitusJump.canJumpAgain = true;
		}

		private void Player_CancelAllJumpVisualEffects(On.Terraria.Player.orig_CancelAllJumpVisualEffects orig, Player self) {
			orig(self);

			self.GetModPlayer<AccessoriesPlayer>().oronitusJump.isPerformingJump = false;
		}

		private static void Player_DoubleJumpVisuals(On.Terraria.Player.orig_DoubleJumpVisuals orig, Player self) {
			orig(self);

			AccessoriesPlayer mp = self.GetModPlayer<AccessoriesPlayer>();
			if (mp.oronitusJump.isPerformingJump && mp.oronitusJump.hasJumpOption && !mp.oronitusJump.canJumpAgain && ((self.gravDir == 1f && self.velocity.Y < 0f) || (self.gravDir == -1f && self.velocity.Y > 0f))) {
				if (self.height >= 32) {
					for (int i = 0; i < 7; i++) {
						Dust dust = Dust.NewDustDirect(self.position + new Vector2(0, 16), self.width, self.height - 16, DustID.GreenTorch, Scale: 2);
						dust.fadeIn = 1f;
						dust.noGravity = Main.rand.NextFloat() <= 0.6667f;
					}
				}
			}
		}

		private static bool CarpetMovementPatch(ILCursor c, ref string badReturnReason) {
			FieldInfo Player_canJumpAgain_Unicorn = typeof(Player).GetField("canJumpAgain_Unicorn", BindingFlags.Public | BindingFlags.Instance);

			// First an only patch:  adding a check for the Jewel of Oronitus jump again flag
			ILLabel afterBlock = null;
			if (!c.TryGotoNext(MoveType.After, i => i.MatchLdarg(0),
				i => i.MatchLdfld(Player_canJumpAgain_Unicorn),
				i => i.MatchBrtrue(out afterBlock))) {
				badReturnReason = "Could not find canJumpAgain_Unicorn instruction sequence";
				return false;
			}

			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Func<Player, bool>>(static player => player.GetModPlayer<AccessoriesPlayer>().oronitusJump.canJumpAgain);
			c.Emit(OpCodes.Brtrue, afterBlock);

			return true;
		}

		private static bool JumpMovementPatch(ILCursor c, ref string badReturnReason) {
			//This is where the actual jump happens
			FieldInfo Player_canJumpAgain_Unicorn = typeof(Player).GetField("canJumpAgain_Unicorn", BindingFlags.Public | BindingFlags.Instance);
			FieldInfo Player_canJumpAgain_Blizzard = typeof(Player).GetField("canJumpAgain_Blizzard", BindingFlags.Public | BindingFlags.Instance);
			FieldInfo Player_autoJump = typeof(Player).GetField("autoJump", BindingFlags.Public | BindingFlags.Instance);
			FieldInfo Player_justJumped = typeof(Player).GetField("justJumped", BindingFlags.Public | BindingFlags.Instance);
			FieldInfo Vector2_X = typeof(Vector2).GetField("X", BindingFlags.Public | BindingFlags.Instance);

			// First edit:  inserting the 'canJumpAgain' check for the custom jump
			ILLabel afterWetCheck = null;
			if (!c.TryGotoNext(MoveType.After, i => i.MatchBrtrue(out _),
				i => i.MatchLdarg(0),
				i => i.MatchLdfld(Player_canJumpAgain_Unicorn),
				i => i.MatchBrtrue(out afterWetCheck))) {
				badReturnReason = "Could not find canJumpAgain_Unicorn instruction sequence";
				return false;
			}

			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Func<Player, bool>>(static player => player.GetModPlayer<AccessoriesPlayer>().oronitusJump.canJumpAgain);
			c.Emit(OpCodes.Brtrue_S, afterWetCheck);

			// Second edit:  resetting the 'flag' field for the custom jump
			if (!c.TryGotoNext(MoveType.After, i => i.MatchLdcI4(0),
				i => i.MatchStloc(out _),
				i => i.MatchLdcI4(0),
				i => i.MatchStloc(out _),
				i => i.MatchLdloc(out _),
				i => i.MatchBrtrue(out _))) {
				badReturnReason = "Could not find flag resetting instruction sequence";
				return false;
			}

			c.Index -= 2;

			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Action<Player>>(static player => player.GetModPlayer<AccessoriesPlayer>().oronitusJump.flag = false);

			// Third edit:  inserting the 'flag' field check
			ILLabel branchToFartJumpCheck = null, branchAfterChecks = null;
			if (!c.TryGotoNext(MoveType.After, i => i.MatchLdarg(0),
				i => i.MatchLdfld(Player_canJumpAgain_Blizzard),
				i => i.MatchBrfalse(out branchToFartJumpCheck),
				i => i.MatchLdcI4(1),
				i => i.MatchStloc(out _),
				i => i.MatchLdarg(0),
				i => i.MatchLdcI4(0),
				i => i.MatchStfld(Player_canJumpAgain_Blizzard),
				i => i.MatchBr(out branchAfterChecks))) {
				badReturnReason = "Could not find canJumpAgain_Blizzard instruction sequence";
				return false;
			}

			c.Emit(OpCodes.Ldarg_0);
			c.Emit(OpCodes.Stsfld, editPlayer_Field);

			c.EmitElseIfBlock(out _, branchAfterChecks, static () => editPlayer.GetModPlayer<AccessoriesPlayer>().oronitusJump.canJumpAgain,
				action: static cursor => {
					cursor.Emit(OpCodes.Ldarg_0);
					cursor.EmitDelegate<Action<Player>>(static player => {
						player.GetModPlayer<AccessoriesPlayer>().oronitusJump.flag = true;
						player.GetModPlayer<AccessoriesPlayer>().oronitusJump.canJumpAgain = false;
					});
				});

			// Forth edit removed due to RefreshDoubleJumps being better

			// Fifth edit:  signifying that the effect code should run and actually applying the double jump
			ILLabel nextBranch = null, blockEnd = null;
			if (!c.TryGotoNext(MoveType.After, i => i.MatchLdloc(out _),
				i => i.MatchOr(),
				i => i.MatchBrfalse(out nextBranch),
				i => i.MatchLdarg(0))) {
				badReturnReason = "Could not find grounded player check instruction sequence";
				return false;
			}

			if (!c.TryGotoNext(MoveType.After, i => i.MatchMul(),
				i => i.MatchConvR4(),
				i => i.MatchStfld(Vector2_X),
				i => i.MatchBr(out blockEnd))) {
				badReturnReason = "Could not find X velocity assignment instruction sequence";
				return false;
			}

			c.Emit(OpCodes.Ldarg_0);
			c.Emit(OpCodes.Stsfld, editPlayer_Field);

			c.EmitElseIfBlock(out _, blockEnd, static () => editPlayer.GetModPlayer<AccessoriesPlayer>().oronitusJump.flag,
				action: static cursor => {
					cursor.Emit(OpCodes.Ldarg_0);
					cursor.EmitDelegate<Action<Player>>(static player => {
						player.GetModPlayer<AccessoriesPlayer>().oronitusJump.isPerformingJump = true;
						int height2 = player.height;
						float num3 = player.gravDir;
						SoundEngine.PlaySound(SoundID.DoubleJump, player.position);
						player.velocity.Y = -Player.jumpSpeed * player.gravDir;
						// Blizzard double jump uses a factor of 1.5f
						player.jump = (int)(Player.jumpHeight * 1.6f);
					});
				},
				targetsToUpdate: nextBranch);

			return true;
		}

		private static bool UpdatePatch(ILCursor c, ref string badReturnReason) {
			// This is where 'AccessoriesPlayer.oronitusJump.canJumpAgain' is set or cleared
			FieldInfo Player_canJumpAgain_Unicorn = typeof(Player).GetField("canJumpAgain_Unicorn", BindingFlags.Public | BindingFlags.Instance);
			FieldInfo Player_maxRunSpeed = typeof(Player).GetField("maxRunSpeed", BindingFlags.Public | BindingFlags.Instance);
			FieldInfo Player_sandStorm = typeof(Player).GetField("sandStorm", BindingFlags.Public | BindingFlags.Instance);
			FieldInfo Entity_position = typeof(Entity).GetField("position", BindingFlags.Public | BindingFlags.Instance);

			// First edit:  Some mounts block double jumps altogether. This should happen for this accessory too
			if (!c.TryGotoNext(MoveType.After, i => i.MatchLdarg(0),
				i => i.MatchLdcI4(0),
				i => i.MatchStfld(Player_canJumpAgain_Unicorn))) {
				badReturnReason = "Could not find canJumpAgain_Unicorn clearing instruction sequence";
				return false;
			}

			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Action<Player>>(static player => player.GetModPlayer<AccessoriesPlayer>().oronitusJump.canJumpAgain = false);

			// Second edit removed due to RefreshDoubleJumps being better

			// Third edit removed due to CancelAllJumpVisualEffects being better

			// Fourth edit: making the player move faster during the custom double jump
			ILLabel nextBlock = null;
			if (!c.TryGotoNext(MoveType.After, i => i.MatchLdarg(0),
				i => i.MatchLdfld(Player_sandStorm),
				i => i.MatchBrfalse(out nextBlock),
				i => i.MatchLdarg(0))) {
				badReturnReason = "Could not find sandStorm read instruction sequence";
				return false;
			}

			int branchIndex = c.Index - 2;
			if (!c.TryGotoNext(MoveType.After, i => i.MatchLdarg(0),
				i => i.MatchLdfld(Player_maxRunSpeed),
				i => i.MatchLdcR4(2),
				i => i.MatchMul(),
				i => i.MatchStfld(Player_maxRunSpeed))) {
				badReturnReason = "Could not find maxRunSpeed modification instruction sequence";
				return false;
			}

			c.Emit(OpCodes.Ldarg_0);
			c.Emit(OpCodes.Stsfld, editPlayer_Field);

			c.EmitIfBlock(out _, IsPlayerPerformingOronitusJump,
				action: static cursor => {
					cursor.Emit(OpCodes.Ldarg_0);
					cursor.EmitDelegate<Action<Player>>(static player => {
						player.runAcceleration *= 2.5f;
						player.maxRunSpeed *= 2f;
					});
				},
				targetsToUpdate: nextBlock);

			// Fifth edit removed due to CancelAllJumpVisualEffects being better

			// Sixth edit removed due to CancelAllJumpVisualEffects being better

			return true;
		}

		private static bool IsPlayerPerformingOronitusJump() {
			var jump = editPlayer.GetModPlayer<AccessoriesPlayer>().oronitusJump;

			return jump.isPerformingJump && jump.hasJumpOption;
		}
	}
}
