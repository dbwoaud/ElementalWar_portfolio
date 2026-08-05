using System.Diagnostics;

public static class ProfilingCounters
{
    public static long RpcSent;              // 이 클라이언트가 보낸 RPC 수
    public static long RpcReceivedDamage;    // 수신한 피해 RPC 수
    public static long PhysicsQueries;       // Physics2D 질의 호출 수
    public static long UnitTicks;            // 상태 머신 Tick 호출 수

    [Conditional("ENABLE_PROFILING")]
    public static void CountRpcSent() => RpcSent++;

    [Conditional("ENABLE_PROFILING")]
    public static void CountRpcReceivedDamage() => RpcReceivedDamage++;

    [Conditional("ENABLE_PROFILING")]
    public static void CountPhysicsQuery() => PhysicsQueries++;

    [Conditional("ENABLE_PROFILING")]
    public static void CountUnitTick() => UnitTicks++;

    public static void ResetAll()
    {
        RpcSent = 0;
        RpcReceivedDamage = 0;
        PhysicsQueries = 0;
        UnitTicks = 0;
    }
}
