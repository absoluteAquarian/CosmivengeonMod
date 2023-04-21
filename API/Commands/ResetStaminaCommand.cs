using CosmivengeonMod.Players;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace CosmivengeonMod.API.Commands {
	public class ResetStaminaCommand : ModCommand {
		public override CommandType Type => CommandType.Chat;
		public override string Command => "rs";
		public override string Usage => "[c/ffca00:Usage: /rs]";
		public override string Description => "Resets this player's Stamina values as well as any buffs provided from boss kills.";

		public override void Action(CommandCaller caller, string input, string[] args) {
			if (CoreMod.Release) {
				caller.Reply("Using this command is disabled.", Color.Red);
				return;
			}

			caller.Player.GetModPlayer<StaminaPlayer>().stamina.Reset();
			caller.Player.GetModPlayer<BossLogPlayer>().BossesKilled.Clear();

			caller.Reply("Stamina buffs have been cleared for all bosses.", Color.Orange);
		}
	}
}
