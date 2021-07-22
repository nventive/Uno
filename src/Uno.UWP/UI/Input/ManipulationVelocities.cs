using System;
using Windows.Foundation;

namespace Windows.UI.Input
{
	public partial struct ManipulationVelocities
	{
		internal static ManipulationVelocities Empty { get; } = new ManipulationVelocities();

		/// <summary>
		/// The expansion, or scaling, velocity in device-independent pixel (DIP) per millisecond.
		/// </summary>
		public Point Linear;

		/// <summary>
		/// The rotational velocity in degrees per millisecond.
		/// </summary>
		public float Angular;

		/// <summary>
		/// The expansion, or scaling, velocity in device-independent pixel (DIP) per millisecond.
		/// </summary>
		public float Expansion;

		// Note: We should apply a velocity factor to thresholds to determine if isSignificant
		internal bool IsAnyAbove(GestureRecognizer.Manipulation.Thresholds thresholds)
			=> Math.Abs(Linear.X) > thresholds.TranslateX
				|| Math.Abs(Linear.Y) > thresholds.TranslateY
				|| Angular > thresholds.Rotate // We used the ToDegreeNormalized, no need to check for negative angles
				|| Math.Abs(Expansion) > thresholds.Expansion;
	}
}
