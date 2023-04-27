using System;
using Terraria.Utilities;

namespace CosmivengeonMod.Utility {
	partial class Extensions {
		public static void AddExcess<T>(this WeightedRandom<T> rand, T element, double totalChanceAfterAdd) {
			double total = CalculateTotalWeight(rand);

			if (total >= totalChanceAfterAdd)
				throw new InvalidOperationException($"WeightedRandom has already met the total chance requirement ({total} >= {totalChanceAfterAdd})");

			rand.Add(element, totalChanceAfterAdd - total);
		}

		private static double CalculateTotalWeight<T>(WeightedRandom<T> rand) {
			double total = 0.0;

			foreach (Tuple<T, double> element in rand.elements)
				total += element.Item2;

			return total;
		}
	}
}
