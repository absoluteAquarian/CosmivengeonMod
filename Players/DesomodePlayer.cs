using CosmivengeonMod.Buffs;
using CosmivengeonMod.Detours;
using CosmivengeonMod.NPCs.Desomode;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Players{
	public class DesomodePlayer : ModPlayer{
		public int GrabCounter = -1;

		private bool prevHoneyed = false;

		public override void SetControls(){
			if(!CosmivengeonWorld.desoMode)
				return;

			//Player is grabbed by an EoW worm head and isn't underground
			if(DetourNPCHelper.EoW_GrabbingNPC != -1 && DetourNPCHelper.EoW_GrabbedPlayer == player.whoAmI && !Main.npc[DetourNPCHelper.EoW_GrabbingNPC].Helper().Flag){
				if(player.controlLeft && player.releaseLeft)
					GrabCounter--;
				if(player.controlRight && player.releaseRight)
					GrabCounter--;
				if(player.controlUp && player.releaseUp)
					GrabCounter--;
				if(player.controlDown && player.releaseDown)
					GrabCounter--;
				if(player.controlJump && player.releaseJump)
					GrabCounter--;

				//No jumping, mounting or grappling allowed!
				player.controlJump = false;
				player.releaseJump = false;
				player.controlMount = false;
				player.releaseMount = false;
				player.controlHook = false;
				player.releaseHook = false;

				//No dashing!
				player.controlRight = false;
				player.releaseRight = false;
				player.controlLeft = false;
				player.releaseLeft = false;

				if(GrabCounter <= 0)
					DetourNPCHelper.EoW_ResetGrab(Main.npc[DetourNPCHelper.EoW_GrabbingNPC], player);
			}
		}

		public override void PreUpdateMovement(){
			if(!CosmivengeonWorld.desoMode)
				return;

			if(DetourNPCHelper.EoW_GrabbingNPC == -1)
				return;

			player.velocity.X = 0;
			player.velocity.Y = 1;

			NPC npc = Main.npc[DetourNPCHelper.EoW_GrabbingNPC];
			Vector2 offset = Vector2.UnitX.RotatedBy(npc.rotation - MathHelper.PiOver2) * 8;
			player.Center = npc.Center + offset;
		}

		public override void PostUpdateRunSpeeds(){
			if(!CosmivengeonWorld.desoMode)
				return;

			if(player.HasBuff(BuffID.Slimed)){
				player.maxRunSpeed *= 0.85f;
				player.accRunSpeed *= 0.85f;
				player.runAcceleration *= 0.85f;
			}
			if(player.HasBuff(ModContent.BuffType<Sticky>())){
				player.maxRunSpeed *= 0.8f;
				player.accRunSpeed *= 0.8f;
				player.runAcceleration *= 0.7f;
			}
		}

		public override void DrawEffects(PlayerDrawInfo drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright){
			if(!CosmivengeonWorld.desoMode)
				return;

			if(player.HasBuff(ModContent.BuffType<Sticky>())){
				Color color = new Color(r, g, b, a);
				color = Color.Lerp(color, Color.Yellow, 0.2f);

				r = color.R / 255f;
				g = color.G / 255f;
				b = color.B / 255f;
				a = color.A / 255f;

				if(Main.rand.NextFloat() < 0.2f){
					Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, DustID.t_Honey);
					dust.velocity = new Vector2(0, 1);
				}
			}
		}

		public override void ModifyHitByNPC(NPC npc, ref int damage, ref bool crit){
			if(!CosmivengeonWorld.desoMode)
				return;

			int[] slimeTypes = new int[]{
				NPCID.ArmedZombieSlimed,
				NPCID.BabySlime,
				NPCID.BigSlimedZombie,
				NPCID.BlackSlime,
				NPCID.BlueSlime,
				NPCID.CorruptSlime,
				NPCID.DungeonSlime,
				NPCID.GreenSlime,
				NPCID.IceSlime,
				NPCID.IlluminantSlime,
				NPCID.JungleSlime,
				NPCID.KingSlime,
				NPCID.LavaSlime,
				NPCID.MotherSlime,
				NPCID.PurpleSlime,
				NPCID.RainbowSlime,
				NPCID.RedSlime,
				NPCID.SandSlime,
				NPCID.SlimedZombie,
				NPCID.Slimeling,
				NPCID.SlimeMasked,
				NPCID.Slimer,
				NPCID.Slimer2,
				NPCID.SlimeRibbonGreen,
				NPCID.SlimeRibbonRed,
				NPCID.SlimeRibbonWhite,
				NPCID.SlimeRibbonYellow,
				NPCID.SlimeSpiked,
				NPCID.SmallSlimedZombie,
				NPCID.SpikedIceSlime,
				NPCID.SpikedJungleSlime,
				NPCID.UmbrellaSlime,
				NPCID.YellowSlime,
				NPCID.Pinky
			};

			if(player.HasBuff(BuffID.Slimed) && slimeTypes.Contains(npc.type)){
				damage = (int)Math.Max(damage * 1.35f, damage + 1);
			}
		}

		public override void PreUpdateBuffs(){
			if(!CosmivengeonWorld.desoMode)
				return;

			if(NPC.AnyNPCs(NPCID.EyeofCthulhu))
				player.AddBuff(BuffID.Darkness, 601);

			if(!player.honeyWet && prevHoneyed)
				player.AddBuff(ModContent.BuffType<Sticky>(), 5 * 60);
		}

		public override void PostUpdate(){
			prevHoneyed = player.honeyWet;
		}
	}
}
