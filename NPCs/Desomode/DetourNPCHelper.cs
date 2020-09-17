using CosmivengeonMod.Detours;
using CosmivengeonMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.NPCs.Desomode{
	public class DetourNPCHelper : GlobalNPC{
		public override bool InstancePerEntity => true;

		//None of these fields are descriptive.  Too bad!

		/// <summary>
		/// A generic timer.
		/// <list type="bullet">
		/// <item>KS - spike attack timer</item>
		/// <item>EoC - timer for spawning minions in Phase 2</item>
		/// <item>EoW - timer for head segements; used to make the head "bite" the player it's grabbed</item>
		/// <item>BoC - timer for the mine psychic attacks</item>
		/// </list>
		/// </summary>
		public int Timer;
		/// <summary>
		/// A generic timer.
		/// <list type="bullet">
		/// <item>KS - timer max for how long the small hops last for</item>
		/// </list>
		/// </summary>
		public int Timer2;
		/// <summary>
		/// A generic timer.
		/// <list type="bullet">
		/// <item>KS - timer for the small hops</item>
		/// <item>BoC - timer for the lightning psychic attacks</item>
		/// </list>
		/// </summary>
		public int Timer3;
		/// <summary>
		/// A generic timer.
		/// </summary>
		public int Timer4;

		/// <summary>
		/// A generic flag.
		/// <list type="bullet">
		/// <item>EoC - whether the boss has done the immediate transition to Phase 2</item>
		/// <item>EoW - if the NPC is underground (only set for head segments)</item>
		/// </list>
		/// </summary>
		public bool Flag;
		/// <summary>
		/// A generic flag.
		/// <list type="bullet">
		/// <item>EoC - whether the boss has done the transition to Phase 3</item>
		/// </list>
		/// </summary>
		public bool Flag2;
		/// <summary>
		/// A generic flag.
		/// <list type="bullet">
		/// <item>EoC - whether the boss should spawn the blood walls</item>
		/// </list>
		/// </summary>
		public bool Flag3;

		//Boss-specific fields
		public float EoC_PhaseTransitionVelocityLength;
		public int EoC_TargetHeight;
		public bool EoC_UsingShader;
		public int EoC_PlayerTargetMovementDirection;
		public int[] EoC_PlayerTargetMovementTimers = new int[2];
		public Vector2 EoC_FadePositionOffset;
		public int EoC_TimerTarget;
		public static int EoC_FirstBloodWallNPC = -1;
		
		public int EoW_SegmentType;
		public static int EoW_GrabbingNPC = -1;
		private static int EoW_grabbingNPCPrevDamage = 0;
		public static int EoW_GrabbedPlayer = -1;

		public static void EoW_SetGrab(NPC npc, Player player){
			if(EoW_GrabbingNPC != -1)
				return;

			EoW_GrabbingNPC = npc.whoAmI;
			EoW_GrabbedPlayer = player.whoAmI;
			EoW_grabbingNPCPrevDamage = npc.damage;
			npc.damage = 0;
			npc.GetGlobalNPC<CosmivengeonGlobalNPC>().endurance -= 0.1f;
			npc.Desomode().EoW_GrabTarget = player.Center;
			player.GetModPlayer<DesomodePlayer>().GrabCounter = 20;

			//Chomps every 2.5 seconds
			npc.Helper().Timer = 150;

			if(player.mount != null)
				player.mount._active = false;

			if(Main.netMode != NetmodeID.MultiplayerClient)
				return;

			NetMessage.SendData(MessageID.SyncNPC, number: npc.whoAmI);
			NetMessage.SendData(MessageID.SyncPlayer, number: player.whoAmI);

			ModPacket packet = ModContent.GetInstance<CosmivengeonMod>().GetPacket();
			packet.Write((byte)CosmivengeonModMessageType.SyncEoWGrab);
			packet.Write(EoW_GrabbingNPC);
			packet.Write(EoW_GrabbedPlayer);
			packet.Send();
		}

		public static void EoW_ResetGrab(NPC npc, Player player){
			if(Main.myPlayer != EoW_GrabbedPlayer || EoW_GrabbingNPC != npc.whoAmI || EoW_GrabbedPlayer != player.whoAmI)
				return;

			EoW_GrabbingNPC = -1;
			npc.damage = EoW_grabbingNPCPrevDamage;
			npc.GetGlobalNPC<CosmivengeonGlobalNPC>().endurance += 0.1f;
			npc.Helper().Timer = 150;
			npc.Desomode().EoW_GrabTarget = null;
			player.GetModPlayer<DesomodePlayer>().GrabCounter = -1;
			player.immune = true;
			player.immuneTime = 60;
			player.velocity.X = npc.velocity.X >= 0 ? 8 : -8;
			player.velocity.Y = -15;

			if(Main.netMode != NetmodeID.MultiplayerClient)
				return;

			NetMessage.SendData(MessageID.SyncNPC, number: npc.whoAmI);
			NetMessage.SendData(MessageID.SyncPlayer, number: player.whoAmI);

			ModPacket packet = ModContent.GetInstance<CosmivengeonMod>().GetPacket();
			packet.Write((byte)CosmivengeonModMessageType.SyncEoWGrab);
			packet.Write(EoW_GrabbingNPC);
			packet.Write(EoW_GrabbedPlayer);
			packet.Send();
		}

		public static void EoW_CheckGrabBite(NPC npc){
			if(EoW_GrabbingNPC == npc.whoAmI){
				//If the NPC is underground, don't try to bite the player
				bool underground = npc.Helper().Flag;

				if(!underground)
					npc.Helper().Timer--;
				else
					npc.Helper().Timer = 150;

				Player grabbed = Main.player[EoW_GrabbedPlayer];

				if(npc.Helper().Timer == 0){
					npc.damage = EoW_grabbingNPCPrevDamage;

					grabbed.immune = false;
					grabbed.immuneTime = 0;
				}else if(npc.Helper().Timer == -1){
					npc.damage = 0;
					npc.Helper().Timer = 150;

					grabbed.immune = true;
					grabbed.immuneTime = 2;
				}else{
					grabbed.immune = true;
					grabbed.immuneTime = 2;
				}
			}
		}
	}
}
