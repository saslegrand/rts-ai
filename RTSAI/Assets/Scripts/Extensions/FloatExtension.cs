using UnityEngine;


namespace RTS.Extensions
{
    public static class FloatExtension
    {
#region Remap Extension Methods
        public static float Remap(this float value, float minFrom, float maxFrom, float minTo, float maxTo)
        {
            return minTo + ( value - minFrom ) * ( maxTo - minTo ) / ( maxFrom - minFrom );
        }
        
        public static float Remap(this float value, float minFrom, float maxFrom, Vector2 to)
        {
            return value.Remap(minFrom, maxFrom, to.x, to.y);
        }
        
        public static float Remap(this float value, Vector2 from, float minTo, float maxTo)
        {
            return value.Remap(from.x, from.y, minTo, maxTo);
        }
        
        public static float Remap(this float value, Vector2 from, Vector2 to)
        {
            return value.Remap(from.x, from.y, to.x, to.y);
        }
#endregion

        public static bool Between(this float value, float min, float max)
        {
            return value >= min && value <= max;
        }
    }
}
