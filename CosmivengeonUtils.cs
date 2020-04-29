using Microsoft.Xna.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
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
			return tile != null && (tile.nactive() && (Main.tileSolid[tile.type] || Main.tileSolidTop[tile.type] && tile.frameY == 0) || tile.liquid > 64);
		}

		private static double Average(params double[] values) => values.Average();

		private static TOut[] Cast<TIn, TOut>(TIn[] array)
			=> array.Select(x => (TOut)Convert.ChangeType(x,typeof(TOut))).ToArray();

		/// <summary>
		/// Returns the average of the values provided.
		/// </summary>
		/// <typeparam name="T">The type of the elements in the array.</typeparam>
		/// <param name="values">The array of values.</param>
		public static T Average<T>(params T[] values) where T : struct, IComparable<T>{
			//If "T" isn't a primitive type, throw an error
			//Also throw an error if "T" is a bool or char
			if(typeof(T).Assembly != typeof(object).Assembly || !typeof(T).IsPrimitive || values[0] is bool || values[0] is char)
				throw new ArithmeticException($"The given type \"{typeof(T)}\" is invalid for this method.");

			return Cast<double, T>(new[]{ Average(Cast<T, double>(values)) }).First();
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

		/// <summary>
		/// I forgot why I made this, but it works.  Seems pointless but it's best to just keep it here.
		/// </summary>
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
				if(songChance < 0.01)
					modNPC.music = ModContent.GetInstance<CosmivengeonMod>().GetSoundSlot(SoundType.Music, "Sounds/Music/successor_of_the_kazoo");
				else if(songChance < 0.06)
					modNPC.music = ModContent.GetInstance<CosmivengeonMod>().GetSoundSlot(SoundType.Music, "Sounds/Music/RETRO_SuccessorOfTheJewel");
				else
					modNPC.music = ModContent.GetInstance<CosmivengeonMod>().GetSoundSlot(SoundType.Music, "Sounds/Music/Successor_of_the_Jewel");
			}else if(boss == CosmivengeonBoss.Frostbite)
				modNPC.music = ModContent.GetInstance<CosmivengeonMod>().GetSoundSlot(SoundType.Music, "Sounds/Music/Frigid_Feud");

			modNPC.musicPriority = MusicPriority.BossLow;
		}

		/// <summary>
		/// Checks whether the mouse is inside this NPC's collision box or within <paramref name="pixels"/> distance of the collision box.
		/// </summary>
		/// <param name="npc">The npc whose collision box is being used.</param>
		/// <param name="pixels">The distance in pixels that the mouse can be from <paramref name="npc"/>'s collision box in both coordinate axes.</param>
		/// <returns></returns>
		public static bool MouseWithinRange(this NPC npc, float pixels)
			=> Main.MouseWorld.X > npc.position.X - pixels && Main.MouseWorld.X < npc.position.X + npc.width + pixels
				&& Main.MouseWorld.Y > npc.position.Y - pixels && Main.MouseWorld.Y < npc.position.Y + npc.height + pixels;

		/// <summary>
		/// Returns whether the <paramref name="player"/> can summon the given <paramref name="boss"/>.
		/// </summary>
		public static bool TrySummonBoss(CosmivengeonBoss boss, Player player){
			bool canSummonBoss = false;

			int[] bossIDs = new int[]{
				ModContent.NPCType<NPCs.Draek.Draek>(),
				ModContent.NPCType<NPCs.Frostbite.Frostbite>()
			};

			int[] altBossIDs = new int[]{
				ModContent.NPCType<NPCs.Draek.DraekP2Head>(),
				int.MinValue
			};

			string[] badUseMessages = new string[]{
				"\"The geode was unresponsive.  Maybe I should try using it in the forest?\"",
				"\"Looks like the lure didn't work.  Maybe it would work better in a colder area?\""
			};

			bool[] requiredCondition = new bool[]{
				PlayerIsInForest(player),
				player.ZoneSnow
			};

			int givenBoss = (int)boss;

			if(givenBoss < Enum.GetValues(typeof(CosmivengeonBoss)).Cast<int>().Max() + 1){
				//If the condition is met, summon the boss
				//Otherwise, make the player say something
				if(!requiredCondition[givenBoss])
					Main.NewText(badUseMessages[givenBoss]);
				else
					canSummonBoss = !NPC.AnyNPCs(bossIDs[givenBoss]) && !NPC.AnyNPCs(altBossIDs[givenBoss]);
			}

			return canSummonBoss;
		}

		/// <summary>
		/// Spawns the NPC on some point <paramref name="tileDistance"/> tiles away from <paramref name="player"/> given its <paramref name="npcID"/>.
		/// </summary>
		/// <param name="player">The player instance.</param>
		/// <param name="npcID">The ID of the NPC being spawned.</param>
		/// <param name="tileDistance">The radius of the circle around the <paramref name="player"/> where the NPC will spawn.</param>
		/// <returns>Whether or not an NPC could be spawned.</returns>
		public static bool SummonBossNearPlayer(Player player, int npcID, float tileDistance){
			float randomAngle;
			Vector2 offset;
			int targetX, targetY;

			//Get a random angle and set the spawn position to some point on
			// a circle centered around the player
			randomAngle = Main.rand.NextFloat(0, MathHelper.TwoPi);
			offset = randomAngle.ToRotationVector2() * tileDistance * 16f;
			targetX = (int)(player.Center.X + offset.X);
			targetY = (int)(player.Center.Y + offset.Y);

			//Try to spawn the new NPC.  If that failed, then "npc" will be 200
			int npc = NPC.NewNPC(targetX, targetY, npcID);

			//Only display the text if we could spawn the NPC
			if(npc < Main.npc.Length){
				string name = Main.npc[npc].TypeName;

				//Display the "X has awoken!" text since we aren't using NPC.SpawnOnPlayer(int, int)
				Main.NewText(Language.GetTextValue("Announcement.HasAwoken", name), 175, 75, 255);
			}

			return npc != Main.npc.Length;	//Return false if we couldn't generate an NPC
		}

		/// <summary>
		/// Spawns an NPC of id <paramref name="npcID"/> within the X-bounds [<paramref name="maxNegX"/>, <paramref name="maxPosX"/>] + <paramref name="player"/>.position.X and Y-bounds [<paramref name="maxNegY"/> , <paramref name="maxPosY"/>] + <paramref name="player"/>.position.Y
		/// </summary>
		/// <param name="player">The player distance.</param>
		/// <param name="npcID">The ID of the NPC being spawned.</param>
		/// <param name="maxNegX">The max X-coordinate offset to the left of the <paramref name="player"/>.</param>
		/// <param name="maxPosX">The max X-coordinate offset to the right of the <paramref name="player"/>.</param>
		/// <returns>Whether or not an NPC could be spawned.</returns>
		public static bool SummonBossAbovePlayer(Player player, int npcID, float maxNegX, float maxPosX, float maxNegY, float maxPosY){
			const float MinRange = 30 * 16;
			float targetX;
			//Keep generating new X-coords until one isn't very close to
			// the player (unless the possible range is too small)
			do{
				targetX = Main.rand.NextFloat(maxNegX, maxPosX) + player.Center.X;
			}while(Math.Abs(targetX - player.Center.X) < MinRange || (maxPosX < MinRange && maxNegX > -MinRange));

			float targetY = Main.rand.NextFloat(maxNegY, maxPosY) + player.Center.Y;

			int npc = NPC.NewNPC((int)targetX, (int)targetY, npcID);
			string name = Main.npc[npc].TypeName;

			//Display the "X has awoken!" text since we aren't using NPC.SpawnOnPlayer(int, int)
			Main.NewText(Language.GetTextValue("Announcement.HasAwoken", name), 175, 75, 255);

			return npc != Main.npc.Length;
		}

		/// <summary>
		/// Mirrors the angle (in radians) over the X-axis and/or Y-axis.
		/// </summary>
		/// <param name="angle">The original angle in radians.</param>
		/// <param name="mirrorX">Whether or not this angle should be mirrored over the X-axis.</param>
		/// <param name="mirrorY">Whether or not this angle should be mirrored over the Y-axis.</param>
		public static void MirrorAngle(this ref float angle, bool mirrorX = false, bool mirrorY = false){
			//Make the angle positive
			while(angle < 0)
				angle += MathHelper.TwoPi;

			//... and within [0, 2pi)
			angle %= MathHelper.TwoPi;
			
			//If neither option was chosen, don't do anything
			if(!mirrorX && !mirrorY)
				return;

			//If they were both chosen, just add Pi to the angle
			if(mirrorX && mirrorY)
				angle += MathHelper.Pi;
			//Otherwise, handle one case or the other
			else if(mirrorY)
				angle = (angle >= MathHelper.Pi ? 3 : 1) * MathHelper.Pi - angle;
			else if(mirrorX)
				angle = MathHelper.TwoPi - angle;

			//Make the angle be within [0, 2pi) again
			angle %= MathHelper.TwoPi;
		}

		/// <summary>
		/// I made this fuction because fuck Re-Logic and their "npc.velocity.X = Utils.Clamp(npc.velocity.X, min, max)" BS
		/// </summary>
		/// <typeparam name="T">The type of the value being set.</typeparam>
		/// <param name="value">The value being clamped.</param>
		/// <param name="min">The minimum value to be clamped to.</param>
		/// <param name="max">The maximum value to be clamped to.</param>
		public static void Clamp<T>(this ref T value, T min, T max) where T : struct, IComparable<T>
			=> value = value.CompareTo(max) > 0 ? max : (value.CompareTo(min) < 0 ? min : value);

		public static string GetPlaceholderTexture(string name)
			=> $"CosmivengeonMod/Items/PlaceHolder{name}";

		/// <summary>
		/// Scales this <paramref name="npc"/>'s health by the scale <paramref name="factor"/> and <paramref name="numPlayers"/> provided.
		/// <list type="bullet">
		/// <item><description>Formula: <paramref name="npc"/>.lifeMax += <paramref name="npc"/>.lifeMax * <paramref name="factor"/> * (<paramref name="numPlayers"/> + 1)</description></item>
		/// </list>
		/// </summary>
		/// <param name="npc">The NPC instance.</param>
		/// <param name="factor">The scale factor that this <paramref name="npc"/>'s health is scaled by.</param>
		/// <param name="numPlayers">The number of players present.  In singleplayer, <paramref name="numPlayers"/> is 0.  In multiplayer, <paramref name="numPlayers"/> is the amount of players present that aren't this game's client.</param>
		public static void ScaleHealthBy(this NPC npc, float factor, int numPlayers)
			=> npc.lifeMax += (int)(npc.lifeMax * factor * (numPlayers + 1));

		public static void TryDecrementAlpha(this Projectile proj, int amount){
			if(proj.alpha > 0)
				proj.alpha -= amount;
			if(proj.alpha < 0)
				proj.alpha = 0;
		}

		public static void SendMessage(string message, Color? color = null){
#pragma warning disable IDE0054 // Use compound assignment
			color = color ?? Color.White;
#pragma warning restore IDE0054 // Use compound assignment

			if(Main.netMode == NetmodeID.SinglePlayer)
				Main.NewText(message, color.Value);
			else
				NetMessage.BroadcastChatMessage(NetworkText.FromLiteral(message), color.Value);
		}

		/// <summary>
		/// Equivalent to <paramref name="vector"/>.RotatedBy(<paramref name="rotateBy"/>).RotatedByRandom(<paramref name="rotateByRandom"/>)
		/// </summary>
		/// <param name="vector">The original vector</param>
		/// <param name="rotateBy">The absolute rotation in radians</param>
		/// <param name="rotateByRandom">The relative random rotation in radians/></param>
		/// <returns></returns>
		public static Vector2 Rotate(this Vector2 vector, double rotateBy, double rotateByRandom)
			=> vector.RotatedBy(rotateBy).RotatedByRandom(rotateByRandom);

		/// <summary>
		/// Calls the Vector2.Rotate() extension, but it converts the parameters to radians
		/// </summary>
		/// <param name="vector">The original rotation</param>
		/// <param name="rotateByDegrees">The absolute rotation in degrees</param>
		/// <param name="rotateByRandomDegrees">The relative random rotation in degrees</param>
		/// <returns></returns>
		public static Vector2 RotateDegrees(this Vector2 vector, float rotateByDegrees, float rotateByRandomDegrees)
			=> vector.Rotate(MathHelper.ToRadians(rotateByDegrees), MathHelper.ToRadians(rotateByRandomDegrees));

		public static T[] CreateArray<T>(T defaultValue, uint size){
			T[] arr = new T[size];
			for(int i = 0; i < size; i++)
				arr[i] = defaultValue;
			return arr;
		}
	}

	public enum CosmivengeonBoss{
		Draek, Frostbite
	}
}