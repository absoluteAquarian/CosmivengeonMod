﻿using System.IO;
using Terraria;
using Terraria.ID;

namespace CosmivengeonMod.Utility {
	public static class Debug {
		public static bool debug_toggleDesoMode;
		public static bool debug_canUseCrazyHand;
		public static bool debug_canClearBossIDs;
		public static bool debug_showEoWOutlines;
		public static bool debug_canShowEoWOutlines;
		public static bool debug_fastDiceOfFateRecharge;

		public static bool allowModFlagEdit;
		public static bool allowWorldFlagEdit;
		public static bool allowTimeEdit;
		public static bool allowStaminaNoDecay;

		public static void InitializeFlags() {
			debug_toggleDesoMode = true;
			debug_showEoWOutlines = !CoreMod.Release;

			debug_canUseCrazyHand = !CoreMod.Release;
			debug_canClearBossIDs = !CoreMod.Release;
			debug_canShowEoWOutlines = !CoreMod.Release;
			debug_fastDiceOfFateRecharge = !CoreMod.Release;

			allowModFlagEdit = !CoreMod.Release;
			allowWorldFlagEdit = !CoreMod.Release;
			allowTimeEdit = !CoreMod.Release;
			allowStaminaNoDecay = !CoreMod.Release;
		}

		public static void CheckWorldFlagUpdate(ref bool flag) {
			bool old = flag;
			NPC.SetEventFlagCleared(ref flag, -1);

			if (!old && Main.netMode == NetmodeID.Server)
				NetMessage.SendData(MessageID.WorldData);  // Immediately inform clients of new world state
		}

		public static void NetSend(BinaryWriter writer) {
			BitsByte bb = new BitsByte(debug_toggleDesoMode,
				debug_canUseCrazyHand,
				debug_canClearBossIDs,
				debug_canShowEoWOutlines,
				debug_fastDiceOfFateRecharge);
			writer.Write(bb);

			bb = new BitsByte(allowModFlagEdit,
				allowWorldFlagEdit,
				allowTimeEdit,
				allowStaminaNoDecay);
			writer.Write(bb);
		}

		public static void NetReceive(BinaryReader reader) {
			BitsByte bb = reader.ReadByte();
			bb.Retrieve(ref debug_toggleDesoMode,
				ref debug_canUseCrazyHand,
				ref debug_canClearBossIDs,
				ref debug_canShowEoWOutlines,
				ref debug_fastDiceOfFateRecharge);

			bb = reader.ReadByte();
			bb.Retrieve(ref allowModFlagEdit,
				ref allowWorldFlagEdit,
				ref allowTimeEdit,
				ref allowStaminaNoDecay);
		}
	}
}
