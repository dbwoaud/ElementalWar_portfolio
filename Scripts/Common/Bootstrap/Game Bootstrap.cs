using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameBootstrap : MonoBehaviour
{
    [Header("전역 매니저 프리팹")]
    [SerializeField] private GameObject[] persistentManagerPrefabs;

    [Header("부트스트랩 후 진입할 씬")]
    [SerializeField] private string firstSceneName = SceneName.MainMenu;

    private static bool isBootstrapped;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() // 도메인 리로드 비활성화 시 정적 상태를 초기화하는 함수
    {
        isBootstrapped = false;
    }

    private void Awake()
    {
        if (isBootstrapped)
        {
            Destroy(gameObject);
            return;
        }
        isBootstrapped = true;

        SetRunInBackground();
        SpawnPersistentManagers();
        LoadFirstScene();
    }

    private static void SetRunInBackground() // 백그라운드 실행을 설정하는 함수
    {
        Application.runInBackground = true;
        PhotonNetwork.KeepAliveInBackground = 60000f;
    }

    private void SpawnPersistentManagers() // 전역 매니저를 생성하는 함수
    {
        if (persistentManagerPrefabs == null)
            return;

        foreach (GameObject prefab in persistentManagerPrefabs)
        {
            if (prefab == null)
                continue;

            GameObject managerObj = Instantiate(prefab);
            managerObj.name = prefab.name;
            DontDestroyOnLoad(managerObj);
        }
    }

    private void LoadFirstScene() // 부트스트랩 후 첫 씬으로 이동하는 함수
    {
        if (string.IsNullOrEmpty(firstSceneName))
            return;

        SceneManager.LoadScene(firstSceneName);
    }
}
