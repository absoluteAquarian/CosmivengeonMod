using CosmivengeonMod.Players;
using CosmivengeonMod.Utility;
using Microsoft.Xna.Framework;
using System;
using Terraria.ModLoader;

namespace CosmivengeonMod.API.Commands{
	public class StaminaNoDecay : ModCommand{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "nsd";

		public override string Usage => "[c/ffca00:Usage: /nsd <true/false>]";

		public override string Description => "Sets whether the Stamina Bar should decay or not.  true = no decay, false = decay";

		public override void Action(CommandCaller caller, string input, string[] args){
			//Only allow editing if the corresponding flag is true
			if(!Debug.allowStaminaNoDecay){
				caller.Reply("Editing this flag is disabled.", Color.Red);
				return;
			}

			if(args.Length < 1){
				caller.Reply("Parameter list was too small.", Color.Red);
				caller.Reply(Usage);
				return;
			}

			bool? value = null;
			if(args[0] == "true" || args[0] == "false")
				value = Convert.ToBoolean(args[0]);

			if(!value.HasValue){
				caller.Reply($"Boolean input was invalid: {args[0]}", Color.Red);
				return;
			}

			caller.Player.GetModPlayer<StaminaPlayer>().stamina.NoDecay = value.Value;

			caller.Reply($"Flag \"No Stamina Decay\" was updated to {args[0]}!", Color.Orange);
		}
	}
}
