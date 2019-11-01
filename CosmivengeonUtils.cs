using Microsoft.Xna.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Terraria;

namespace CosmivengeonMod{
	public static class CosmivengeonUtils{
		public static float DamageDecrease => Main.expertMode ? 0.25f : 0.5f;

		public static Color TausFavouriteColour = new Color(106, 0, 170);

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

		public static float Average(params int[] values) => values.Sum() / (float)values.Length;

		public static int IntAverage(params int[] values) => (int)Average(values);

		public static Color Blend(Color color, Color otherColor)
			=> new Color(IntAverage(color.R, otherColor.R), IntAverage(color.G, otherColor.G), IntAverage(color.B, otherColor.B), color.A);

		/// <summary>
		/// Chooses the value to get based on the game's mode.
		/// </summary>
		/// <typeparam name="T">The type of the values to choose from.</typeparam>
		/// <param name="normal">The value to use if the game is in Normal Mode.</param>
		/// <param name="expert">The value to use if the game is in Expert Mode.</param>
		/// <param name="desolation">The value to use if the game is in Desolation Mode.</param>
		/// <returns></returns>
		public static T GetModeChoice<T>(T normal, T expert, T desolation)
			=> CosmivengeonWorld.desoMode ? desolation : (Main.expertMode ? expert : normal);

		public static int SpawnProjectile(this NPC npc, Vector2 position, Vector2 velocity, int type, int damage, float knockback, int owner = 255, float ai0 = 0f, float ai1 = 0f){
			int npcDamage = npc.damage;
			npc.damage = 0;

			int proj = Projectile.NewProjectile(position, velocity, type, TrueDamage(damage), knockback, owner, ai0, ai1);

			npc.damage = npcDamage;
			return proj;
		}

		public static int SpawnProjectile(this NPC npc, float spawnX, float spawnY, float velocityX, float velocityY, int type, int damage, float knockback, int owner = 255, float ai0 = 0f, float ai1 = 0f)
			=> npc.SpawnProjectile(new Vector2(spawnX, spawnY), new Vector2(velocityX, velocityY), type, damage, knockback, owner, ai0, ai1);
	}
}