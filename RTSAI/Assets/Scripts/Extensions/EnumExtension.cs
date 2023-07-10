namespace RTS.Extensions
{
    public static class EnumExtension
    {
        public static int GetCount<T>() where T : System.Enum
        {
            return System.Enum.GetValues(typeof(T)).Length;
        }
    }
}