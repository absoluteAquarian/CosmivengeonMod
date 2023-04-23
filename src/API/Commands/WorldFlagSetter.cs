using CosmivengeonMod.Utility;
using CosmivengeonMod.Systems;
using Microsoft.Xna.Framework;
using System;
using System.Reflection;
using Terraria.ModLoader;

namespace CosmivengeonMod.API.Commands {
	public class WorldFlagSetter : ModCommand {
		public override CommandType Type => CommandType.Chat;

		public override string Command => "setworldflag";

		public override string Usage => "[c/ff6a00:Usage: /setworldflag <internal flag name> <true/false>]";

		public override string Description => "Sets the given Cosmivengeon world flag to the given Boolean value.";

		public override void Action(CommandCaller caller, string input, string[] args) {
			//Check if the provided flag actually exists.
			//If it does, edit it
			if (!Debug.allowWorldFlagEdit) {
				caller.Reply("Editing world flags is disabled.", Color.Red);
				return;
			}

			if (args.Length < 2) {
				caller.Reply("Parameter list was too small.", Color.Red);
				return;
			}

			FieldInfo field = typeof(WorldEvents).GetField(args[0], BindingFlags.Public | BindingFlags.Static);
			bool? value = null;
			if (args[1] == "true" || args[1] == "false")
				value = Convert.ToBoolean(args[1]);

			if (field != null && value != null) {
				field.SetValue(null, (bool)value);
			} else {
				if (field is null)
					caller.Reply($"Unknown flag: {args[0]}", Color.Red);
				if (value is null)
					caller.Reply($"Boolean input was invalid: {args[1]}", Color.Red);
				return;
			}

			caller.Reply($"Flag \"{args[0]}\" was updated to \"{args[1]}\"!", Color.Orange);
		}
	}
}
