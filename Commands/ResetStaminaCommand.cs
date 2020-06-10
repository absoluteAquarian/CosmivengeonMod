using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace CosmivengeonMod.Commands{
	public class ResetStaminaCommand : ModCommand{
		public override CommandType Type => CommandType.Chat;
		public override string Command => "rs";
		public override string Usage => "[c/ffca00:Usage: /rs]";
		public override string Description => "Resets this player's Stamina values as well as any buffs provided from boss kills.";

		public override void Action(CommandCaller caller, string input, string[] args){
			caller.Player.GetModPlayer<CosmivengeonPlayer>().stamina.Reset();
			caller.Player.GetModPlayer<CosmivengeonPlayer>().BossesKilled.Clear();

			caller.Reply("Stamina buffs have been cleared for all bosses.", Color.Orange);
		}
	}
}
