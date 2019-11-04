using Microsoft.Xna.Framework;
using System;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Commands{
	public class ModFlagSetter : ModCommand{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "setmodflag";

		public override string Usage => "[c/ffa600:/setmodflag <internal flag name> <true/false>]";

		public override string Description => "Sets the given Cosmivengeon Mod flag to the given Boolean value.";

		public override void Action(CommandCaller caller, string input, string[] args){
			//Check if the provided flag actually exists.
			//If it does, edit it
			if(!CosmivengeonMod.allowModFlagEdit){
				Main.NewText("Editing mod flags is disabled.", Color.Red);
				return;
			}

			if(args.Length < 2){
				Main.NewText("Parameter list was too small.", Color.Red);
				return;
			}

			//The user is not allowed to edit the "allowModFlagEdit" flag
			if(args[0] == "allowModFlagEdit"){
				Main.NewText($"Unable to edit the \"allowModFlagEdit\" flag.", Color.Red);
				return;
			}

			FieldInfo field = typeof(CosmivengeonMod).GetField(args[0], BindingFlags.Public | BindingFlags.Static);
			bool? value = null;
			if(args[1] == "true" || args[1] == "false")
				value = Convert.ToBoolean(args[1]);

			if(field != null && value != null){
				field.SetValue(null, (bool)value);
			}else{
				if(field is null)
					Main.NewText($"Unknown flag: {args[0]}", Color.Red);
				if(value is null)
					Main.NewText($"Boolean input was invalid: {args[1]}", Color.Red);
				return;
			}

			Main.NewText($"Flag \"{args[0]}\" was updated to \"{args[1]}\"!", Color.Orange);
		}
	}
}