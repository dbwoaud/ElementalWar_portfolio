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

    [Header("프로파일링 활성화 여부")]
    [SerializeField] private bool enableProfiling = true;

    [Header("최적화 항목")]
    [SerializeField] private bool usePooling = true;
    [SerializeField] private bool useNonAllocQueries = true;
    [SerializeField] private bool useScanThrottle = true;
    [SerializeField] private bool tickOnlyOwnedUnits = true;

    [Header("CSV 로그 태그")]
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
