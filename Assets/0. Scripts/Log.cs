using System.Diagnostics;

public enum LogLevel { None, Error, Warn, Info }

public static class Log
{
    static LogLevel currentLevel;
    public static LogLevel CurrentLevel => currentLevel;

    // 정적 생성자: 프로그램 시작 시 한 번만 실행
    static Log()
    {
#if UNITY_EDITOR
        currentLevel = LogLevel.Info;   // 에디터(디버그) 환경
#elif DEVELOPMENT_BUILD
        currentLevel = LogLevel.Warn;   // 개발 빌드 (Debug 빌드)
#else
        currentLevel = LogLevel.Error;  // 릴리즈 빌드
#endif
        UnityEngine.Debug.Log($"[Log] Initialized with level: {currentLevel}");
    }

    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void Info(string msg)
    {
        if (currentLevel >= LogLevel.Info)
            UnityEngine.Debug.Log(msg);
    }

    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void Warn(string msg)
    {
        if (currentLevel >= LogLevel.Warn)
            UnityEngine.Debug.LogWarning(msg);
    }

    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void Error(string msg)
    {
        if (currentLevel >= LogLevel.Error)
            UnityEngine.Debug.LogError(msg);
    }
}
