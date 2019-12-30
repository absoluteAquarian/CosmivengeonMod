using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using System.IO;
using System.Reflection;

namespace CosmivengeonMod.Items.Draek{
	[AutoloadEquip(EquipType.Neck)]
	public class JewelOfOronitus : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Jewel of Oronitus");
			//NOTE:  "\nGrants an earth-blessed rock jump" needs to be added after "Fall speed increased by 20%" when the IL code is implemented and works.
			Tooltip.SetDefault("Damage dealt and damage reduction increased by 5%" +
				"\nMovement speed increased by 10%" +
				"\nFall speed increased by 20%" +
				"\nAn ancient artifact, last donned by the rock serpent Draek." +
				"\nIts original master, Oronitus, was seemingly lost to time many years ago...");

			Main.RegisterItemAnimation(item.type, new JewelOfOronitusAnimation());
		}

#region IL Code
//Ignore any "unreachable code" and "unused members" warnings
#pragma warning disable CS0162
#pragma warning disable IDE0051

		public override bool Autoload(ref string name){
//			IL.Terraria.Player.JumpMovement += HookJumpMovement;
//			IL.Terraria.Player.DoubleJumpVisuals += HookDoubleJumpVisuals;
			
			return base.Autoload(ref name);
		}

		private void HookDoubleJumpVisuals(ILContext il){
			//For now, I am going to avoid messing with this code so that i can finish everything else.
			//If this code is shown to anyone, just ignore the thrown exception below
			throw new NotImplementedException("This hook has not been implemented yet.  If you got this error, please contact the developers.");

			ILCursor c = new ILCursor(il);

			LogDoubleJumpVisualsBefore(c);

			//Move the cursor to before the "ret" instruction
			c.GotoNext(MoveType.AfterLabel, i => i.MatchRet());
			c.Index--;

			ILLabel afterIfBlockLabel = c.DefineLabel();

			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Func<Player,bool>>(player => player.GetModPlayer<CosmivengeonPlayer>().dJumpEffect_JewelOfOronitus && player.GetModPlayer<CosmivengeonPlayer>().doubleJump_JewelOfOronitus && !player.GetModPlayer<CosmivengeonPlayer>().jumpAgain_JewelOfOronitus && ((player.gravDir == 1f && player.velocity.Y < 1f) || (player.gravDir == -1f && player.velocity.Y > 1f)));
			c.Emit(OpCodes.Brfalse_S, afterIfBlockLabel);
			c.Emit(OpCodes.Ldarg_0);
			//near-copy of Blizzard in a Bottle visual effect
			c.EmitDelegate<Action<Player>>(player => {
				int dustHeightOffset = player.height - 6;
				if(player.gravDir == -1f){
					dustHeightOffset = 6;
				}
				for(int k = 0; k < 2; k++){
					int num13 = Dust.NewDust(new Vector2(player.position.X, player.position.Y + dustHeightOffset), player.width, 12, 74, player.velocity.X * 0.3f, player.velocity.Y * 0.3f, 0, default, 1f);
					Main.dust[num13].velocity *= 0.1f;
					if(k == 0)
						Main.dust[num13].velocity += player.velocity * 0.03f;
					else
						Main.dust[num13].velocity -= player.velocity * 0.03f;
					Main.dust[num13].velocity -= player.velocity * 0.1f;
					Main.dust[num13].noGravity = true;
					Main.dust[num13].noLight = true;
				}
				for(int l = 0; l < 3; l++){
					int num14 = Dust.NewDust(new Vector2(player.position.X, player.position.Y + dustHeightOffset), player.width, 12, 74, player.velocity.X * 0.3f, player.velocity.Y * 0.3f, 0, default, 1f);
					Main.dust[num14].fadeIn = 1.5f;
					Main.dust[num14].velocity *= 0.6f;
					Main.dust[num14].velocity += player.velocity * 0.8f;
					Main.dust[num14].noGravity = true;
					Main.dust[num14].noLight = true;
				}
				for(int m = 0; m < 3; m++){
					int num15 = Dust.NewDust(new Vector2(player.position.X, player.position.Y + dustHeightOffset), player.width, 12, 74, player.velocity.X * 0.3f, player.velocity.Y * 0.3f, 0, default, 1f);
					Main.dust[num15].fadeIn = 1.5f;
					Main.dust[num15].velocity *= 0.6f;
					Main.dust[num15].velocity -= player.velocity * 0.8f;
					Main.dust[num15].noGravity = true;
					Main.dust[num15].noLight = true;
				}
			});
			c.MarkLabel(afterIfBlockLabel);

			LogDoubleJumpVisualsAfter(c);
		}

		private void HookJumpMovement(ILContext il){
			//For now, I am going to avoid messing with this code so that i can finish everything else.
			//If this code is shown to anyone, just ignore the thrown exception below
			throw new NotImplementedException("This hook has not been implemented yet.  If you got this error, please contact the developers.");

			FileStream file = File.OpenWrite(@"C:/Users/jruss/Desktop/HookJumpMovement.log");
			using(StreamWriter writer = new StreamWriter(file)){
				int prevIndex;
				int section = 1;

				ILCursor c = new ILCursor(il);

				LogJumpMovementBefore(c);

				BeginLogInstructions(writer, section++);

				/*		else if ((((sliding || velocity.Y == 0f) | flag) || jumpAgainCloud || jumpAgainSandstorm || jumpAgainBlizzard
				 *		|| player.GetModPlayer<CosmivengeonPlayer>().jumpAgain_JewelOfOronitus											<== inserting this piece
				 *		|| jumpAgainFart
				 */

				if(!c.TryGotoNext(MoveType.AfterLabel, i => i.MatchLdfld(typeof(Terraria.Player), nameof(Terraria.Player.jumpAgainBlizzard))))
					return;

				prevIndex = c.Index;

				c.Index += 2;

				ILLabel orChainLabel = c.DefineLabel();

				c.Emit(OpCodes.Ldarg_0);
				c.EmitDelegate<Func<Player, bool>>(player => player.GetModPlayer<CosmivengeonPlayer>().jumpAgain_JewelOfOronitus);
				c.Emit(OpCodes.Brtrue_S, orChainLabel);
				c.Index += 2;
				c.MarkLabel(orChainLabel);

				EndLogInstructions(writer, c, prevIndex);

				/*		bool flag7 = false;
				 *		player.GetModPlayer<CosmivengeonPlayer>().flag_JewelOfOronitus = false;		<== inserting this line
				 *		if (!flag)
				 */

				BeginLogInstructions(writer, section++);

				if(!c.TryGotoNext(MoveType.AfterLabel, i => i.MatchStloc(13)))	//stloc.s flag7
					return;														//This instruction is the first one of this type and parameter,
																				// so we don't need to check its relative position
				prevIndex = c.Index;

				c.Emit(OpCodes.Ldarg_0);
				c.EmitDelegate<Action<Player>>(player => player.GetModPlayer<CosmivengeonPlayer>().flag_JewelOfOronitus = false);

				EndLogInstructions(writer, c, prevIndex);

				/*		}
				 *		else if(player.GetModPlayer<CosmivengeonPlayer>().jumpAgain_JewelOfOronitus){		<== inserting these lines
				 *			player.GetModPlayer<CosmivengeonPlayer>().flag_JewelOfOronitus = true;			<==
				 *			player.GetModPlayer<CosmivengeonPlayer>().jumpAgain_JewelOfOronitus = false;	<==
				 *		}																					<==
				 *		else if(jumpAgainFart)
				 */

				BeginLogInstructions(writer, section++);

				//Loop through found instances of the opcode we want until we find the right one
				bool correctOpCodeFound = false;
				while(!correctOpCodeFound){
					if(c.TryGotoNext(MoveType.AfterLabel, i => i.MatchLdarg(0))){
						Instruction ins = c.Prev.Previous.Previous.Previous.Previous;
						if(ins.MatchStloc(10))
							correctOpCodeFound = true;
					}else
						throw new Exception("Error finding IL code segment:  else if (jumpAgainFart) ...");
				}

				prevIndex = c.Index;
			
				ILLabel elseContinueLabel = c.DefineLabel();
				ILLabel ifBlockEndLabel = c.DefineLabel();
			
				c.Emit(OpCodes.Ldarg_0);
				c.EmitDelegate<Func<Player, bool>>(player => player.GetModPlayer<CosmivengeonPlayer>().jumpAgain_JewelOfOronitus);
				c.Emit(OpCodes.Brfalse_S, elseContinueLabel);
				c.Emit(OpCodes.Ldarg_0);
				c.EmitDelegate<Action<Player>>(player => player.GetModPlayer<CosmivengeonPlayer>().flag_JewelOfOronitus = true);
				c.Emit(OpCodes.Ldarg_0);
				c.EmitDelegate<Action<Player>>(player => player.GetModPlayer<CosmivengeonPlayer>().jumpAgain_JewelOfOronitus = false);
				c.Emit(OpCodes.Br_S, ifBlockEndLabel);
				c.MarkLabel(elseContinueLabel);

				c.TryGotoNext(MoveType.AfterLabel, i => i.MatchStfld(typeof(Terraria.Player), nameof(Terraria.Player.canRocket)));
				c.Index -= 2;	//move back two instructions
				c.MarkLabel(ifBlockEndLabel);

				EndLogInstructions(writer, c, prevIndex);

				/*		}
				 *		if ((velocity.Y == 0f || sliding || (autoJump && justJumped))						<==
				 *				&& player.GetModPlayer<CosmivengeonPlayer>().doubleJump_JewelOfOronitus)		<==	inserting these two lines
				 *			player.GetModPlayer<CosmivengeonPlayer>().jumpAgain_JewelOfOronitus					<==	(first line shown as two lines)
				 *		if ((velocity.Y == 0f || sliding || (autoJump && justJumped)) && doubleJumpFart)
				 */

				BeginLogInstructions(writer, section++);

				if(!c.TryGotoNext(i => i.MatchStfld(typeof(Terraria.Player), nameof(Terraria.Player.jumpAgainBlizzard))))
					return;

				prevIndex = c.Index;

				c.Index++;

				ILLabel ifBlockEndLabel_2 = c.DefineLabel();

				c.Emit(OpCodes.Ldarg_0);
				c.EmitDelegate<Func<Player, bool>>(player => player.velocity.Y == 0f || player.sliding || (player.autoJump && player.justJumped) && player.GetModPlayer<CosmivengeonPlayer>().doubleJump_JewelOfOronitus);
				c.Emit(OpCodes.Brfalse_S, ifBlockEndLabel_2);
				c.Emit(OpCodes.Ldarg_0);
				c.EmitDelegate<Action<Player>>(player => player.GetModPlayer<CosmivengeonPlayer>().jumpAgain_JewelOfOronitus = true);
				c.MarkLabel(ifBlockEndLabel_2);

				EndLogInstructions(writer, c, prevIndex);

				/*		}
				 *		else if(player.GetModPlayer<CosmivengeonPlayer>().flag_JewelOfOronitus){
				 *			player.GetModPlayer<CosmivengeonPlayer>().dJumpEffect_JewelOfOronitus = true;		<== inserting these 5 lines
				 *			Main.PlaySound(16, (int)player.position.X, (int)player.position.Y, 1, 1f, 0f);		<==
				 *			player.velocity.Y = (0f - Player.jumpSpeed) * player.gravDir;						<==
				 *			player.jump = (int)((double)Player.jumpHeight * 1.5);								<==
				 *		}																						<==
				 *		else if (flag6)
				 */

				BeginLogInstructions(writer, section++);

				if(!c.TryGotoNext(i => i.MatchLdloc(12)))	//ldloc.s flag6
					return;
				
				prevIndex = c.Index;

				ILLabel elseIfBlockEndLabel = c.DefineLabel();
				ILLabel flag6JumpLabel = c.DefineLabel();

				c.Emit(OpCodes.Ldarg_0);
				c.EmitDelegate<Func<Player, bool>>(player => player.GetModPlayer<CosmivengeonPlayer>().flag_JewelOfOronitus);
				c.Emit(OpCodes.Brfalse_S, flag6JumpLabel);
				c.Emit(OpCodes.Ldarg_0);
				c.EmitDelegate<Action<Player>>(player => {
					player.GetModPlayer<CosmivengeonPlayer>().dJumpEffect_JewelOfOronitus = true;
					Main.PlaySound(16, (int)player.position.X, (int)player.position.Y);
					player.velocity.Y = (0f - Player.jumpSpeed) * player.gravDir;
					player.jump = (int)(Player.jumpHeight * 1.5f);
				});
				c.Emit(OpCodes.Br, elseIfBlockEndLabel);
				c.MarkLabel(flag6JumpLabel);

				EndLogInstructions(writer, c, prevIndex);

				if(!c.TryGotoNext(i => i.MatchStfld(typeof(Terraria.Player), nameof(Terraria.Player.releaseJump))))
					return;

				c.Index -= 5;   //move back 5 instructions

				c.MarkLabel(elseIfBlockEndLabel);

				LogJumpMovementAfter(c);
			}
		}

#pragma warning restore CS0162
#pragma warning restore IDE0051

		private void LogJumpMovementBefore(ILCursor c){
			using(StreamWriter writer = new StreamWriter(@"C:/Users/jruss/Desktop/HookJumpBefore.log", false))
				CompleteLog(writer, c);
		}

		private void LogJumpMovementAfter(ILCursor c){
			using(StreamWriter writer = new StreamWriter(@"C:/Users/jruss/Desktop/HookJumpAfter.log", false))
				CompleteLog(writer, c);
		}

		private void LogDoubleJumpVisualsBefore(ILCursor c){
			using(StreamWriter writer = new StreamWriter(@"C:/Users/jruss/Desktop/HookDJVisualsBefore.log", false))
				CompleteLog(writer, c);
		}

		private void LogDoubleJumpVisualsAfter(ILCursor c){
			using(StreamWriter writer = new StreamWriter(@"C:/Users/jruss/Desktop/HookDJVisualsAfter.log", false))
				CompleteLog(writer, c);
		}

		private void CompleteLog(StreamWriter writer, ILCursor c){
			int index = c.Index;

			writer.WriteLine($"// ILCursor: {c.Method}");
			c.Index = 0;
			do{
				Instruction curIns = c.Instrs[c.Index];
				string operand = (curIns.Operand == null) ? "" : curIns.Operand.ToString();
				writer.WriteLine($"Index: {c.Index}, Instruction: {curIns.OpCode.Name} {operand}");
				c.Index++;
			}while(c.Index < c.Instrs.Count);

			c.Index = index;
		}

		private void BeginLogInstructions(StreamWriter writer, int section){
			if(section == 1)
				writer.WriteLine(DateTime.Now.ToString("'['HH':'mm':'ss']'"));
			writer.Write($"HookJumpMovement Section #{section}, ");
		}

		private void EndLogInstructions(StreamWriter writer, ILCursor c, int prevIndex){
			writer.WriteLine($"Indices {prevIndex}-{c.Index}\n");

			for(int i = prevIndex - 2; i <= c.Index + 2; i++) {
				Instruction curIns = c.Instrs[i];
				string operand = (curIns.Operand == null) ? "" : curIns.Operand.ToString();
				writer.WriteLine($"Index: {i}, Instruction: {curIns.OpCode.Name} {operand}");
			}
			writer.WriteLine();
		}
#endregion

		public override void SetDefaults(){
			item.width = 26;
			item.height = 34;
			item.accessory = true;
			item.value = Item.sellPrice(gold: 1, silver: 50);
			item.rare = ItemRarityID.Expert;
			item.expert = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual){
			player.allDamage += 0.05f;
			player.endurance += 0.05f;
			player.moveSpeed *= 1.1f;
			player.accRunSpeed *= 1.1f;
			player.maxFallSpeed *= 1.2f;
			player.gravity *= 1.2f;
			player.GetModPlayer<CosmivengeonPlayer>().doubleJump_JewelOfOronitus = true;
		}
	}

	public class JewelOfOronitusAnimation : DrawAnimation{
		public JewelOfOronitusAnimation(){
			FrameCount = 25;
			TicksPerFrame = 5;
		}

		public override Rectangle GetFrame(Texture2D texture)
			=> texture.Frame(1, FrameCount, 0, Frame);

		public override void Update(){
			if(++FrameCounter % TicksPerFrame == 0)
				Frame = ++Frame % FrameCount;
		}
	}
}
