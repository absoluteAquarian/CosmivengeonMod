using System;
using System.Diagnostics.CodeAnalysis;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace CosmivengeonMod{
	public class CosmivengeonMod : Mod{
		public static bool debug_toggleDesoMode = false;
		public static bool debug_canUseExpertModeToggle = false;
		public static bool debug_canUsePotentiometer = false;
		public static bool allowModFlagEdit = true;

		public static float DamageDecrease => Main.expertMode ? 0.25f : 0.5f;

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
		/// Equivalent to (float)System.Math.Cos(angle)
		/// </summary>
		/// <param name="angle">The angle in radians.</param>
		/// <returns></returns>
		[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Name case is intended.")]
		public static float fCos(double angle){
			return (float)Math.Cos(angle);
		}


		/// <summary>
		/// Equivalent to (float)System.Math.Sin(angle)
		/// </summary>
		/// <param name="angle">The angle in radians.</param>
		/// <returns></returns>
		[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Name case is intended.")]
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

		/// <summary>
		/// Converts the given XNA angle to the equivalent Terraria angle.
		/// For reference, -Pi/2 is down on the Y-axis and Pi is right on the X-axis.
		/// </summary>
		/// <param name="angle">The angle in radians.</param>
		/// <returns></returns>
		public static float ToTerrariaAngle(float angle){
			return (angle - MathHelper.Pi) % MathHelper.TwoPi;
		}

		public static bool TileIsSolidOrPlatform(int x, int y){
			Tile tile = Main.tile[x, y];
			return tile != null && (tile.nactive() && (Main.tileSolid[(int)tile.type] || Main.tileSolidTop[(int)tile.type] && (int)tile.frameY == 0) || (int)tile.liquid > 64);
		}
	}

	internal enum CosmivengeonModMessageType : byte{
		SyncPlayer
	}
}