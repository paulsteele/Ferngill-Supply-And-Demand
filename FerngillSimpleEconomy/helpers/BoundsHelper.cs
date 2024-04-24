using System;

namespace fse.core.helpers;

public static class BoundsHelper
{
	public static int EnsureBounds(int input, int min, int max) => Math.Min(Math.Max(input, min), max);
}