using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Photon.Pun;
using Unity.Profiling;
using UnityEngine;

public class NetworkPerformanceLogger : MonoBehaviour
{
    [Header("샘플링 설정")]
    [SerializeField] private float sampleInterval = 1f;
    [SerializeField] private string scenarioName = "unnamed";
    public string ScenarioName { get => scenarioName; set => scenarioName = value; }

    [Header("상태")]
    [SerializeField] private bool isLogging;
    public bool IsLogging => isLogging;

    private ProfilerRecorder gcAllocRecorder;

    private readonly List<string> rows = new List<string>(512);
    private readonly StringBuilder lineBuilder = new StringBuilder(256);

    // 샘플 구간 누적치
    private float intervalTimer;
    private int frameCount;
    private float frameTimeSum;
    private float frameTimeMax;
    private long gcAllocSum;

    // 구간 시작 시점 스냅샷 (델타 계산용)
    private int gcCollectAtStart;
    private long rpcSentAtStart;
    private long physicsQueriesAtStart;
    private long unitTicksAtStart;
    private long bytesOutAtStart;
    private long bytesInAtStart;

    private float sessionStartTime;

    private const string CsvHeader =
        "elapsed_s,scenario,variant,is_master," +
        "frame_time_avg_ms,frame_time_max_ms,fps_avg," +
        "gc_alloc_per_frame_kb,gc_collect_count," +
        "active_units,rpc_sent,physics_queries,unit_ticks," +
        "bytes_out,bytes_in,rtt_ms,rtt_variance_ms,resent_reliable";


    private void OnEnable()
    {
        gcAllocRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
    }

    private void OnDisable()
    {
        if (gcAllocRecorder.Valid)
            gcAllocRecorder.Dispose();
    }

    public void StartLogging(string scenario) // 계측을 시작하는 함수
    {
        scenarioName = scenario;
        rows.Clear();
        rows.Add(CsvHeader);

        PhotonNetwork.NetworkStatisticsEnabled = true;

        ProfilingCounters.ResetAll();
        sessionStartTime = Time.realtimeSinceStartup;
        ResetIntervalAccumulators();

        isLogging = true;
        Debug.Log($"[Profiler] 계측 시작: scenario={scenario} / variant={ProfilingSwitches.VariantName}");
    }

    public string StopLoggingAndExport() // 계측을 종료하고 CSV 로 저장하는 함수
    {
        if (!isLogging)
            return null;

        isLogging = false;

        string fileName = $"profile_{scenarioName}_{ProfilingSwitches.VariantName}_" +
                          $"{(PhotonNetwork.IsMasterClient ? "master" : "guest")}_" +
                          $"{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        string path = Path.Combine(Application.persistentDataPath, fileName);

        File.WriteAllLines(path, rows, Encoding.UTF8);
        Debug.Log($"[Profiler] 계측 종료. {rows.Count - 1}행 저장 → {path}");
        return path;
    }

    private void Update()
    {
        if (!isLogging)
            return;

        AccumulateFrame();

        intervalTimer += Time.unscaledDeltaTime;
        if (intervalTimer < sampleInterval)
            return;

        AppendRow();
        ResetIntervalAccumulators();
    }

    private void AccumulateFrame() // 프레임 단위 지표를 누적하는 함수
    {
        float frameMs = Time.unscaledDeltaTime * 1000f;

        frameCount++;
        frameTimeSum += frameMs;
        if (frameMs > frameTimeMax)
            frameTimeMax = frameMs;

        if (gcAllocRecorder.Valid)
            gcAllocSum += gcAllocRecorder.LastValue;
    }

    private void AppendRow() // 한 샘플 구간의 결과를 CSV 행으로 추가하는 함수
    {
        float elapsed = Time.realtimeSinceStartup - sessionStartTime;
        int safeFrameCount = Mathf.Max(frameCount, 1);

        float frameAvgMs = frameTimeSum / safeFrameCount;
        float fpsAvg = frameAvgMs > 0f ? 1000f / frameAvgMs : 0f;
        float gcPerFrameKb = gcAllocSum / (float)safeFrameCount / 1024f;

        int gcCollectDelta = GC.CollectionCount(0) - gcCollectAtStart;
        long rpcDelta = ProfilingCounters.RpcSent - rpcSentAtStart;
        long physicsDelta = ProfilingCounters.PhysicsQueries - physicsQueriesAtStart;
        long tickDelta = ProfilingCounters.UnitTicks - unitTicksAtStart;

        ReadNetworkStats(out long bytesOut, out long bytesIn,
                         out int rtt, out int rttVariance, out int resent);

        lineBuilder.Clear();
        Append(elapsed, 2);
        Append(scenarioName);
        Append(ProfilingSwitches.VariantName);
        Append(PhotonNetwork.IsMasterClient ? 1 : 0);
        Append(frameAvgMs, 3);
        Append(frameTimeMax, 3);
        Append(fpsAvg, 1);
        Append(gcPerFrameKb, 3);
        Append(gcCollectDelta);
        Append(UnitRegistry.ActiveUnits.Count);
        Append(rpcDelta);
        Append(physicsDelta);
        Append(tickDelta);
        Append(bytesOut - bytesOutAtStart);
        Append(bytesIn - bytesInAtStart);
        Append(rtt);
        Append(rttVariance);
        Append(resent, last: true);

        rows.Add(lineBuilder.ToString());
    }

    private void ReadNetworkStats(out long bytesOut, out long bytesIn,
                                  out int rtt, out int rttVariance, out int resent)
    {
        bytesOut = 0; bytesIn = 0; rtt = 0; rttVariance = 0; resent = 0;

        try
        {
            var peer = PhotonNetwork.NetworkingClient?.LoadBalancingPeer;
            if (peer == null)
                return;

            rtt = peer.RoundTripTime;
            rttVariance = peer.RoundTripTimeVariance;
            resent = peer.ResentReliableCommands;

            if (peer.TrafficStatsOutgoing != null)
                bytesOut = peer.TrafficStatsOutgoing.TotalPacketBytes;
            if (peer.TrafficStatsIncoming != null)
                bytesIn = peer.TrafficStatsIncoming.TotalPacketBytes;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Profiler] 네트워크 통계 수집 실패: {e.Message}");
        }
    }

    private void ResetIntervalAccumulators() // 샘플 구간 누적치를 초기화하는 함수
    {
        intervalTimer = 0f;
        frameCount = 0;
        frameTimeSum = 0f;
        frameTimeMax = 0f;
        gcAllocSum = 0;

        gcCollectAtStart = GC.CollectionCount(0);
        rpcSentAtStart = ProfilingCounters.RpcSent;
        physicsQueriesAtStart = ProfilingCounters.PhysicsQueries;
        unitTicksAtStart = ProfilingCounters.UnitTicks;

        ReadNetworkStats(out bytesOutAtStart, out bytesInAtStart, out _, out _, out _);
    }

    private void Append(float value, int digits, bool last = false)
        => Append(value.ToString($"F{digits}", CultureInfo.InvariantCulture), last);

    private void Append(long value, bool last = false)
        => Append(value.ToString(CultureInfo.InvariantCulture), last);

    private void Append(string value, bool last = false)
    {
        lineBuilder.Append(value);
        if (!last)
            lineBuilder.Append(',');
    }
}
