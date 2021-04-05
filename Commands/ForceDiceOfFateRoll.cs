using CosmivengeonMod.Projectiles.Weapons;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Commands{
	public class ForceDiceOfFateRoll : ModCommand{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "cmroll";

		public override string Usage => "[c/ffa600:/cmroll <number>]";

		public override string Description => "Simulates a roll of the Dice of Fate.";

		public override void Action(CommandCaller caller, string input, string[] args){
			if(CosmivengeonMod.Release){
				caller.Reply("This command is disabled.", Color.Red);
				return;
			}

			if(args.Length < 1){
				caller.Reply("Expected an integer argument.", Color.Red);
				caller.Reply(Usage);
				return;
			}

			if(args.Length > 1){
				caller.Reply("Too many arguments.", Color.Red);
				caller.Reply(Usage);
				return;
			}

			if(!int.TryParse(args[0], out int roll)){
				caller.Reply("Expected an integer argument.", Color.Red);
				caller.Reply(Usage);
				return;
			}else if(roll < 1 || roll > 20){
				caller.Reply("Expected an integer argument between 1 and 20, inclusive.", Color.Red);
				caller.Reply(Usage);
				return;
			}

			//Successful roll
			Projectile proj = Projectile.NewProjectileDirect(caller.Player.Center,
				Vector2.UnitX.RotatedByRandom(MathHelper.Pi) * 4f,
				ModContent.ProjectileType<DiceOfFateDice>(),
				0,
				0,
				caller.Player.whoAmI);
			(proj.modProjectile as DiceOfFateDice).random = roll;
		}
	}
}
