using UnityEngine;

public enum ElementType { Void, Wind, Forest, Fire, Mountain }

[CreateAssetMenu(fileName = "UnitStat", menuName = "Scriptable Objects/UnitStat")]
public class UnitStat : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private Sprite unitIcon;
    [SerializeField] private string unitName;
    [SerializeField, TextArea(3,10)] private string unitDescription;
    [SerializeField] private ElementType elementType;
    [SerializeField] private int spawnCost;
    [SerializeField] private float spawnCoolTime;

    [Header("전투 능력치")]
    [SerializeField] private float maxHP;
    [SerializeField] private float attackDamage;
    [SerializeField] private float firstAttackDelay;
    [SerializeField] private float attackInterval;
    [SerializeField] private float attackRange;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float aoeRadius;

    public GameObject UnitPrefab => unitPrefab;
    public Sprite UnitIcon => unitIcon;
    public string UnitName => unitName;
    public string UnitDescription => unitDescription;
    public ElementType ElementType => elementType;
    public int SpawnCost => spawnCost;
    public float SpawnCoolTime => spawnCoolTime;

    public float MaxHP => maxHP;
    public float AttackDamage => attackDamage;
    public float FirstAttackDelay => firstAttackDelay;
    public float AttackInterval => attackInterval;
    public float AttackRange => attackRange;
    public float MoveSpeed => moveSpeed;
    public float AoeRadius => aoeRadius;

    private static readonly float[,] ElementMultiplier =
    {
        /*          공허   풍    림    화    산 */
        /* 공허 */ { 1f,  1f,   1f,   1f,   1f   },
        /*  풍  */ { 1f,  1f,   0.75f,1f,   1.5f },
        /*  림  */ { 1f,  1.5f, 1f,   0.75f,1f   },
        /*  화  */ { 1f,  1f,   1.5f, 1f,   0.75f},
        /*  산  */ { 1f,  0.75f,1f,   1.5f, 1f   },
    };


    public static float CalculateDamage(ElementType attacker, ElementType defender, float baseDamage) // 속성에 따른 데미지를 계산하는 함수
    {
        return baseDamage * ElementMultiplier[(int)attacker, (int)defender];
    }
}