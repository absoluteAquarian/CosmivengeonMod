using CosmivengeonMod.Enums;
using CosmivengeonMod.NPCs.Global;
using CosmivengeonMod.Players;
using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.API.Edits.Detours.Desomode {
	public class DetourNPCHelper : GlobalNPC {
		public override bool InstancePerEntity => true;

		//None of these fields are descriptive.  Too bad!

		/// <summary>
		/// A generic timer.
		/// <list type="bullet">
		/// <item>KS - spike attack timer</item>
		/// <item>EoC - timer for spawning minions in Phase 2</item>
		/// <item>EoW - timer for head segements; used to make the head "bite" the player it's grabbed</item>
		/// <item>BoC - timer for the mine psychic attacks</item>
		/// <item>QB - timer for the aura wobbling</item>
		/// <item>Skeletron Hand - timer for shooting bones</item>
		/// <item>WoF - timer for shooting demon scythes or imp fireballs in Phase 2</item>
		/// </list>
		/// </summary>
		public int Timer;
		/// <summary>
		/// A generic timer.
		/// <list type="bullet">
		/// <item>KS - timer max for how long the small hops last for</item>
		/// <item>QB - timer for the wait after getting enraged</item>
		/// <item>WoF - indicator for what attack the boss is doing during Phase 2</item>
		/// </list>
		/// </summary>
		public int Timer2;
		/// <summary>
		/// A generic timer.
		/// <list type="bullet">
		/// <item>KS - timer for the small hops</item>
		/// <item>BoC - timer for the lightning psychic attacks</item>
		/// <item>QB - timer for automatic charges</item>
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
		/// <item>QB - if the NPC should be drawn with a red hue and wobbling aura</item>
		/// <item>WoF - if the boss has reached Phase 2</item>
		/// </list>
		/// </summary>
		public bool Flag;
		/// <summary>
		/// A generic flag.
		/// <list type="bullet">
		/// <item>EoC - whether the boss has done the transition to Phase 3</item>
		/// <item>QB - if the first charge attack has happened</item>
		/// <item>WoF - mouth was forced within 100 tiles of the target player</item>
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

		public static void EoW_SetGrab(NPC npc, Player player) {
			if (EoW_GrabbingNPC != -1)
				return;

			EoW_GrabbingNPC = npc.whoAmI;
			EoW_GrabbedPlayer = player.whoAmI;
			EoW_grabbingNPCPrevDamage = npc.damage;
			npc.damage = 0;
			npc.GetGlobalNPC<StatsNPC>().endurance -= 0.1f;
			npc.Desomode().EoW_GrabTarget = player.Center;
			player.GetModPlayer<DesomodePlayer>().GrabCounter = 20;

			//Chomps every 2.5 seconds
			npc.Helper().Timer = 150;

			if (player.mount != null)
				player.mount._active = false;

			if (Main.netMode != NetmodeID.Server)
				return;

			NetMessage.SendData(MessageID.SyncNPC, number: npc.whoAmI);
			NetMessage.SendData(MessageID.SyncPlayer, number: player.whoAmI);

			ModPacket packet = CoreMod.Instance.GetPacket();
			packet.Write((byte)MessageType.SyncEoWGrab);
			packet.Write(EoW_GrabbingNPC);
			packet.Write(EoW_GrabbedPlayer);
			packet.Send();
		}

		public static void EoW_ResetGrab(NPC npc, Player player) {
			if (Main.myPlayer != EoW_GrabbedPlayer || EoW_GrabbingNPC != npc.whoAmI || EoW_GrabbedPlayer != player.whoAmI)
				return;

			EoW_GrabbingNPC = -1;
			npc.damage = EoW_grabbingNPCPrevDamage;
			npc.GetGlobalNPC<StatsNPC>().endurance += 0.1f;
			npc.Helper().Timer = 150;
			npc.Desomode().EoW_GrabTarget = null;
			player.GetModPlayer<DesomodePlayer>().GrabCounter = -1;
			player.immune = true;
			player.immuneTime = 60;
			player.velocity.X = npc.velocity.X >= 0 ? 8 : -8;
			player.velocity.Y = -15;

			if (Main.netMode != NetmodeID.Server)
				return;

			NetMessage.SendData(MessageID.SyncNPC, number: npc.whoAmI);
			NetMessage.SendData(MessageID.SyncPlayer, number: player.whoAmI);

			ModPacket packet = CoreMod.Instance.GetPacket();
			packet.Write((byte)MessageType.SyncEoWGrab);
			packet.Write(EoW_GrabbingNPC);
			packet.Write(EoW_GrabbedPlayer);
			packet.Send();
		}

		public static void EoW_CheckGrabBite(NPC npc) {
			if (EoW_GrabbingNPC == npc.whoAmI) {
				//If the NPC is underground, don't try to bite the player
				bool underground = npc.Helper().Flag;

				if (!underground)
					npc.Helper().Timer--;
				else
					npc.Helper().Timer = 150;

				Player grabbed = Main.player[EoW_GrabbedPlayer];

				if (npc.Helper().Timer == 0) {
					npc.damage = EoW_grabbingNPCPrevDamage;

					grabbed.immune = false;
					grabbed.immuneTime = 0;

					SendData(npc.whoAmI);
				} else if (npc.Helper().Timer == -1) {
					npc.damage = 0;
					npc.Helper().Timer = 150;

					grabbed.immune = true;
					grabbed.immuneTime = 2;
				} else {
					grabbed.immune = true;
					grabbed.immuneTime = 2;
				}
			}
		}

		public static void ReceiveData(BinaryReader reader) {
			NPC npc = Main.npc[reader.ReadInt32()];
			DetourNPCHelper helper = npc.Helper();
			DesomodeNPC desomode = npc.Desomode();
			StatsNPC stats = npc.GetGlobalNPC<StatsNPC>();
			BuffNPC buffs = npc.GetGlobalNPC<BuffNPC>();

			helper.Timer = reader.ReadInt32();
			helper.Timer2 = reader.ReadInt32();
			helper.Timer3 = reader.ReadInt32();
			helper.Timer4 = reader.ReadInt32();

			helper.Flag = reader.ReadBoolean();
			helper.Flag2 = reader.ReadBoolean();
			helper.Flag3 = reader.ReadBoolean();

			helper.EoC_PhaseTransitionVelocityLength = reader.ReadSingle();
			helper.EoC_TargetHeight = reader.ReadInt32();
			helper.EoC_UsingShader = reader.ReadBoolean();
			helper.EoC_PlayerTargetMovementDirection = reader.ReadInt32();
			helper.EoC_PlayerTargetMovementTimers[0] = reader.ReadInt32();
			helper.EoC_PlayerTargetMovementTimers[1] = reader.ReadInt32();
			helper.EoC_FadePositionOffset.X = reader.ReadSingle();
			helper.EoC_FadePositionOffset.Y = reader.ReadSingle();
			helper.EoC_TimerTarget = reader.ReadInt32();
			EoC_FirstBloodWallNPC = reader.ReadByte();

			helper.EoW_SegmentType = reader.ReadByte();
			EoW_GrabbingNPC = reader.ReadByte();
			EoW_grabbingNPCPrevDamage = reader.ReadInt32();
			EoW_GrabbedPlayer = reader.ReadByte();

			desomode.EoW_Spawn = reader.ReadBoolean();
			desomode.EoW_WormSegmentsCount = reader.ReadByte();
			float targetX = reader.ReadSingle();
			float targetY = reader.ReadSingle();
			if (targetX != -1000 && targetY != -1000)
				desomode.EoW_GrabTarget = new Vector2(targetX, targetY);
			else
				desomode.EoW_GrabTarget = null;

			desomode.QB_baseScale = reader.ReadSingle();
			desomode.QB_baseSize.X = reader.ReadSingle();
			desomode.QB_baseSize.Y = reader.ReadSingle();

			buffs.primordialWrath = reader.ReadBoolean();
			stats.baseEndurance = reader.ReadSingle();
			stats.endurance = reader.ReadSingle();
		}

		public static void SendData(int whoAmI) {
			if (Main.netMode != NetmodeID.Server)
				return;

			NPC npc = Main.npc[whoAmI];
			DetourNPCHelper helper = npc.Helper();
			DesomodeNPC desomode = npc.Desomode();
			StatsNPC stats = npc.GetGlobalNPC<StatsNPC>();
			BuffNPC buffs = npc.GetGlobalNPC<BuffNPC>();

			ModPacket packet = CoreMod.Instance.GetPacket();
			packet.Write((byte)MessageType.SyncGlobalNPCBossData);
			packet.Write(whoAmI);

			packet.Write(helper.Timer);
			packet.Write(helper.Timer2);
			packet.Write(helper.Timer3);
			packet.Write(helper.Timer4);

			packet.Write(helper.Flag);
			packet.Write(helper.Flag2);
			packet.Write(helper.Flag3);

			packet.Write(helper.EoC_PhaseTransitionVelocityLength);
			packet.Write(helper.EoC_TargetHeight);
			packet.Write(helper.EoC_UsingShader);
			packet.Write(helper.EoC_PlayerTargetMovementDirection);
			packet.Write(helper.EoC_PlayerTargetMovementTimers[0]);
			packet.Write(helper.EoC_PlayerTargetMovementTimers[1]);
			packet.Write(helper.EoC_FadePositionOffset.X);
			packet.Write(helper.EoC_FadePositionOffset.Y);
			packet.Write(helper.EoC_TimerTarget);
			packet.Write((byte)EoC_FirstBloodWallNPC);

			packet.Write((byte)helper.EoW_SegmentType);
			packet.Write((byte)EoW_GrabbingNPC);
			packet.Write(EoW_grabbingNPCPrevDamage);
			packet.Write((byte)EoW_GrabbedPlayer);

			packet.Write(desomode.EoW_Spawn);
			packet.Write((byte)desomode.EoW_WormSegmentsCount);
			packet.Write(desomode.EoW_GrabTarget?.X ?? -1000);
			packet.Write(desomode.EoW_GrabTarget?.Y ?? -1000);

			packet.Write(desomode.QB_baseScale);
			packet.Write(desomode.QB_baseSize.X);
			packet.Write(desomode.QB_baseSize.Y);

			packet.Write(buffs.primordialWrath);
			packet.Write(stats.baseEndurance);
			packet.Write(stats.endurance);

			packet.Send();
		}
	}
}
