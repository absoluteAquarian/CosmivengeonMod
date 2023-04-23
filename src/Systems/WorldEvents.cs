using CosmivengeonMod.Utility;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CosmivengeonMod.Systems {
	public class WorldEvents : ModSystem {
		public static bool desoMode;

		public static bool downedDraekBoss;
		public static bool downedFrostbiteBoss;

		public override void OnWorldLoad() {
			// World flags
			desoMode = false;
			downedDraekBoss = false;
			downedFrostbiteBoss = false;
		}

		public override void OnWorldUnload() => OnWorldLoad();

		public override void SaveWorldData(TagCompound tag) {
			List<string> downed = new List<string>();

			if (downedDraekBoss)
				downed.Add("draek");
			if (downedFrostbiteBoss)
				downed.Add("frostbite");

			tag["downed"] = downed;
			tag["desolation"] = desoMode;
		}

		public override void LoadWorldData(TagCompound tag) {
			var downed = tag.GetList<string>("downed");
			downedDraekBoss = downed.Contains("draek");
			downedFrostbiteBoss = downed.Contains("frostbite");
			desoMode = tag.GetBool("desolation");

			Debug.InitializeFlags();
		}

		public override void NetSend(BinaryWriter writer) {
			BitsByte flags = new BitsByte();
			flags[0] = downedDraekBoss;
			flags[1] = desoMode;
			flags[3] = downedFrostbiteBoss;
			writer.Write(flags);

			Debug.NetSend(writer);
		}

		public override void NetReceive(BinaryReader reader) {
			BitsByte flags = reader.ReadByte();
			downedDraekBoss = flags[0];
			desoMode = flags[1];
			downedFrostbiteBoss = flags[2];

			Debug.NetReceive(reader);
		}
	}
}
