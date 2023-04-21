using CosmivengeonMod.API.Edits.Detours.Desomode;
using CosmivengeonMod.Buffs.Harmful;
using CosmivengeonMod.Worlds;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Players{
	public class DesomodePlayer : ModPlayer{
		public int GrabCounter = -1;

		private bool prevHoneyed = false;

		public override void SetControls(){
			if(!WorldEvents.desoMode)
				return;

			//Player is grabbed by an EoW worm head and isn't underground
			if(DetourNPCHelper.EoW_GrabbingNPC != -1 && DetourNPCHelper.EoW_GrabbedPlayer == Player.whoAmI && !Main.npc[DetourNPCHelper.EoW_GrabbingNPC].Helper().Flag){
				if(Player.controlLeft && Player.releaseLeft)
					GrabCounter--;
				if(Player.controlRight && Player.releaseRight)
					GrabCounter--;
				if(Player.controlUp && Player.releaseUp)
					GrabCounter--;
				if(Player.controlDown && Player.releaseDown)
					GrabCounter--;
				if(Player.controlJump && Player.releaseJump)
					GrabCounter--;

				//No jumping, mounting or grappling allowed!
				Player.controlJump = false;
				Player.releaseJump = false;
				Player.controlMount = false;
				Player.releaseMount = false;
				Player.controlHook = false;
				Player.releaseHook = false;

				//No dashing!
				Player.controlRight = false;
				Player.releaseRight = false;
				Player.controlLeft = false;
				Player.releaseLeft = false;

				if(GrabCounter <= 0)
					DetourNPCHelper.EoW_ResetGrab(Main.npc[DetourNPCHelper.EoW_GrabbingNPC], Player);
			}
		}

		public override void PreUpdateMovement(){
			if(!WorldEvents.desoMode)
				return;

			//PreUpdateMovement() is called a bit after the tongued velocity is applied... perfect!
			if(Main.wof >= 0){
				//Handle player being too far in front of the WoF
				float dist = Math.Abs(Player.Center.X - Main.npc[Main.wof].Center.X);
				if(Player.position.Y / 16 >= Main.maxTilesY - 200 && dist > 100 * 16){
					Player.tongued = true;

					Player.velocity = Player.DirectionTo(Main.npc[Main.wof].Center);
				}

				bool licked = Player.tongued || Player.HasBuff(BuffID.TheTongue);

				if(licked){
					//Player velocity is forced to be in front of the mouth... let's speed that up
					Vector2 dir = Vector2.Normalize(Player.velocity);
					float length = Player.velocity.Length();

					float extra;
					if(dist > 150 * 16)
						extra = 30;
					else if(dist > 100 * 16)
						extra = 20;
					else if(dist > 50 * 16)
						extra = 15;
					else
						extra = 10;

					float desiredVel = Math.Abs(Main.npc[Main.wof].velocity.X) + extra;
					if(length < desiredVel)
						Player.velocity = dir * desiredVel;
				}
			}

			if(Player.tongued || DetourNPCHelper.EoW_GrabbingNPC == -1)
				return;

			Player.velocity.X = 0;
			Player.velocity.Y = 1;

			NPC npc = Main.npc[DetourNPCHelper.EoW_GrabbingNPC];
			Vector2 offset = Vector2.UnitX.RotatedBy(npc.rotation - MathHelper.PiOver2) * 8;
			Player.Center = npc.Center + offset;
		}

		public override void PostUpdateRunSpeeds(){
			if(!WorldEvents.desoMode)
				return;

			if(Player.HasBuff(BuffID.Slimed)){
				Player.maxRunSpeed *= 0.85f;
				Player.accRunSpeed *= 0.85f;
				Player.runAcceleration *= 0.85f;
			}
			if(Player.HasBuff(ModContent.BuffType<Sticky>())){
				Player.maxRunSpeed *= 0.8f;
				Player.accRunSpeed *= 0.8f;
				Player.runAcceleration *= 0.7f;
			}
		}

		public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright){
			if(!WorldEvents.desoMode)
				return;

			if(Player.HasBuff(ModContent.BuffType<Sticky>())){
				Color color = new Color(r, g, b, a);
				color = Color.Lerp(color, Color.Yellow, 0.2f);

				r = color.R / 255f;
				g = color.G / 255f;
				b = color.B / 255f;
				a = color.A / 255f;

				if(Main.rand.NextFloat() < 0.2f){
					Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.t_Honey);
					dust.velocity = new Vector2(0, 1);
				}
			}
		}

		public override void ModifyHitByNPC(NPC npc, ref int damage, ref bool crit){
			if(!WorldEvents.desoMode)
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

			if(Player.HasBuff(BuffID.Slimed) && slimeTypes.Contains(npc.type)){
				damage = (int)Math.Max(damage * 1.35f, damage + 1);
			}
		}

		public override void PreUpdateBuffs(){
			if(!WorldEvents.desoMode)
				return;

			if(NPC.AnyNPCs(NPCID.EyeofCthulhu))
				Player.AddBuff(BuffID.Darkness, 601);

			if(!Player.honeyWet && prevHoneyed)
				Player.AddBuff(ModContent.BuffType<Sticky>(), 5 * 60);
		}

		public override void PostUpdate(){
			prevHoneyed = Player.honeyWet;
		}
	}
}
