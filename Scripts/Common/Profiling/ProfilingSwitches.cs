using UnityEngine;

public class ProfilingSwitches : MonoBehaviour
{
    public static bool IsProfilingBuild { get; private set; }
    public static bool UsePooling { get; private set; } = true;
    public static bool UseNonAllocQueries { get; private set; } = true;
    public static bool UseScanThrottle { get; private set; } = true;
    public static bool TickOnlyOwnedUnits { get; private set; } = true;

    public static float ScanInterval => UseScanThrottle ? DefaultScanInterval : 0f;

    public const float DefaultScanInterval = 0.1f;

    [Header("계측 활성화")]
    [SerializeField] private bool enableProfiling = true;

    [Header("최적화 항목 (해제 시 before 측정)")]
    [SerializeField] private bool usePooling = true;
    [SerializeField] private bool useNonAllocQueries = true;
    [SerializeField] private bool useScanThrottle = true;
    [SerializeField] private bool tickOnlyOwnedUnits = true;

    [Header("변형 이름 (CSV 태그)")]
    [SerializeField] private string variantName = "after";
    public static string VariantName { get; private set; } = "after";


    private void Awake()
    {
        IsProfilingBuild = enableProfiling;
        UsePooling = usePooling;
        UseNonAllocQueries = useNonAllocQueries;
        UseScanThrottle = useScanThrottle;
        TickOnlyOwnedUnits = tickOnlyOwnedUnits;
        VariantName = string.IsNullOrWhiteSpace(variantName) ? "unnamed" : variantName;
    }
}
