using UnityEngine;

public class MapData : MonoBehaviour
{
    [Header("맵 메타데이터")]
    [SerializeField] private ElementType mapType;
    [SerializeField] private AudioClip mapBGM;

    [Header("스폰 포인트 설정")]
    [SerializeField] private Transform player1CastlePoint;
    [SerializeField] private Transform player2CastlePoint;

    [Header("카메라 및 지형 설정")]
    [SerializeField] private PolygonCollider2D cameraBounds;
    [SerializeField] private BoxCollider2D groundCollider;

    [Header("편의 프로퍼티")]
    public ElementType MapType => mapType;
    public AudioClip MapBGM => mapBGM;
    public Transform Player1CastlePoint => player1CastlePoint;
    public Transform Player2CastlePoint => player2CastlePoint;
    public PolygonCollider2D CameraBounds => cameraBounds;
    public BoxCollider2D GroundCollider => groundCollider;
}
