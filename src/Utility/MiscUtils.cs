using CosmivengeonMod.DataStructures;
using CosmivengeonMod.Enums;
using CosmivengeonMod.Systems;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CosmivengeonMod.Utility {
	public static class MiscUtils {
		public static float DamageDecrease => Main.masterMode ? 1f / 6f : Main.expertMode ? 1f / 4f : 1f / 2f;

		public static Color TausFavouriteColour = new Color(106, 0, 170);

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
			=> TileIsSolidOrPlatform(x, y) && (Main.tile[x, y].TileType != TileID.Platforms || !TileID.Sets.Platforms[Main.tile[x, y].TileType]);

		public static bool TileIsSolidOrPlatform(Vector2 pos)
			=> TileIsSolidOrPlatform((int)pos.X >> 4, (int)pos.Y >> 4);

		public static bool TileIsSolidOrPlatform(int x, int y) {
			Tile tile = Framing.GetTileSafely(x, y);
			return tile.HasUnactuatedTile && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType] && tile.TileFrameY == 0) || tile.LiquidAmount > 64;
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
			=> WorldEvents.desoMode ? desolation : (Main.expertMode ? expert : normal);

		public static float CalculateBossHealthScale(out int playerCount) {
			//This is what vanila does
			// TODO: was this changed in 1.4?
			playerCount = 0;
			float healthFactor = 1f;
			float component = 0.35f;

			if (Main.netMode == NetmodeID.SinglePlayer) {
				playerCount = 1;
				return 1f;
			}

			for (int i = 0; i < Main.maxPlayers; i++)
				if (Main.player[i].active)
					playerCount++;

			for (int i = 0; i < playerCount; i++) {
				healthFactor += component;
				component += (1f - component) / 3f;
			}

			if (healthFactor > 8f)
				healthFactor = (healthFactor * 2f + 8f) / 3f;
			if (healthFactor > 1000f)
				healthFactor = 1000f;

			return healthFactor;
		}

		public static int SpawnProjectileSynced(IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockback, float ai0 = 0f, float ai1 = 0f, int owner = -1) {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return Main.maxProjectiles;

			if (owner == -1)
				owner = Main.myPlayer;

			int proj = Projectile.NewProjectile(source, position, velocity, type, TrueDamage(damage), knockback, owner, ai0, ai1);
			if (Main.netMode == NetmodeID.Server)
				NetMessage.SendData(MessageID.SyncProjectile, number: proj);

			return proj;
		}

		public static int SpawnNPCSynced(IEntitySource source, Vector2 spawn, int type, float ai0 = 0f, float ai1 = 0f, float ai2 = 0f, float ai3 = 0f) {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return Main.maxNPCs;

			int npc = NPC.NewNPC(source, (int)spawn.X, (int)spawn.Y, type, 0, ai0, ai1, ai2, ai3, 255);
			if (Main.netMode == NetmodeID.Server)
				NetMessage.SendData(MessageID.SyncNPC, number: npc);

			return npc;
		}

		public static void PlayMusic(ModNPC modNPC, CosmivengeonBoss boss) {
			if (Main.netMode == NetmodeID.Server)
				return;

			modNPC.Music = BossPackage.bossInfo[boss].musicTable.Get();
		}

		/// <summary>
		/// Returns whether the <paramref name="player"/> can summon the given <paramref name="boss"/>.
		/// </summary>
		public static bool TrySummonBoss(CosmivengeonBoss boss, Player player) {
			bool canSummonBoss = false;

			if (!BossPackage.bossInfo.ContainsKey(boss))
				throw new ArgumentException($"Boss enum value hasn't been assigned to any boss information: CosmivengeonBoss.{boss}");

			BossPackage package = BossPackage.bossInfo[boss];
			if (!package.checkSummonRequirement(player))
				Main.NewText($"[n:{player.name}] {package.invalidSummonUseMessage}");
			else
				canSummonBoss = !NPC.AnyNPCs(package.bossID) && (package.altBossID == -1 || !NPC.AnyNPCs(package.altBossID));

			return canSummonBoss;
		}

		/// <summary>
		/// Spawns the NPC on some point <paramref name="tileDistance"/> tiles away from <paramref name="player"/> given its <paramref name="npcID"/>.
		/// </summary>
		/// <param name="player">The player instance.</param>
		/// <param name="npcID">The ID of the NPC being spawned.</param>
		/// <param name="tileDistance">The radius of the circle around the <paramref name="player"/> where the NPC will spawn.</param>
		/// <returns>Whether or not an NPC could be spawned.</returns>
		public static bool SummonBossNearPlayer(Player player, int npcID, float tileDistance) {
			if (player.whoAmI != Main.myPlayer)
				return true;

			if (Main.netMode == NetmodeID.MultiplayerClient) {
				var packet = CoreMod.Instance.GetPacket();
				packet.Write((byte)MessageType.SpawnBoss);
				packet.Write((byte)player.whoAmI);
				packet.Write(npcID);
				packet.Write(tileDistance);
				packet.Send();
				return true;
			}

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
			int npc = NPC.NewNPC(NPC.GetBossSpawnSource(player.whoAmI), targetX, targetY, npcID);

			//Only display the text if we could spawn the NPC
			if (npc < Main.npc.Length) {
				string name = Main.npc[npc].TypeName;

				//Display the "X has awoken!" text since we aren't using NPC.SpawnOnPlayer(int, int)
				Main.NewText(Language.GetTextValue("Announcement.HasAwoken", name), 175, 75, 255);
			}

			return npc != Main.npc.Length;  //Return false if we couldn't generate an NPC
		}

		/// <summary>
		/// Spawns an NPC of id <paramref name="npcID"/> within the X-bounds [<paramref name="maxNegX"/>, <paramref name="maxPosX"/>] + <paramref name="player"/>.position.X and Y-bounds [<paramref name="maxNegY"/> , <paramref name="maxPosY"/>] + <paramref name="player"/>.position.Y
		/// </summary>
		/// <param name="player">The player distance.</param>
		/// <param name="npcID">The ID of the NPC being spawned.</param>
		/// <param name="maxNegX">The max X-coordinate offset to the left of the <paramref name="player"/>.</param>
		/// <param name="maxPosX">The max X-coordinate offset to the right of the <paramref name="player"/>.</param>
		/// <param name="maxNegY">The max Y-coordinate offset to the left of the <paramref name="player"/>.</param>
		/// <param name="maxPosY">The max Y-coordinate offset to the right of the <paramref name="player"/>.</param>
		/// <returns>Whether or not an NPC could be spawned.</returns>
		public static bool SummonBossAbovePlayer(Player player, int npcID, float maxNegX, float maxPosX, float maxNegY, float maxPosY) {
			if (player.whoAmI != Main.myPlayer)
				return true;

			if (Main.netMode == NetmodeID.MultiplayerClient) {
				var packet = CoreMod.Instance.GetPacket();
				packet.Write((byte)MessageType.SpawnBossAbovePlayer);
				packet.Write((byte)player.whoAmI);
				packet.Write(npcID);
				packet.Write(maxNegX);
				packet.Write(maxPosX);
				packet.Write(maxNegY);
				packet.Write(maxPosY);
				packet.Send();
				return true;
			}

			const float MinRange = 30 * 16;
			float targetX;
			//Keep generating new X-coords until one isn't very close to
			// the player (unless the possible range is too small)
			do {
				targetX = Main.rand.NextFloat(maxNegX, maxPosX) + player.Center.X;
			} while (Math.Abs(targetX - player.Center.X) < MinRange || (maxPosX < MinRange && maxNegX > -MinRange));

			float targetY = Main.rand.NextFloat(maxNegY, maxPosY) + player.Center.Y;

			int npc = NPC.NewNPC(NPC.GetBossSpawnSource(player.whoAmI), (int)targetX, (int)targetY, npcID);
			string name = Main.npc[npc].TypeName;

			//Display the "X has awoken!" text since we aren't using NPC.SpawnOnPlayer(int, int)
			Main.NewText(Language.GetTextValue("Announcement.HasAwoken", name), 175, 75, 255);

			return npc != Main.npc.Length;
		}

		public static string GetPlaceholderTexture(string name)
			=> $"CosmivengeonMod/Items/PlaceHolder{name}";

		public static void SendMessage(string message, Color? color = null) {
			color ??= Color.White;

			if (Main.netMode == NetmodeID.SinglePlayer)
				Main.NewText(message, color.Value);
			else
				ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(message), color.Value);
		}

		public static T[] CreateArray<T>(T defaultValue, uint size) {
			T[] arr = new T[size];
			for (int i = 0; i < size; i++)
				arr[i] = defaultValue;
			return arr;
		}

		public static float RotationFromVelocity(float velocityComponent, float velocityRange, float degrees)
			=> Math.Min(Math.Max(-velocityRange, velocityComponent), velocityRange) / velocityRange * MathHelper.ToRadians(degrees);

		/// <summary>
		/// Returns if this game is the local host in a multiplayer instance.  If the game is a singleplayer game, this method returns false.
		/// </summary>
		public static bool ClientIsLocalHost(int whoAmI) {
			if (whoAmI < 0 || whoAmI > Main.maxPlayers)
				return false;

			if (Main.netMode == NetmodeID.SinglePlayer)
				return false;

			if (Main.netMode == NetmodeID.MultiplayerClient)
				return Netplay.Connection.Socket.GetRemoteAddress().IsLocalHost();

			RemoteClient client = Netplay.Clients[whoAmI];
			return client.State == 10 && client.Socket.GetRemoteAddress().IsLocalHost();
		}
	}
}
