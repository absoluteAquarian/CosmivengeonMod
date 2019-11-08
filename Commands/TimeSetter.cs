using Microsoft.Xna.Framework;
using System;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Commands{
	public class TimeSetter : ModCommand{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "timeset";

		public override string Usage => "[c/ffca00:Usage: /timeset <time string> <AM/PM>]";

		public override string Description => "Sets the time.  <time string> is a string, such as \"4:30\" (without the quotes).";

		public override void Action(CommandCaller caller, string input, string[] args){
			const int _4_30 = 4 * 3600 + 30 * 60;
			const int _7_30 = 7 * 3600 + 30 * 60;
			const int _12_00 = 12 * 3600;
			const int _7_30PM_day = 54000;
			const int _4_30AM_night = 32400;
			const int _12AM = _4_30AM_night - _4_30;	//16,200
			const int _12PM = _7_30PM_day - _7_30;		//27,000

			//Only allow time editing if the corresponding flag is true
			if(!CosmivengeonMod.allowTimeEdit){
				Main.NewText("Editing time is disabled.", Color.Red);
				return;
			}

			if(args.Length < 2){
				Main.NewText("Parameter list was too small.", Color.Red);
				return;
			}

			//Check if the time format (AM/PM) is correct
			if(!(args[1] == "PM" || args[1] == "AM")){
				Main.NewText("Time format was invalid.", Color.Red);
				return;
			}

			//Check if the time value is valid (only digits and ":")
			if(!IsValidTime(args[0])){
				Main.NewText("Time format was invalid.", Color.Red);
				return;
			}

			//The time is valid.  Get the tick count and update the time accordingly
			bool am = args[1] == "AM";
			int colonIndex = args[0].IndexOf(':');
			int hour = int.Parse(args[0].Substring(0, colonIndex));
			int minutes = int.Parse(args[0].Substring(colonIndex + 1, 2));
			int tickTime = hour * 3600 + minutes * 60;

			if(am){
				if(tickTime < _4_30){
					am = false;
					tickTime += _12AM;
				}else if(tickTime == _4_30)
					tickTime = 0;
				else if(tickTime == _12_00){
					am = false;
					tickTime = _12AM;
				}else
					tickTime -= _12AM;
			}else{
				if(tickTime < _7_30){
					am = true;
					tickTime += _12PM;
				}else if(tickTime == _7_30)
					tickTime = 0;
				else if(tickTime == _12_00){
					am = true;
					tickTime = _12PM;
				}else
					tickTime -= _12PM;
			}
			
			//Set the time
			Main.dayTime = am;
			Main.time = tickTime;

			Main.NewText($"Time was updated to {args[0]} {args[1]}!", Color.Orange);
		}

		private bool IsValidTime(string time){
			int colonIndex = time.IndexOf(':');
			if(!time.Contains(":") || time.Length < 4 || time.Length > 5 || time.All(c => !char.IsDigit(c) && c != ':') || time.Length - colonIndex != 3)
				return false;
			return true;
		}
	}
}