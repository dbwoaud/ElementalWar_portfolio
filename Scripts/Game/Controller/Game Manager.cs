using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;


public class GameManager : BaseSceneController<GameManager>
{
    [Header("캐싱 변수")]
    [SerializeField] private GameUIManager gameUIManager;
    [SerializeField] private GameNetworkManager gameNetworkManager;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private EnergyManager energyManager;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private DeckHotkeyHandler deckHotkeyHandler;
    [SerializeField] private UnitDatabase unitDatabase;

    [Header("덱 설정 변수")]
    [SerializeField] private DeckModel deckModel;

    [Header("게임 상태 변수")]
    [SerializeField] private GameStateModel gameState;
    [SerializeField] private Vector3? destroyedCastlePosition;

    [Header("소환 설정")]
    [SerializeField] private Transform myCastleSpawnPoint;
    [SerializeField] private Transform myUnitSpawnPoint;


    protected override void SetCachedVariable() // 캐싱 변수를 설정하는 함수
    {
        deckModel = new DeckModel();
        gameState = new GameStateModel();
        base.SetCachedVariable();
    }

    protected override void SubscribeEvents() // 이벤트를 구독하는 함수
    {
        base.SubscribeEvents();
        Castle.OnAnyCastleDestroyed += HandleGameResult;
        if (gameState != null)
        {
            gameState.OnGameOver += HandleGameOverState;
        }

        if (deckHotkeyHandler != null)
        {
            deckHotkeyHandler.OnSlotHotkeyPressed += HandleHotkeyUnitSpawn;
        }

        if (MapManager.Instance != null)
        {
            mapManager = MapManager.Instance;
            mapManager.OnMapSetupCompleted += HandleMapSetupCompleted;
            mapManager.OnLoadProgress += HandleMapLoadProgress;
        }

        if (EnergyManager.Instance != null)
        {
            energyManager = EnergyManager.Instance;
            energyManager.OnEnergyChanged += HandleEnergyChanged;
        }
    }

    protected override void SetUIManager() // UI 매니저를 설정하는 함수
    {
        if (GameUIManager.Instance != null)
        {
            gameUIManager = GameUIManager.Instance;
            gameUIManager.ShowGameLoadingPanel();
            gameUIManager.OnReturnToRoomRequested += HandleReturnToRoomRequest;
            gameUIManager.OnReturnToLobbyRequested += HandleReturnToLobbyRequest;
            gameUIManager.OnUnitSlotClicked += HandleUnitSpawnRequest;
        } 
    }

    protected override void SetNetworkManager() // 네트워크 매니저를 설정하는 함수
    {
        if (gameNetworkManager != null)
        {
            gameNetworkManager.OnMapIndexSet += HandleMapSelected;
            gameNetworkManager.OnOpponentLeftRoom += HandleOpponentLeft;
            gameNetworkManager.OnLeftRoomSuccess += HandleLeftRoomForLobby;
            gameNetworkManager.OnReturnToRoomRequested += HandleExecuteReturnToRoom;
        }
    }

    protected override void PlayBGM() // 씬의 배경음악을 재생하는 함수
    {
        SoundManager.Instance?.StopAll();
    }

    protected override void InitializeState() // 씬의 초기상태를 설정하는 함수
    {
        UnitRegistry.Clear();
        RegisterAllUnitToNetworkPool();
        LoadDeckFromNetwork();
    }

    protected override void UnsubscribeAll() // 모든 이벤트를 구독 해제 하는 함수
    {
        base.UnsubscribeAll();
        Castle.OnAnyCastleDestroyed -= HandleGameResult;
        if (gameState != null)
        {
            gameState.OnGameOver -= HandleGameOverState;
        }

        if (deckHotkeyHandler != null)
        {
            deckHotkeyHandler.OnSlotHotkeyPressed -= HandleHotkeyUnitSpawn;
        }

        if (mapManager != null)
        {
            mapManager.OnMapSetupCompleted -= HandleMapSetupCompleted;
            mapManager.OnLoadProgress -= HandleMapLoadProgress;
        }

        if (energyManager != null)
        {
            energyManager.OnEnergyChanged -= HandleEnergyChanged;
        }

        UnitRegistry.Clear();
    }

    protected override void ResetUIManager() // UI 매니저를 리셋하는 함수
    {
        if (gameUIManager != null)
        {
            gameUIManager.OnReturnToRoomRequested -= HandleReturnToRoomRequest;
            gameUIManager.OnReturnToLobbyRequested -= HandleReturnToLobbyRequest;
            gameUIManager.OnUnitSlotClicked -= HandleUnitSpawnRequest;
        }
    }

    protected override void ResetNetworkManager() // 네트워크 매니저를 리셋하는 함수
    {
        if (gameNetworkManager != null)
        {
            gameNetworkManager.OnMapIndexSet -= HandleMapSelected;
            gameNetworkManager.OnOpponentLeftRoom -= HandleOpponentLeft;
            gameNetworkManager.OnLeftRoomSuccess -= HandleLeftRoomForLobby;
            gameNetworkManager.OnReturnToRoomRequested -= HandleExecuteReturnToRoom;
        }
    }

    private void HandleGameResult(bool localPlayerLost, Vector3 castlePos) // 게임 결과를 처리하는 함수
    {
        destroyedCastlePosition = castlePos;
        gameState.DeclareGameOver(!localPlayerLost);
    }

    private void HandleGameOverState(bool localPlayerWon) // 게임 종료 상태를 처리하는 함수
    {
        if (deckHotkeyHandler != null)
            deckHotkeyHandler.IsEnabled = false;

        StartCoroutine(GameOverSequence(localPlayerWon));
    }

    private void HandleHotkeyUnitSpawn(int slotIndex) // 단축키로 유닛 소환을 처리하는 함수
    {
        if (gameState.IsGameOver)
            return;

        UnitStat unit = deckModel.GetUnit(slotIndex);
        if (unit == null)
            return;

        HandleUnitSpawnRequest(slotIndex, unit);
    }

    private void HandleUnitSpawnRequest(int slotIndex, UnitStat spawnUnitStat) // 유닛 소환 요청을 처리하는 함수
    {
        if (gameState.IsGameOver)
            return;

        if (spawnUnitStat == null)
            return;

        if (!CanSpawnFromSlot(slotIndex))
            return;

        SetUnitSpawnPoint();

        if (myUnitSpawnPoint == null)
            return;

        if (!CheckEnergyToSpawn(spawnUnitStat))
            return;

        SpawnUnit(spawnUnitStat);
        gameUIManager?.StartSlotCoolTime(slotIndex);
    }

    private IEnumerator GameOverSequence(bool localPlayerWon) // 게임 종료 연출을 수행하는 코루틴 
    {
        cameraController?.DisablePlayerControl();
        if (cameraController != null && destroyedCastlePosition.HasValue)
        {
            cameraController.MoveToTarget(destroyedCastlePosition.Value, 2f);
            yield return new WaitForSeconds(2f);
        }
        else
            yield return new WaitForSeconds(0.5f);

        yield return new WaitForSeconds(0.5f);

        PauseAllGameSystems();

        if (localPlayerWon)
            SoundManager.Instance?.Play(SoundKey.GameWin);
        else
            SoundManager.Instance?.Play(SoundKey.GameLose);

        gameUIManager?.ShowGameResultPanel(localPlayerWon, PhotonNetwork.NickName);
    }

    private void PauseAllGameSystems() // 모든 게임 시스템을 중지시키는 함수
    {
        Time.timeScale = 0f;
        SoundManager.Instance?.StopAll();
        energyManager?.StopEnergySystem();
        CastleAttackManager.Instance?.StopAttackSystem();
    }

    private void HandleMapSetupCompleted(MapData spawnedMap) // 맵 설정 완료 시 실행되는 함수
    {
        SetCameraOnMap(spawnedMap);
        gameUIManager?.HideGameLoadingPanel();
        StartCoroutine(GameStartSequence(spawnedMap?.MapBGM));
    }

    private void SetCameraOnMap(MapData spawnedMap) // 맵의 카메라를 설정하는 함수 
    {
        if (spawnedMap != null && cameraController != null)
        {
            cameraController.SetBounds(spawnedMap.CameraBounds);
            myCastleSpawnPoint = PhotonNetwork.IsMasterClient ? spawnedMap.Player1CastlePoint : spawnedMap.Player2CastlePoint;
        }
    }

    private IEnumerator GameStartSequence(AudioClip mapBGM) // 게임 시작 연출을 수행하는 코루틴
    {
        ShowGameStartPanel();
        PlayGameStartBGM();
        yield return new WaitForSeconds(2f);
        HideGameStartPanel();
        PlayMapBGM(mapBGM);
        if (deckHotkeyHandler != null)
            deckHotkeyHandler.IsEnabled = true;
    }

    private void ShowGameStartPanel() // 게임 시작 패널을 활성화하는 함수
    {
        string p1 = PhotonNetwork.PlayerList[0].NickName;
        string p2 = PhotonNetwork.PlayerList.Length > 1 ? PhotonNetwork.PlayerList[1].NickName : PlayerConstants.Default.Nickname;
        gameUIManager?.ShowGameStartPanel(p1, p2);
    }

    private static void PlayGameStartBGM() // 게임 시작 배경음악을 재생하는 함수
    {
        SoundManager.Instance?.Play(SoundKey.GameStartCue);
    }

    private void HideGameStartPanel() // 게임 시작 패널을 비활성화하는 함수
    {
        gameUIManager?.HideGameStartPanel();
    }

    private void PlayMapBGM(AudioClip mapBGM) // 게임 맵 배경음악을 재생하는 함수
    {
        if (mapBGM != null)
            SoundManager.Instance?.PlayDynamicBGM(mapBGM);
    }

    private void HandleMapLoadProgress(float normalized) // 맵 로딩 진행도를 처리하는 함수
    {
        gameUIManager?.UpdateLoadingProgress(normalized);
    }

    private void HandleEnergyChanged(float currentEnergy) // 에너지 변화를 처리하는 함수
    {
        gameUIManager?.UpdateSlotStateByEnergy(currentEnergy);
    }

    private void HandleReturnToRoomRequest() // UI 관련 방으로 돌아가기 요청을 처리하는 함수
    {
        PopupPanelUIManager.Instance?.ShowWaiting(PopupMessage.Waiting.RoomEntry, null);
        ResumeTime();
        if (PhotonNetwork.IsMasterClient)
        {
            HandleExecuteReturnToRoom();
        }
        else
        {
            gameNetworkManager?.HandleReturnToRoomRequest();
        }
    }

    private void ResumeTime() // 시간을 재개하는 함수
    {
        Time.timeScale = 1f;
    }

    private void HandleExecuteReturnToRoom() // 방장의 방 복귀 실행을 처리하는 함수
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        ResumeTime();
        ReopenRoom();
        PhotonNetwork.LoadLevel(SceneName.Room);
    }

    private void ReopenRoom() // 게임 종료 후 방을 다시 여는 함수
    {
        if (!PhotonNetwork.IsMasterClient || PhotonNetwork.CurrentRoom == null)
            return;

        PhotonNetwork.CurrentRoom.IsOpen = true;
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable()
        {
            { RoomConstants.Properties.GameStart, false },
            { RoomConstants.Properties.MapIndex, null }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    private void HandleReturnToLobbyRequest() // 로비로 돌아가기 요청을 처리하는 함수
    {
        ResumeTime();
        DestroyAllNetworkObjects();
        ReopenRoom();
        if (PhotonNetwork.CurrentRoom != null)
            PhotonNetwork.LeaveRoom();
    }

    private void DestroyAllNetworkObjects() // 방 내부의 모든 네트워크 오브젝트를 파괴하는 함수
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (PhotonNetwork.CurrentRoom != null)
            PhotonNetwork.DestroyAll();
    }

    private bool CanSpawnFromSlot(int slotIndex) // 특정 슬롯의 유닛을 소환할 수 있는지 확인하는 함수
    {
        return gameUIManager != null && gameUIManager.CheckUnitSpawnable(slotIndex);
    }

    private void SetUnitSpawnPoint() // 유닛 소환 지점을 설정하는 함수
    {
        var playerCastle = CastleAttackManager.Instance?.PlayerCastle;
        if (playerCastle != null)
            myUnitSpawnPoint = playerCastle.UnitSpawnPoint;
    }

    private bool CheckEnergyToSpawn(UnitStat unitToSpawn) // 유닛의 소환 에너지가 있는지 확인하는 함수
    {
        return energyManager!= null && energyManager.TryConsumeEnergy(unitToSpawn.SpawnCost);
    }

    private void SpawnUnit(UnitStat spawnUnitStat) // 유닛을 소환하는 함수
    {
        PhotonNetwork.Instantiate(spawnUnitStat.UnitPrefab.name, myUnitSpawnPoint.position, Quaternion.identity, 0);
        SoundManager.Instance?.Play(SoundKey.UnitSpawn);
    }

    private void HandleMapSelected(int mapIndex) // 맵 선택을 처리하는 함수
    {
        mapManager?.SetupGameMap(mapIndex);
    }

    private void HandleOpponentLeft(Player leftPlayer) // 상대 플레이어의 탈주를 처리하는 함수
    {
        if (gameState.IsGameOver)
            return;

        gameState.DeclareGameOver(true);
    }

    private void HandleLeftRoomForLobby() // 방 나가기 후 로비 이동을 처리하는 함수
    {
        PhotonNetwork.LoadLevel(SceneName.Lobby);
    }

    private void RegisterAllUnitToNetworkPool() // 네트워크 풀에 모든 유닛 프리팹을 등록하는 함수
    {
        if (NetworkPoolManager.Instance == null || unitDatabase == null)
            return;

        foreach (var unit in unitDatabase.Units)
        {
            if (unit.UnitPrefab != null)
                NetworkPoolManager.Instance.RegisterNetworkPrefab(unit.UnitPrefab);
        }
    }

    private void LoadDeckFromNetwork() // 서버에서 덱 정보를 가져오고 설정하는 함수
    {
        string[] myDeckNames = gameNetworkManager?.GetMyDeckNames();
        if (myDeckNames == null || unitDatabase == null)
            return;

        for (int i = 0; i < myDeckNames.Length; i++)
        {
            UnitStat unitStat = unitDatabase.FindByName(myDeckNames[i]);
            if (unitStat == null)
                continue;
                
            deckModel.SetUnit(i, unitStat);
            gameUIManager?.SetGameUnitSlotsUI(i, unitStat);
        }
    }
}