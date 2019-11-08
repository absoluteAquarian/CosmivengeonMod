using CosmivengeonMod.Buffs.Stamina;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Commands{
	public class StaminaNoDecay : ModCommand{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "nsd";

		public override string Usage => "[c/ffca00:Usage: /nsd <true/false>]";

		public override string Description => "Sets whether the Stamina Bar should decay or not.  true = no decay, false = decay";

		public override void Action(CommandCaller caller, string input, string[] args){
			//Only allow time editing if the corresponding flag is true
			if(!CosmivengeonMod.allowStaminaNoDecay){
				Main.NewText("Editing this flag is disabled.", Color.Red);
				return;
			}

			if(args.Length < 1){
				Main.NewText("Parameter list was too small.", Color.Red);
				return;
			}

			bool? value = null;
			if(args[0] == "true" || args[0] == "false")
				value = Convert.ToBoolean(args[0]);

			if(!value.HasValue){
				Main.NewText($"Boolean input was invalid: {args[0]}", Color.Red);
				return;
			}

			Main.LocalPlayer.GetModPlayer<CosmivengeonPlayer>().stamina.NoDecay = value.Value;

			Main.NewText($"Flag \"No Stamina Decay\" was updated to {args[0]}!", Color.Orange);
		}
	}
}