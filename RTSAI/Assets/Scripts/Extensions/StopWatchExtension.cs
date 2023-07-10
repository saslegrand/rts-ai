using Debug = UnityEngine.Debug;
using System.Diagnostics;

public static class StopWatchExtension
{
    public static void PrintTimeSeconds(this Stopwatch watch, string details)
    {
        Debug.Log(details + $" - Time: { watch.Elapsed.Seconds }s");
    }
    
    public static void PrintTimeMilliseconds(this Stopwatch watch, string details)
    {
        Debug.Log(details + $" - Time: { watch.ElapsedMilliseconds }ms");
    }
    
    public static void PrintTimeTicks(this Stopwatch watch, string details)
    {
        Debug.Log(details + $" - Time: { watch.Elapsed.Ticks }ticks");
    }
    
    public static void PrintTimeDay(this Stopwatch watch, string details)
    {
        Debug.Log(details + $" - Time: { watch.Elapsed.Days }d");
    }
}
