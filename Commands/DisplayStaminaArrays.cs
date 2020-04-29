using System.Linq;
using Terraria.ModLoader;

namespace CosmivengeonMod.Commands{
	public class DisplayStaminaArrays : ModCommand{
		public override CommandType Type => CommandType.Chat;
		public override string Command => "dsa";
		public override string Usage => "[c/ffca00:Usage: /dsa]";
		public override string Description => "Displays the current values for the Multipliers and Adders arrays for this player's Stamina";

		public override void Action(CommandCaller caller, string input, string[] args){
			CosmivengeonPlayer mp = caller.Player.GetModPlayer<CosmivengeonPlayer>();
			string mults = string.Join(", ", mp.stamina.Multipliers);
			caller.Reply($"Multipliers: {mults}");
			string adds = string.Join(", ", mp.stamina.Adders);
			caller.Reply($"Adders: {adds}");
		}
	}
}
