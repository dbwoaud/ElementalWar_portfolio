using UnityEngine;

public class ProfilingSwitches : MonoBehaviour
{
#if ENABLE_PROFILING
    public const float DefaultScanInterval = 0.1f;

    public static bool IsProfilingBuild { get; private set; }
    public static bool UsePooling { get; private set; } = true;
    public static bool UseNonAllocQueries { get; private set; } = true;
    public static bool UseScanThrottle { get; private set; } = true;
    public static bool TickOnlyOwnedUnits { get; private set; } = true;
    public static string VariantName { get; private set; } = "after";

    public static float ScanInterval => UseScanThrottle ? DefaultScanInterval : 0f;

    [Header("프로파일링 활성화 여부")]
    [SerializeField] private bool enableProfiling = true;

    [Header("최적화 항목")]
    [SerializeField] private bool usePooling = true;
    [SerializeField] private bool useNonAllocQueries = true;
    [SerializeField] private bool useScanThrottle = true;
    [SerializeField] private bool tickOnlyOwnedUnits = true;

    [Header("CSV 로그 태그")]
    [SerializeField] private string variantName = "after";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() // 도메인 리로드 비활성화 시 정적 값을 초기화하는 함수
    {
        IsProfilingBuild = false;
        UsePooling = true;
        UseNonAllocQueries = true;
        UseScanThrottle = true;
        TickOnlyOwnedUnits = true;
        VariantName = "after";
    }

    private void Awake()
    {
        IsProfilingBuild = enableProfiling;
        UsePooling = usePooling;
        UseNonAllocQueries = useNonAllocQueries;
        UseScanThrottle = useScanThrottle;
        TickOnlyOwnedUnits = tickOnlyOwnedUnits;
        VariantName = string.IsNullOrWhiteSpace(variantName) ? "unnamed" : variantName;
    }

#else
    public const float DefaultScanInterval = 0.1f;

    public const bool IsProfilingBuild = false;
    public const bool UsePooling = true;
    public const bool UseNonAllocQueries = true;
    public const bool UseScanThrottle = true;
    public const bool TickOnlyOwnedUnits = true;
    public const float ScanInterval = DefaultScanInterval;
    public const string VariantName = "release";

#endif
}