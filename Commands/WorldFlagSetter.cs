using Microsoft.Xna.Framework;
using System;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Commands{
	public class WorldFlagSetter : ModCommand{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "setworldflag";

		public override string Usage => "/setworldflag <internal flag name> <true/false>";

		public override string Description => "Toggle the given Cosmivengeon world flag.";

		public override void Action(CommandCaller caller, string input, string[] args){
			//Check if the provided flag actually exists.
			//If it does, edit it
			if(args.Length == 0){
				Main.NewText(Usage, Color.DarkOrange);
				return;
			}

			if(args.Length < 2){
				Main.NewText("Parameter list was too small.", Color.Red);
				return;
			}

			FieldInfo field = typeof(CosmivengeonWorld).GetField(args[0], BindingFlags.Public | BindingFlags.Static);
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