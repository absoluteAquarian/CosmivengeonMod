using CosmivengeonMod.Worlds;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.API.Commands{
	public class ToggleDifficulty : ModCommand{
		public override CommandType Type => CommandType.Console;
		public override string Command => "cmtd";
		public override string Usage => "[c/ffca00:Usage: /cmtd <true/false>]";
		public override string Description => "Used in the server's console to toggle Desolation mode.  Required for dedicated servers, not necessary for local servers.";

		public override void Action(CommandCaller caller, string input, string[] args){
			if(Main.gameMenu){
				caller.Reply("ERROR: Game instance is not in a world yet!", Color.Red);
				return;
			}

			if(args.Length == 0){
				caller.Reply("Parameter list was too small.", Color.Red);
				return;
			}

			if(args.Length > 1){
				caller.Reply("Parameter list was too large.", Color.Red);
				return;
			}

			if(!bool.TryParse(args[0], out bool result)){
				caller.Reply("Expected a boolean argument.", Color.Red);
				return;
			}

			//Set the difficulty
			WorldEvents.desoMode = result;
		}
	}
}
