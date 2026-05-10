using UnityEngine;

public class CameraBoundManager : MonoBehaviour
{
    [Header("카메라 경계 설정 변수")]
    public float MinX { get; private set; }
    public float MaxX { get; private set; }
    public bool IsInitialized { get; private set; }


    public void SetBounds(Camera cam, PolygonCollider2D boundsCollider) // 카메라의 경계를 설정하는 함수
    {
        if (cam == null || boundsCollider == null)
            return;

        float cameraWidth = cam.orthographicSize * cam.aspect;
        float boundsMinX = boundsCollider.bounds.min.x;
        float boundsMaxX = boundsCollider.bounds.max.x;

        MinX = boundsMinX + cameraWidth;
        MaxX = boundsMaxX - cameraWidth;

        if (MinX > MaxX)
            MinX = MaxX = (boundsMinX + boundsMaxX) / 2f;

        IsInitialized = true;
    }

    public float ClampX(float currentX) // 현재 x값을 경계 내로 제한하는 함수
    {
        if (!IsInitialized)
            return currentX;

        return Mathf.Clamp(currentX, MinX, MaxX);
    }
}
