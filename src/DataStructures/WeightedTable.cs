using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Terraria.Utilities;

namespace CosmivengeonMod.DataStructures {
	public class WeightedTable<T> {
		public readonly struct Entry {
			public readonly T value;
			public readonly double chance;

			public Entry(T value, double chance) {
				this.value = value;
				this.chance = chance;
			}
		}

		private readonly List<Entry> entries = new();
		private bool refreshTotalWeight;

		public readonly UnifiedRandom rand;

		public IReadOnlyList<Entry> Entries => entries.AsReadOnly();

		public double TotalWeight { get; private set; }

		public WeightedTable(UnifiedRandom rand) {
			this.rand = rand;
		}

		public WeightedTable<T> Add(T value, double chance = 1.0) {
			entries.Add(new Entry(value, chance));
			refreshTotalWeight = true;
			return this;
		}

		public WeightedTable<T> AddExcess(T value, double totalWeightToReach) {
			if (TotalWeight >= totalWeightToReach)
				throw new InvalidOperationException($"WeightedTable has already met the total chance requirement ({TotalWeight} >= {totalWeightToReach})");

			return Add(value, totalWeightToReach - TotalWeight);
		}

		public void Remove(int index) => entries.RemoveAt(index);

		public void RemoveAll(Func<Entry, bool> predicate) => entries.RemoveAll(new Predicate<Entry>(predicate));

		public T Get() {
			if (refreshTotalWeight)
				CalculateTotalWeight();

			double choice = rand.NextDouble();
			choice *= TotalWeight;

			foreach (Entry element in entries) {
				if (choice > element.chance) {
					choice -= element.chance;
					continue;
				}

				return element.value;
			}

			return default;
		}

		private void CalculateTotalWeight() {
			double total = 0.0;

			foreach (var entry in entries)
				total += entry.chance;

			TotalWeight = total;
			refreshTotalWeight = false;
		}
	}
}
