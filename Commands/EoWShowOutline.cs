using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace CosmivengeonMod.Commands{
	public class EoWShowOutline : ModCommand{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "eso";

		public override string Usage => "[c/ffa600:/eso <true/false>]";

		public override string Description => "A debug command to show what segments for the Eater of Worlds will do what.";

		public override void Action(CommandCaller caller, string input, string[] args){
			if(!CosmivengeonMod.debug_canShowEoWOutlines){
				caller.Reply("Editing this flag is disabled.", Color.Red);
				return;
			}

			if(!bool.TryParse(args[0], out bool result)){
				caller.Reply("Invalid argument.  Expected a boolean (true/false).", Color.Red);
				return;
			}

			CosmivengeonMod.debug_showEoWOutlines = result;
			caller.Reply($"Flag \"Show EoW Segment Outlines\" was toggled to {result}!", Color.Orange);
		}
	}
}
