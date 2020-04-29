using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace CosmivengeonMod.Commands{
	public class ClearBossKilledCommand : ModCommand{
		public override CommandType Type => CommandType.Chat;
		public override string Command => "cmcbc";
		public override string Usage => "[c/ffca00:Usage: /cmcbc <npc id> <true/false>]";
		public override string Description => "Used to clear the indicated boss <npc id> from the \"bosses killed\" Dictionary.";

		public override void Action(CommandCaller caller, string input, string[] args){
			//Only allow editing if the corresponding flag is true
			if(!CosmivengeonMod.debug_canClearBossIDs){
				caller.Reply("Using this command is disabled.", Color.Red);
				return;
			}

			if(args.Length < 2){
				caller.Reply("Parameter list was too small.", Color.Red);
				return;
			}

			if(!int.TryParse(args[0], out int id) || id < 0 || id > NPCLoader.NPCCount){
				caller.Reply("Invalid ID string provided.  Expected a positive integer that's a valid NPC ID.", Color.Red);
				return;
			}

			if(!bool.TryParse(args[1], out bool flag)){
				caller.Reply("Invalid string provided.  Expected a boolean (true/false) value for the second argument.", Color.Red);
				return;
			}

			//Input is valid, let's try to use it
			caller.Player.GetModPlayer<CosmivengeonPlayer>().BossesKilled.Remove(id);
		}
	}
}
