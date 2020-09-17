using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

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
				&& !player.ZoneBeach
				&& !player.ZoneDesert
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

		public static bool TileIsSolidNotPlatform(Vector2 pos)
			=> TileIsSolidNotPlatform((int)pos.X >> 4, (int)pos.Y >> 4);

		public static bool TileIsSolidNotPlatform(int x, int y)
			=> TileIsSolidOrPlatform(x, y) && (Main.tile[x, y].type != TileID.Platforms || !TileID.Sets.Platforms[Main.tile[x, y].type]);

		public static bool TileIsSolidOrPlatform(Vector2 pos)
			=> TileIsSolidOrPlatform((int)pos.X >> 4, (int)pos.Y >> 4);

		public static bool TileIsSolidOrPlatform(int x, int y){
			Tile tile = Framing.GetTileSafely(x, y);
			return tile.nactive() && (Main.tileSolid[tile.type] || Main.tileSolidTop[tile.type] && tile.frameY == 0) || tile.liquid > 64;
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

		public static int SpawnProjectileSynced(Vector2 position, Vector2 velocity, int type, int damage, float knockback, float ai0 = 0f, float ai1 = 0f, int owner = -1){
			if(owner == -1)
				owner = Main.myPlayer;

			int proj = Projectile.NewProjectile(position, velocity, type, TrueDamage(damage), knockback, owner, ai0, ai1);
			if(Main.netMode != NetmodeID.SinglePlayer)
				NetMessage.SendData(MessageID.SyncProjectile, number: proj);

			return proj;
		}

		public static int SpawnNPCSynced(Vector2 spawn, int type, float ai0 = 0f, float ai1 = 0f, float ai2 = 0f, float ai3 = 0f){
			int npc = Main.maxNPCs;
			if(Main.netMode != NetmodeID.MultiplayerClient){
				npc = NPC.NewNPC((int)spawn.X, (int)spawn.Y, type, 0, ai0, ai1, ai2, ai3, 255);

				NetMessage.SendData(MessageID.SyncNPC, number: npc);
			}
			return npc;
		}

		public static void PlayMusic(ModNPC modNPC, CosmivengeonBoss boss){
			float songChance = Main.rand.NextFloat();

			CosmivengeonMod modInstance = CosmivengeonMod.Instance;

			if(boss == CosmivengeonBoss.Draek){
				/*	1% chance - kazoo theme
				 *	5% chance - retro theme
				 *	94% chance - current theme
				 */
				if(songChance < 0.01)
					modNPC.music = modInstance.GetSoundSlot(SoundType.Music, "Sounds/Music/successor_of_the_kazoo");
				else if(songChance < 0.06)
					modNPC.music = modInstance.GetSoundSlot(SoundType.Music, "Sounds/Music/RETRO_SuccessorOfTheJewel");
				else
					modNPC.music = modInstance.GetSoundSlot(SoundType.Music, "Sounds/Music/Successor_of_the_Jewel");
			}else if(boss == CosmivengeonBoss.Frostbite)
				modNPC.music = modInstance.GetSoundSlot(SoundType.Music, "Sounds/Music/Frigid_Feud");
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
		/// Scales this <paramref name="npc"/>'s health by the scale <paramref name="factor"/> provided.
		/// </summary>
		/// <param name="npc">The NPC instance.</param>
		/// <param name="factor">The scale factor that this <paramref name="npc"/>'s health is scaled by.</param>
		public static void ScaleHealthBy(this NPC npc, float factor){
			float bossScale = CalculateBossHealthScale(out _);

			npc.lifeMax = (int)(npc.lifeMax * Main.expertLife);
			npc.lifeMax = (int)(npc.lifeMax * factor * bossScale);
		}

		public static float CalculateBossHealthScale(out int playerCount){
			//This is what vanila does
			playerCount = 0;
			float healthFactor = 1f;
			float component = 0.35f;

			if(Main.netMode == NetmodeID.SinglePlayer){
				playerCount = 1;
				return 1f;
			}

			for(int i = 0; i < Main.maxPlayers; i++)
				if(Main.player[i].active)
					playerCount++;

			for(int i = 0; i < playerCount; i++){
				healthFactor += component;
				component += (1f - component) / 3f;
			}

			if(healthFactor > 8f)
				healthFactor = (healthFactor * 2f + 8f) / 3f;
			if(healthFactor > 1000f)
				healthFactor = 1000f;

			return healthFactor;
		}

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

		public static float RotationFromVelocity(float velocityComponent, float velocityRange, float degrees)
			=> Math.Min(Math.Max(-velocityRange, velocityComponent), velocityRange) / velocityRange * MathHelper.ToRadians(degrees);

		public static bool WithinDistance(this Entity ent, Vector2 pos, float distance)
			=> ent.DistanceSQ(pos) < distance * distance;

		public static void SmoothStep(this Entity entity, ref Vector2 pos, int width = -1, int height = -1){
			//Smooth steps down/up slopes
			if(entity is NPC npc){
				if(npc.velocity.Y == 0f)
					Collision.StepDown(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
				if(npc.velocity.Y >= 0)
					Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY, specialChecksMode: 1);
			}else if(entity is Projectile proj){
				width = width == -1 ? proj.width : width;
				height = height == -1 ? proj.height : height;

				if(proj.velocity.Y == 0f)
					Collision.StepDown(ref pos, ref proj.velocity, width, height, ref proj.stepSpeed, ref proj.gfxOffY);
				if(proj.velocity.Y >= 0)
					Collision.StepUp(ref pos, ref proj.velocity, width, height, ref proj.stepSpeed, ref proj.gfxOffY, specialChecksMode: 1);
			}
		}

		public static void SetImmune(this NPC npc, int type) => npc.buffImmune[type] = true;

		public static Vector3 ScreenCoord(this Vector2 vector)
			=> new Vector3(-1 + vector.X / Main.screenWidth * 2, (-1 + vector.Y / Main.screenHeight * 2f) * -1, 0);
		public static Vector3 ScreenCoord(this Vector3 vector)
			=> new Vector3(-1 + vector.X / Main.screenWidth * 2, (-1 + vector.Y / Main.screenHeight * 2f) * -1, 0);

		public static SoundEffectInstance PlayCustomSound(this Mod mod, Vector2 position, string file)
			=> Main.PlaySound(SoundLoader.customSoundType, (int)position.X, (int)position.Y, mod.GetSoundSlot(SoundType.Custom, $"Sounds/Custom/{file}"));

		public static void PrepareToDrawPrimitives(int capacity, out VertexBuffer buffer){
			buffer = new VertexBuffer(Main.graphics.GraphicsDevice, typeof(VertexPositionColor), capacity, BufferUsage.WriteOnly);

			Main.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
		}

		public static void DrawPrimitives(VertexPositionColor[] draws, VertexBuffer buffer){
			Main.graphics.GraphicsDevice.SetVertexBuffer(null);
			buffer.SetData(draws);

			Main.graphics.GraphicsDevice.SetVertexBuffer(buffer);
			Main.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

			foreach(EffectPass currentTechniquePass in CosmivengeonMod.PrimitivesEffect.CurrentTechnique.Passes){
				currentTechniquePass.Apply();
					
				Main.graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.LineList, 0, draws.Length / 2);
			}
		}

		/// <summary>
		/// Returns if this game is the local host in a multiplayer instance.  If the game is a singleplayer game, this method returns false.
		/// </summary>
		public static bool ClientIsLocalHost(int whoAmI){
			if(whoAmI < 0 || whoAmI > Main.maxPlayers)
				return false;

			if(Main.netMode == NetmodeID.SinglePlayer)
				return false;

			if(Main.netMode == NetmodeID.MultiplayerClient)
				return Netplay.Connection.Socket.GetRemoteAddress().IsLocalHost();

			RemoteClient client = Netplay.Clients[whoAmI];
			return client.State == 10 && client.Socket.GetRemoteAddress().IsLocalHost();
		}
	}

	public enum CosmivengeonBoss{
		Draek, Frostbite
	}
}