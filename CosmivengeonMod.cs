using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace CosmivengeonMod{
	public class CosmivengeonMod : Mod{
		public static bool desoMode = false;
		public static bool debug_toggleDesoMode = true;
		public static bool debug_canUseExpertModeToggle = true;
		public static bool debug_canUsePotentiometer = true;

		public static float DamageDecrease{
			get{
				return (Main.expertMode) ? 0.25f : 0.5f;
			}
		}

		public static Color TausFavouriteColour = new Color(106, 0, 170);

		public CosmivengeonMod(){
			
		}

		public static bool PlayerIsInForest(Player player){
			return !player.ZoneJungle
				&& !player.ZoneDungeon
				&& !player.ZoneCorrupt
				&& !player.ZoneCrimson
				&& !player.ZoneHoly
				&& !player.ZoneSnow
				&& !player.ZoneUndergroundDesert
				&& !player.ZoneGlowshroom
				&& !player.ZoneMeteor
				&& player.ZoneOverworldHeight;
		}

		/// <summary>
		/// Vanilla Terraria code is broken.  Reduces hostile projectile damage to account for this.
		/// </summary>
		/// <param name="damage">The intended damage to be dealt.</param>
		/// <returns></returns>
		public static int TrueDamage(int damage){
			return (int)(damage * DamageDecrease);
		}

		/// <summary>
		/// Equivalent to (float)Math.Cos(angle)
		/// </summary>
		/// <param name="angle">The angle in radians.</param>
		/// <returns></returns>
		public static float fCos(double angle){
			return (float)Math.Cos(angle);
		}

		/// <summary>
		/// Equivalent to (float)Math.Sin(angle)
		/// </summary>
		/// <param name="angle">The angle in radians.</param>
		/// <returns></returns>
		public static float fSin(double angle){
			return (float)Math.Sin(angle);
		}

		/// <summary>
		/// Converts the given Terraria angle to the proper XNA angle.
		/// For reference, Pi/2 is down on the Y-axis and Pi is left on the X-axis.
		/// </summary>
		/// <param name="angle">The angle in radians.</param>
		/// <returns></returns>
		public static float ToActualAngle(float angle){
			return (angle + MathHelper.Pi) % MathHelper.TwoPi;
		}
	}
}