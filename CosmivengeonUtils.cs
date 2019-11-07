using Microsoft.Xna.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

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
		public static int TrueDamage(int damage) => (int)(damage * DamageDecrease);

		/// <summary>
		/// Equivalent to (float)System.Math.Cos(angle)
		/// </summary>
		/// <param name="angle">The angle in radians.</param>
		[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Name case is intended.")]
		public static float fCos(double angle) => (float)Math.Cos(angle);


		/// <summary>
		/// Equivalent to (float)System.Math.Sin(angle)
		/// </summary>
		/// <param name="angle">The angle in radians.</param>
		[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Name case is intended.")]
		public static float fSin(double angle) => (float)Math.Sin(angle);

		/// <summary>
		/// Equivalent to (float)System.Math.Sqrt(value)
		/// </summary>
		[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Name case is intended.")]
		public static float fSqrt(double value) => (float)Math.Sqrt(value);

		/// <summary>
		/// Converts the given Terraria angle to the proper XNA angle.
		/// For reference, Pi/2 is down on the Y-axis and Pi is left on the X-axis.
		/// </summary>
		/// <param name="angle">The angle in radians.</param>
		/// <returns></returns>
		public static float ToActualAngle(float angle) => (angle + MathHelper.Pi) % MathHelper.TwoPi;

		/// <summary>
		/// Converts the given XNA angle to the equivalent Terraria angle.
		/// For reference, -Pi/2 is down on the Y-axis and Pi is right on the X-axis.
		/// </summary>
		/// <param name="angle">The angle in radians.</param>
		/// <returns></returns>
		public static float ToTerrariaAngle(float angle) => (angle - MathHelper.Pi) % MathHelper.TwoPi;

		public static bool TileIsSolidOrPlatform(int x, int y){
			Tile tile = Main.tile[x, y];
			return tile != null && (tile.nactive() && (Main.tileSolid[(int)tile.type] || Main.tileSolidTop[(int)tile.type] && (int)tile.frameY == 0) || (int)tile.liquid > 64);
		}

		private static double Average(params double[] values) => values.Sum() / values.Length;

		/// <summary>
		/// Returns the average of the values provided.
		/// </summary>
		/// <typeparam name="T">The type of the elements in the array.</typeparam>
		/// <param name="values">The array of values.</param>
		public static T Average<T>(params T[] values) where T : struct, IComparable<T>{
			//If "T" isn't a primitive type, throw an error
			if(typeof(T).Assembly != typeof(object).Assembly || !typeof(T).IsPrimitive)
				throw new ArithmeticException($"The given type \"{typeof(T)}\" is invalid for this method.");

			return new double[]{ Average(values.Cast<double>().ToArray()) }.Cast<T>().First();
		}

		/// <summary>
		/// Blends the two colours together with a 50% bias.
		/// </summary>
		/// <param name="color"></param>
		/// <param name="otherColor"></param>
		public static Color Blend(Color color, Color otherColor)
			=> FadeBetween(color, otherColor, 0.5f);

		/// <summary>
		/// Blends the two colours with the given % bias towards "toColor".  Thanks direwolf420!
		/// </summary>
		/// <param name="fromColor">The original colour.</param>
		/// <param name="toColor">The colour being blended towards</param>
		/// <param name="fadePercent">The % bias towards "toColor".  Range: [0,1]</param>
		public static Color FadeBetween(Color fromColor, Color toColor, float fadePercent)
			=> fadePercent == 0f ? fromColor : new Color(fromColor.ToVector4() * (1f - fadePercent) + toColor.ToVector4() * fadePercent);

		/// <summary>
		/// Chooses the value to get based on the game's mode.
		/// </summary>
		/// <typeparam name="T">The type of the values to choose from.</typeparam>
		/// <param name="normal">The value to use if the game is in Normal Mode.</param>
		/// <param name="expert">The value to use if the game is in Expert Mode.</param>
		/// <param name="desolation">The value to use if the game is in Desolation Mode.</param>
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

		public static void PlayMusic(ModNPC modNPC, CosmivengeonBoss boss){
			float songChance = Main.rand.NextFloat();
			if(boss == CosmivengeonBoss.Draek){
				if(modNPC.npc.type == ModContent.NPCType<NPCs.Draek.DraekP2Head>())
					
				if(songChance < 0.01 || modNPC.music == CosmivengeonMod.Instance.GetSoundSlot(SoundType.Music, "Sounds/Music/successor_of_the_kazoo"))
					modNPC.music = CosmivengeonMod.Instance.GetSoundSlot(SoundType.Music, "Sounds/Music/successor_of_the_kazoo");
				else if(songChance < 0.06 || modNPC.music == CosmivengeonMod.Instance.GetSoundSlot(SoundType.Music, "Sounds/Music/RETRO_SuccessorOfTheJewel"))
					modNPC.music = CosmivengeonMod.Instance.GetSoundSlot(SoundType.Music, "Sounds/Music/RETRO_SuccessorOfTheJewel");
				else
					modNPC.music = CosmivengeonMod.Instance.GetSoundSlot(SoundType.Music, "Sounds/Music/Successor_of_the_Jewel");
			}

			modNPC.musicPriority = MusicPriority.BossLow;
		}
	}

	public enum CosmivengeonBoss{
		Draek
	}
}