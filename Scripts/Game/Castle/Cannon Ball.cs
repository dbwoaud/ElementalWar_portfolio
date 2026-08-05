using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;


public class Cannonball : MonoBehaviour
{
    [Header("네트워크 설정")]
    [SerializeField] private PhotonView castleView;
    [SerializeField] private float hpDamagePercent = 0.5f;
    [SerializeField] private bool hasDetonated;
    private static readonly List<Unit> unitBuffer = new List<Unit>(64);

    public void Init(PhotonView ownerCastleView) // 대포를 발사한 로컬 플레이어를 설정하는 함수
    {
        castleView = ownerCastleView;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasDetonated) 
            return;

        if (!collision.CompareTag(GameSystem.Ground.ColliderTag)) 
            return;

        hasDetonated = true;
        if (castleView != null)
        {
            Vector2 hitPoint = collision.ClosestPoint(transform.position);
            castleView.RPC(nameof(Castle.RPC_ShowExplosionEffect), RpcTarget.All, hitPoint);
            ApplyNetworkDamage();
        }
            
        Destroy(gameObject);
    }

    private void ApplyNetworkDamage() // 적 유닛에게 데미지를 적용하는 함수
    {
        UnitRegistry.CopyTo(unitBuffer);
        for (int i = 0; i < unitBuffer.Count; i++)
        {
            Unit unit = unitBuffer[i];
            if (unit == null || !unit.IsTargetable)
                continue;

            PhotonView targetView = unit.GetComponent<PhotonView>();
            if (targetView == null)
                continue;

            if (targetView.IsMine == castleView.IsMine)
                continue;

            float damage = unit.MaxHP * hpDamagePercent;
            targetView.RPC(nameof(Unit.RPC_TakeDamage), RpcTarget.All, damage);
        }
    }
}