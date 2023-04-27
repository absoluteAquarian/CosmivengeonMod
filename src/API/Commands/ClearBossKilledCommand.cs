using CosmivengeonMod.DataStructures;
using CosmivengeonMod.NPCs.Global;
using CosmivengeonMod.Players;
using CosmivengeonMod.Utility;
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CosmivengeonMod.API.Commands {
	public class ClearBossKilledCommand : ModCommand {
		public override CommandType Type => CommandType.Chat;
		public override string Command => "cmcbc";
		public override string Usage => "[c/ffca00:Usage: /cmcbc <id/name> <true/false>]";
		public override string Description => "Used to clear or set the flag for the indicated boss in the \"bosses killed\" dictionary.";

		public override void Action(CommandCaller caller, string input, string[] args) {
			//Only allow editing if the corresponding flag is true
			if (!Debug.debug_canClearBossIDs) {
				caller.Reply("Using this command is disabled.", Color.Red);
				return;
			}

			string inputWithoutCommand = string.Join(" ", args);
			//For some reason, tModLoader splits the args by whitespace.  Let's fix that!
			//First, check if there are only two quotes (user surrounded the boss name with quotes: "Eye of Cthulhu")
			if (inputWithoutCommand.Count(c => c == '\"') == 2) {
				//Remove the first quote and split on the index of the second one
				inputWithoutCommand = inputWithoutCommand.Substring(1);
				args = inputWithoutCommand.Split('\"');
			}

			//If the conversion was successful, args would be something like: { "Eye of Cthulhu", "true" }
			//If it wasn't, the args are either invalid or we have something like: { "4", "true" }

			if (args.Length < 2) {
				caller.Reply("Parameter list was too small.", Color.Red);
				caller.Reply(Usage);
				return;
			}

			if (args.Length > 2) {
				caller.Reply("Parameter list was too big.", Color.Red);
				caller.Reply(Usage);
				return;
			}

			var buffs = StaminaBossKillBuffLoader.Buffs;
			var arg = args[0].ToLower();

			int id;

			//Check if the name is a boss name and not a number ID
			if (buffs.FirstOrDefault(b => b.GetBossNames().Any(n => n.ToLower() == arg)) is StaminaBossKillBuff buffFromName) {
				//We can assume that only one entry had this name in it;  If that isn't the case, then it's not Cosmivengeon's fault
				id = buffFromName.NPCType;
			} else if (!int.TryParse(args[0], out id) || id < 0 || id >= NPCLoader.NPCCount) {
				caller.Reply("Invalid ID string provided.  Expected a positive integer that's a valid NPC ID or a string that's one of the predefined names for a boss.", Color.Red);
				return;
			}

			foreach (var buff in buffs)
				buff.TransmuteNPCType(ref id);

			if (!StaminaBossKillBuffLoader.TryFindBuffData(id, out _)) {
				caller.Reply("Stamina buffs for this boss have not been implemented yet.  Unable to clear or reset this flag.", Color.Red);
				return;
			}

			if (!bool.TryParse(args[1], out bool flag)) {
				caller.Reply("Invalid string provided.  Expected a boolean (true/false) value for the second argument.", Color.Red);
				return;
			}

			//Input is valid, let's try to use it
			BossLogPlayer mp = caller.Player.GetModPlayer<BossLogPlayer>();
			if (!flag)
				mp.BossesKilled.RemoveAll(id);
			else if (!mp.BossesKilled.HasKey(id))
				mp.BossesKilled.Add(new StaminaBuffData(id));

			string bossName = GetBossNameFromDictionary(id);
			string npcNameWithVerb = id == NPCID.Retinazer || id == NPCID.Spazmatism
				? $"\"{bossName}\" have"
				: $"\"{bossName}\" has";

			caller.Reply($"Boss killed flag for {npcNameWithVerb} been set to \"{flag.ToString().ToLower()}\".");
		}

		public static string GetBossNameFromDictionary(int dictID)
			=> dictID == NPCID.Retinazer || dictID == NPCID.Spazmatism ? Language.GetTextValue("Enemies.TheTwins") : Lang.GetNPCNameValue(dictID);
	}
}
