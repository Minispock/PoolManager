#define USE_LOGS

using System.Diagnostics;

public static class Trace
{
    [Conditional("USE_LOGS")]
    public static void Message ( object message )
    {
        UnityEngine.Debug.Log( message );
    }

    [Conditional("USE_LOGS")]
    public static void Warning ( object message )
    {
        UnityEngine.Debug.LogWarning( message );
    }

    [Conditional("USE_LOGS")]
    public static void Error ( object message )
    {
        UnityEngine.Debug.LogError( message );
    }
}