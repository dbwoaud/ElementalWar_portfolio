using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera), typeof(CameraInputManager), typeof(CameraBoundManager))]
public class CameraController : MonoBehaviour
{
    [Header("캐싱 변수")]
    [SerializeField] private Camera cam;
    [SerializeField] private CameraInputManager cameraInputManager;
    [SerializeField] private CameraBoundManager cameraBoundManager;

    [Header("스크롤 설정")]
    [SerializeField] private float scrollSpeed = 20f;

    [Header("코루틴 제어")]
    private Coroutine moveCoroutine;


    private void Awake()
    {
        cam = GetComponent<Camera>();
        cameraInputManager = GetComponent<CameraInputManager>();
        cameraBoundManager = GetComponent<CameraBoundManager>();
        if(cameraInputManager != null)
        {
            cameraInputManager.OnScrollInput += HandleEdgeScroll;
        }
    }

    private void HandleEdgeScroll(float direction) // 카메라 이동을 처리하는 함수
    {
        if (!cameraBoundManager.IsInitialized)
            return;

        if (moveCoroutine != null)
            return;

        Vector3 pos = transform.position;
        pos.x += direction * scrollSpeed * Time.deltaTime;
        pos.x = cameraBoundManager.ClampX(pos.x);
        transform.position = pos;
    }


    private void OnDestroy()
    {
        if (cameraInputManager != null)
        {
            cameraInputManager.OnScrollInput -= HandleEdgeScroll;
        }
    }

    public void SetBounds(PolygonCollider2D bounds) // 카메라의 경계를 설정하는 함수
    {
        cameraBoundManager?.SetBounds(cam, bounds);
    }

    public void EnablePlayerControl() // 플레이어의 제어를 활성화하는 함수
    {
        cameraInputManager.IsInputEnabled = true;
    }

    public void DisablePlayerControl() // 플레이어의 제어를 비활성화하는 함수
    {
        cameraInputManager.IsInputEnabled = false;
    }

    public void MoveToTarget(Vector3 targetWorldPos, float duration) // 카메라를 정해진 위치로 이동시키는 함수
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveToTargetCoroutine(targetWorldPos, duration));
    }

    private IEnumerator MoveToTargetCoroutine(Vector3 targetWorldPos, float duration) // 카메라를 정해진 위치로 이동시키는 코루틴
    {
        DisablePlayerControl();

        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3
        (
            cameraBoundManager.ClampX(targetWorldPos.x),
            transform.position.y,
            transform.position.z
        );

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        transform.position = endPos;
        moveCoroutine = null;

        EnablePlayerControl();
    }
}