using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;

namespace CosmivengeonMod.API.ExternalMethods{
	public static partial class Vanilla{
		/// <summary>
		/// A modified version of <see cref="WorldGen.CloudIsland(int, int)"/> for the Prism subworld
		/// </summary>
		public static void PrismCloudIsland(int i, int j){
			double num = WorldGen.genRand.Next(100, 150);
			double num2;

			float num3 = WorldGen.genRand.Next(20, 30);

			int num4 = i;
			int num5 = i;
			int num6 = i;
			int num7 = j;
			
			Vector2 vector = default;
			vector.X = i;
			vector.Y = j;
			
			Vector2 vector2 = default;
			vector2.X = WorldGen.genRand.Next(-20, 21) * 0.2f;
			
			while(vector2.X > -2f && vector2.X < 2f)
				vector2.X = WorldGen.genRand.Next(-20, 21) * 0.2f;

			vector2.Y = WorldGen.genRand.Next(-20, -10) * 0.02f;

			while(num > 0.0 && num3 > 0f){
				num -= WorldGen.genRand.Next(4);
				num3 -= 1f;

				int num8 = (int)(vector.X - num * 0.5);
				int num9 = (int)(vector.X + num * 0.5);
				int num10 = (int)(vector.Y - num * 0.5);
				int num11 = (int)(vector.Y + num * 0.5);

				if(num8 < 0)
					num8 = 0;

				if(num9 > Main.maxTilesX)
					num9 = Main.maxTilesX;

				if(num10 < 0)
					num10 = 0;

				if(num11 > Main.maxTilesY)
					num11 = Main.maxTilesY;

				num2 = num * WorldGen.genRand.Next(80, 120) * 0.01;
				float num12 = vector.Y + 1f;

				for(int k = num8; k < num9; k++){
					if(WorldGen.genRand.Next(2) == 0)
						num12 += WorldGen.genRand.Next(-1, 2);

					if(num12 < vector.Y)
						num12 = vector.Y;

					if(num12 > vector.Y + 2f)
						num12 = vector.Y + 2f;

					for(int l = num10; l < num11; l++){
						if(l <= num12)
							continue;

						float num13 = Math.Abs(k - vector.X);
						float num14 = Math.Abs(l - vector.Y) * 3f;
						if(Math.Sqrt(num13 * num13 + num14 * num14) < num2 * 0.4){
							if(k < num4)
								num4 = k;

							if(k > num5)
								num5 = k;

							if(l < num6)
								num6 = l;

							if(l > num7)
								num7 = l;

							Main.tile[k, l].active(active: true);
							Main.tile[k, l].type = TileID.Cloud;
							WorldGen.SquareTileFrame(k, l);
						}
					}
				}

				vector += vector2;
				vector2.X += WorldGen.genRand.Next(-20, 21) * 0.05f;
				if(vector2.X > 1f)
					vector2.X = 1f;

				if(vector2.X < -1f)
					vector2.X = -1f;

				if(vector2.Y > 0.2)
					vector2.Y = -0.2f;

				if(vector2.Y < -0.2)
					vector2.Y = -0.2f;
			}

			int num15 = num4;
			int num17;

			for(num15 += WorldGen.genRand.Next(5); num15 < num5; num15 += WorldGen.genRand.Next(num17, (int)(num17 * 1.5))){
				int num16 = num7;

				while(!Main.tile[num15, num16].active())
					num16--;

				num16 += WorldGen.genRand.Next(-3, 4);
				num17 = WorldGen.genRand.Next(4, 8);

				int num18 = TileID.Cloud;
				if(WorldGen.genRand.Next(4) == 0)
					num18 = TileID.SnowCloud;

				for(int m = num15 - num17; m <= num15 + num17; m++){
					for(int n = num16 - num17; n <= num16 + num17; n++){
						if(n > num6){
							float num19 = Math.Abs(m - num15);
							float num20 = Math.Abs(n - num16) * 2;

							if(Math.Sqrt(num19 * num19 + num20 * num20) < num17 + WorldGen.genRand.Next(2)) {
								Main.tile[m, n].active(active: true);
								Main.tile[m, n].type = (ushort)num18;
								WorldGen.SquareTileFrame(m, n);
							}
						}
					}
				}
			}

			num = WorldGen.genRand.Next(80, 95);
			num3 = WorldGen.genRand.Next(10, 15);

			vector.X = i;
			vector.Y = num6;
			vector2.X = WorldGen.genRand.Next(-20, 21) * 0.2f;

			while(vector2.X > -2f && vector2.X < 2f)
				vector2.X = WorldGen.genRand.Next(-20, 21) * 0.2f;

			vector2.Y = WorldGen.genRand.Next(-20, -10) * 0.02f;
			while(num > 0.0 && num3 > 0f){
				num -= WorldGen.genRand.Next(4);
				num3 -= 1f;
				int num8 = (int)(vector.X - num * 0.5);
				int num9 = (int)(vector.X + num * 0.5);
				int num10 = num6 - 1;
				int num11 = (int)(vector.Y + num * 0.5);
				if(num8 < 0)
					num8 = 0;

				if(num9 > Main.maxTilesX)
					num9 = Main.maxTilesX;

				if(num10 < 0)
					num10 = 0;

				if(num11 > Main.maxTilesY)
					num11 = Main.maxTilesY;

				num2 = num * WorldGen.genRand.Next(80, 120) * 0.01;

				float num21 = vector.Y + 1f;
				for(int num22 = num8; num22 < num9; num22++){
					if(WorldGen.genRand.Next(2) == 0)
						num21 += WorldGen.genRand.Next(-1, 2);

					if(num21 < vector.Y)
						num21 = vector.Y;

					if(num21 > vector.Y + 2f)
						num21 = vector.Y + 2f;

					for(int num23 = num10; num23 < num11; num23++){
						if(num23 > num21){
							float num24 = Math.Abs(num22 - vector.X);
							float num25 = Math.Abs(num23 - vector.Y) * 3f;

							if(Math.Sqrt(num24 * num24 + num25 * num25) < num2 * 0.4 && Main.tile[num22, num23].type == TileID.Cloud){
								Main.tile[num22, num23].type = 0;
								WorldGen.SquareTileFrame(num22, num23);
							}
						}
					}
				}

				vector += vector2;
				vector2.X += WorldGen.genRand.Next(-20, 21) * 0.05f;
				if(vector2.X > 1f)
					vector2.X = 1f;

				if(vector2.X < -1f)
					vector2.X = -1f;

				if(vector2.Y > 0.2)
					vector2.Y = -0.2f;

				if(vector2.Y < -0.2)
					vector2.Y = -0.2f;
			}

			int num26 = num4;

			num26 += WorldGen.genRand.Next(5);

			while(num26 < num5){
				int num27 = num7;

				while((!Main.tile[num26, num27].active() || Main.tile[num26, num27].type != 0) && num26 < num5){
					num27--;
					if(num27 < num6){
						num27 = num7;
						num26 += WorldGen.genRand.Next(1, 4);
					}
				}

				if(num26 >= num5)
					continue;

				num27 += WorldGen.genRand.Next(0, 4);
				int num28 = WorldGen.genRand.Next(2, 5);
				int num29 = TileID.Cloud;

				for(int num30 = num26 - num28; num30 <= num26 + num28; num30++){
					for(int num31 = num27 - num28; num31 <= num27 + num28; num31++){
						if(num31 > num6){
							float num32 = Math.Abs(num30 - num26);
							float num33 = Math.Abs(num31 - num27) * 2;

							if(Math.Sqrt(num32 * num32 + num33 * num33) < num28) {
								Main.tile[num30, num31].type = (ushort)num29;
								WorldGen.SquareTileFrame(num30, num31);
							}
						}
					}
				}

				num26 += WorldGen.genRand.Next(num28, (int)(num28 * 1.5));
			}

			for(int num34 = num4 - 20; num34 <= num5 + 20; num34++){
				for(int num35 = num6 - 20; num35 <= num7 + 20; num35++){
					bool flag = true;

					for(int num36 = num34 - 1; num36 <= num34 + 1; num36++){
						for(int num37 = num35 - 1; num37 <= num35 + 1; num37++){
							if(!Main.tile[num36, num37].active())
								flag = false;
						}
					}

					if(flag){
						Main.tile[num34, num35].wall = WallID.Cloud;
						WorldGen.SquareWallFrame(num34, num35);
					}
				}
			}

			for(int num38 = num4; num38 <= num5; num38++){
				int num39;
				for(num39 = num6 - 10; !Main.tile[num38, num39 + 1].active(); num39++);

				if(num39 >= num7 || Main.tile[num38, num39 + 1].type != TileID.Cloud)
					continue;

				if(WorldGen.genRand.Next(10) == 0){
					int num40 = WorldGen.genRand.Next(1, 3);

					for(int num41 = num38 - num40; num41 <= num38 + num40; num41++){
						if(Main.tile[num41, num39].type == TileID.Cloud){
							Main.tile[num41, num39].active(active: false);
							Main.tile[num41, num39].liquid = byte.MaxValue;
							Main.tile[num41, num39].lava(lava: false);
							WorldGen.SquareTileFrame(num38, num39);
						}

						if(Main.tile[num41, num39 + 1].type == TileID.Cloud){
							Main.tile[num41, num39 + 1].active(active: false);
							Main.tile[num41, num39 + 1].liquid = byte.MaxValue;
							Main.tile[num41, num39 + 1].lava(lava: false);
							WorldGen.SquareTileFrame(num38, num39 + 1);
						}

						if(num41 > num38 - num40 && num41 < num38 + 2 && Main.tile[num41, num39 + 2].type == TileID.Cloud){
							Main.tile[num41, num39 + 2].active(active: false);
							Main.tile[num41, num39 + 2].liquid = byte.MaxValue;
							Main.tile[num41, num39 + 2].lava(lava: false);
							WorldGen.SquareTileFrame(num38, num39 + 2);
						}
					}
				}

				if(WorldGen.genRand.Next(5) == 0)
					Main.tile[num38, num39].liquid = byte.MaxValue;

				Main.tile[num38, num39].lava(lava: false);
				WorldGen.SquareTileFrame(num38, num39);
			}

			int num42 = WorldGen.genRand.Next(4);
			for(int num43 = 0; num43 <= num42; num43++){
				int num44 = WorldGen.genRand.Next(num4 - 5, num5 + 5);
				int num45 = num6 - WorldGen.genRand.Next(20, 40);
				int num46 = WorldGen.genRand.Next(4, 8);
				int num47 = TileID.Cloud;

				if(WorldGen.genRand.Next(2) == 0)
					num47 = TileID.SnowCloud;

				for(int num48 = num44 - num46; num48 <= num44 + num46; num48++){
					for(int num49 = num45 - num46; num49 <= num45 + num46; num49++){
						float num50 = Math.Abs(num48 - num44);
						float num51 = Math.Abs(num49 - num45) * 2;

						if(Math.Sqrt(num50 * num50 + num51 * num51) < num46 + WorldGen.genRand.Next(-1, 2)) {
							Main.tile[num48, num49].active(active: true);
							Main.tile[num48, num49].type = (ushort)num47;
							WorldGen.SquareTileFrame(num48, num49);
						}
					}
				}

				for(int num52 = num44 - num46 + 2; num52 <= num44 + num46 - 2; num52++){
					int num53;
					for(num53 = num45 - num46; !Main.tile[num52, num53].active(); num53++);

					Main.tile[num52, num53].active(active: false);
					Main.tile[num52, num53].liquid = byte.MaxValue;
					WorldGen.SquareTileFrame(num52, num53);
				}
			}
		}
	}
}
