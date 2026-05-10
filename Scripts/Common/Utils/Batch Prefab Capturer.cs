using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BatchPrefabCapturer : MonoBehaviour
{
    [Header("카메라 세팅")]
    public Camera captureCamera;

    [Header("캡처할 프리팹 목록")]
    public List<GameObject> targetPrefabs;

    [Header("이미지 세팅")]
    public int imageSize = 512;
    [Range(1f, 2f)]
    public float padding = 1.2f; // 1.2면 상하좌우 약 20%의 여백 생성
    
    
    [ContextMenu("지정된 모든 프리팹 촬영하기!")]
    public void CaptureAllPrefabs()
    {
        if (captureCamera == null || targetPrefabs.Count == 0)
        {
            Debug.LogError("카메라 또는 캡처할 프리팹이 설정되지 않았습니다!");
            return;
        }

        string saveDirectory = Application.dataPath + "/Project/Resources/Character Icon";
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory); 
        }

        foreach (GameObject prefab in targetPrefabs)
        {
            if (prefab == null) 
                continue;

            GameObject instance = Instantiate(prefab, Vector3.zero, Quaternion.identity);

            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                DestroyImmediate(instance);
                continue;
            }

            Bounds bounds = renderers[0].bounds;
            foreach (Renderer r in renderers)
            {
                bounds.Encapsulate(r.bounds); 
            }

            captureCamera.transform.position = new Vector3(bounds.center.x, bounds.center.y, -10f);

            float maxExtent = Mathf.Max(bounds.extents.x, bounds.extents.y);
            captureCamera.orthographicSize = maxExtent * padding;

            RenderTexture rt = new RenderTexture(imageSize, imageSize, 24);
            captureCamera.targetTexture = rt;
            Texture2D screenShot = new Texture2D(imageSize, imageSize, TextureFormat.RGBA32, false);

            captureCamera.Render();

            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, imageSize, imageSize), 0, 0);

            captureCamera.targetTexture = null;
            RenderTexture.active = null;
            DestroyImmediate(rt);
            DestroyImmediate(instance);

            byte[] bytes = screenShot.EncodeToPNG();
            string fileName = prefab.name + "_Icon.png";
            string fullPath = Path.Combine(saveDirectory, fileName);
            File.WriteAllBytes(fullPath, bytes);

            Debug.Log($"[저장 완료] {fileName} (Size: {captureCamera.orthographicSize})");
        }
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
        Debug.Log("모든 프리팹 캡처가 성공적으로 완료되었습니다! (Assets/UnitIcons 폴더 확인)");
    }
}