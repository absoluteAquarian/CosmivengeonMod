using Microsoft.Xna.Framework;
using System;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Commands{
	public class ModFlagSetter : ModCommand{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "setmodflag";

		public override string Usage => "/setmodflag <internal flag name> <true/false>";

		public override string Description => "Toggle the given Cosmivengeon Mod flag.";

		public override void Action(CommandCaller caller, string input, string[] args){
			//Check if the provided flag actually exists.
			//If it does, edit it
			if(!CosmivengeonMod.allowModFlagEdit){
				Main.NewText("Editing mod flags is disabled.", Color.Red);
				return;
			}

			if(args.Length == 0){
				Main.NewText(Usage, Color.DarkOrange);
				return;
			}

			if(args.Length < 2){
				Main.NewText("Parameter list was too small.", Color.Red);
				return;
			}

			//There's one flag the user shouldn't be able to edit:  CosmivengeonMod.allowModFlagEdit
			//If the user tries to edit this flag OR the flag is already false, return a UsageException
			if(args[0] == "allowModFlagEdit"){
				Main.NewText("Unable to edit the \"allowModFlagEdit\" flag.", Color.Red);
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