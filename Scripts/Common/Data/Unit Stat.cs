using UnityEngine;

public enum ElementType { Void, Wind, Forest, Fire, Mountain }

[CreateAssetMenu(fileName = "UnitStat", menuName = "Scriptable Objects/UnitStat")]
public class UnitStat : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] public GameObject unitPrefab;
    [SerializeField] public Sprite unitIcon;
    [SerializeField] public string unitName;
    [SerializeField, TextArea(3,10)] public string unitDescription;
    [SerializeField] public ElementType elementType;
    [SerializeField] public int spawnCost;
    [SerializeField] public float spawnCoolTime;

    [Header("전투 능력치")]
    [SerializeField] public float maxHP;
    [SerializeField] public float attackDamage;
    [SerializeField] public float firstAttackDelay;
    [SerializeField] public float attackInterval;
    [SerializeField] public float attackRange;
    [SerializeField] public float moveSpeed;
    [SerializeField] public float aoeRadius;


    public float CalculateDamage(ElementType attacker, ElementType defender, float baseDamage) // 속성에 따른 데미지를 계산하는 함수
    {
        float multiplier = 1.0f;
        if (attacker == ElementType.Wind)
        {
            if (defender == ElementType.Forest) 
                multiplier = 0.75f;
            else if (defender == ElementType.Mountain) 
                multiplier = 1.5f;
        }
        else if (attacker == ElementType.Forest)
        {
            if (defender == ElementType.Fire) 
                multiplier = 0.75f;
            else if (defender == ElementType.Wind) 
                multiplier = 1.5f;
        }
        else if (attacker == ElementType.Fire)
        {
            if (defender == ElementType.Mountain) 
                multiplier = 0.75f;
            else if (defender == ElementType.Forest) 
                multiplier = 1.5f;
        }
        else if (attacker == ElementType.Mountain)
        {
            if (defender == ElementType.Wind) 
                multiplier = 0.75f;
            else if (defender == ElementType.Fire) 
                multiplier = 1.5f;
        }
        return baseDamage * multiplier;
    }
}