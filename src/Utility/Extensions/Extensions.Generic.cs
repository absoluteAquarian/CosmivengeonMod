using System;
using Terraria;

namespace CosmivengeonMod.Utility {
	public static partial class Extensions {
		/// <summary>
		/// A simplified alternative to clamping and re-assigning a value
		/// </summary>
		/// <typeparam name="T">The type of the value being set.</typeparam>
		/// <param name="value">The value being clamped.</param>
		/// <param name="min">The minimum value to be clamped to.</param>
		/// <param name="max">The maximum value to be clamped to.</param>
		public static void Clamp<T>(this ref T value, T min, T max) where T : struct, IComparable<T>
			=> value = Utils.Clamp(value, min, max);
	}
}
