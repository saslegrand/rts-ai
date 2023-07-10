namespace RTS.Extensions
{

	public static class IntExtension
	{
		public static float Remap(this int value, int minFrom, int maxFrom, int minTo, int maxTo)
		{
			return minTo + ( value + minFrom ) * ( maxTo - minTo ) / (float)( maxFrom - minFrom );
		}

		public static bool Between(this int value, int min, int max)
		{
			return value >= min && value <= max;
		}
	}

}
