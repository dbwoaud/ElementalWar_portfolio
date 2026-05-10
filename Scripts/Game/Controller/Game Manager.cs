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

    [Header("스폰 설정")]
    [SerializeField] private Transform myCastleSpawnPoint;
    [SerializeField] private Transform myUnitSpawnPoint;

    protected override void SetCachedVariable()
    {
        deckModel = new DeckModel();
        gameState = new GameStateModel();
        base.SetCachedVariable();
    }

    protected override void SubscribeEvents()
    {
        base.SubscribeEvents();
        Castle.OnAnyCastleDestroyed += HandleCastleDestroyed;
        gameState.OnGameOver += HandleGameOver;

        if (MapManager.instance != null)
        {
            mapManager = MapManager.instance;
            mapManager.OnMapSetupCompleted += HandleMapSetupCompleted;
            mapManager.OnLoadProgress += HandleMapLoadProgress;
        }
        if (EnergyManager.instance != null && gameUIManager)
        {
            energyManager = EnergyManager.instance;
            energyManager.OnEnergyChanged += HandleEnergyChanged;
        }
    }

    protected override void SetUIManager()
    {
        if (GameUIManager.instance != null)
        {
            gameUIManager = GameUIManager.instance;
            gameUIManager.ShowGameLoadingPanel();
            gameUIManager.OnReturnToRoomRequested += HandleReturnToRoomRequest;
            gameUIManager.OnReturnToLobbyRequested += HandleReturnToLobbyRequest;
            gameUIManager.OnUnitSlotClicked += HandleUnitSpawnRequest;
        } 
    }

    protected override void SetNetworkManager()
    {
        if (gameNetworkManager != null)
        {
            gameNetworkManager.OnMapIndexReceived += HandleMapSelected;
            gameNetworkManager.OnOpponentLeftRoom += HandleOpponentLeft;
            gameNetworkManager.OnLeftRoomSuccess += HandleLeftRoomForLobby;
        }
    }

    protected override void ResetUIManager()
    {
        if (gameUIManager != null)
        {
            gameUIManager.OnReturnToRoomRequested -= HandleReturnToRoomRequest;
            gameUIManager.OnReturnToLobbyRequested -= HandleReturnToLobbyRequest;
            gameUIManager.OnUnitSlotClicked -= HandleUnitSpawnRequest;
        }
    }

    protected override void ResetNetworkManager()
    {
        if (gameNetworkManager != null)
        {
            gameNetworkManager.OnMapIndexReceived -= HandleMapSelected;
            gameNetworkManager.OnOpponentLeftRoom -= HandleOpponentLeft;
            gameNetworkManager.OnLeftRoomSuccess -= HandleLeftRoomForLobby;
        }
    }

    protected override void PlayBGM()
    {
        SoundManager.instance?.StopAll();
    }

    protected override void InitializeState()
    {
        UnitRegistry.Clear();
        RegisterAllUnitToNetworkPool();
        LoadMyDeckFromNetwork();
        SubscribeHotkey();
    }

    private void HandleReturnToRoomRequest() // 방으로 돌아가기 버튼 클릭 시 실행되는 함수
    {
        PopupPanelUIManager.instance?.ShowWaiting(PopupMessage.Waiting.RoomEntry, null);
        ResumeTime();
        ReopenRoom();
        PhotonNetwork.LoadLevel(SceneName.Room);
    }

    private void ResumeTime() // 시간을 흐르게하는 함수
    {
        Time.timeScale = 1f;
    }

    private void ReopenRoom() // 게임이 끝나고 방을 다시 오픈하는 함수
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

    private void HandleReturnToLobbyRequest() // 로비로 돌아가기 버튼 클릭 시 실행되는 함수
    {
        ResumeTime();
        ReopenRoom();
        if (PhotonNetwork.CurrentRoom != null)
            PhotonNetwork.LeaveRoom();
    }

    private void HandleUnitSpawnRequest(int slotIndex, UnitStat spawnUnitStat) // 유닛 생성 요청을 처리하는 함수
    {
        if (gameState.IsGameOver)
            return;

        SetUnitSpawnPoint();

        if (myUnitSpawnPoint == null)
            return;

        if (!CheckEnoughEnergy(spawnUnitStat))
            return;

        SpawnUnit(spawnUnitStat);
        gameUIManager?.StartSlotCoolTime(slotIndex);
    }

    private void SetUnitSpawnPoint() // 유닛 소환 지점을 설정하는 함수
    {
        var playerCastle = CastleAttackManager.instance?.PlayerCastle;
        if (playerCastle != null)
            myUnitSpawnPoint = playerCastle.UnitSpawnPoint;
    }

    private bool CheckEnoughEnergy(UnitStat unitToSpawn) // 유닛을 소환하기 위한 충분한 에너지가 있는지 확인하는 함수
    {
        return energyManager!= null && energyManager.TryConsumeEnergy(unitToSpawn.spawnCost);
    }

    private void SpawnUnit(UnitStat spawnUnitStat) // 유닛을 소환하는 함수
    {
        PhotonNetwork.Instantiate(spawnUnitStat.unitPrefab.name, myUnitSpawnPoint.position, Quaternion.identity, 0);
        SoundManager.instance?.Play(SoundKey.UnitSpawn);
    }

    private void HandleMapSelected(int mapIndex) // 맵 선택을 처리하는 함수
    {
        mapManager?.SetupGameEnvironment(mapIndex);
    }

    private void HandleOpponentLeft(Player leftPlayer) // 상대방의 탈주를 처리하는 함수
    {
        if (gameState.IsGameOver)
            return;

        gameState.DeclareGameOver(true);
    }

    private void HandleLeftRoomForLobby() // 방 퇴장 완료 후 로비로 이동하는 함수
    {
        PhotonNetwork.LoadLevel(SceneName.Lobby);
    }

    private void RegisterAllUnitToNetworkPool() // 네트워크 풀에 프리팹을 동록하는 함수
    {
        if (NetworkPoolManager.instance == null || unitDatabase == null)
            return;

        foreach (var unit in unitDatabase.All)
        {
            if (unit.unitPrefab != null)
                NetworkPoolManager.instance.RegisterNetworkPrefab(unit.unitPrefab);
        }
    }

    private void LoadMyDeckFromNetwork() // 네트워크에서 덱 정보를 가져오는 함수
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
            gameUIManager?.UpdateDeckSlotsUI(i, unitStat);
        }
    }

    private void SubscribeHotkey() // 키보드 숫자키 입력 이벤트를 구독하는 함수
    {
        if (deckHotkeyHandler != null)
            deckHotkeyHandler.OnSlotHotkeyPressed += HandleHotkeyUnitSpawn;
    }

    private void HandleHotkeyUnitSpawn(int slotIndex) // 단축키로 유닛을 소환하는 함수
    {
        if (gameState.IsGameOver)
            return;

        UnitStat unit = deckModel.GetUnit(slotIndex);
        if (unit != null)
            HandleUnitSpawnRequest(slotIndex, unit);
    }

    private void HandleCastleDestroyed(bool localPlayerLost, Vector3 castlePos) // 자신의 성이 파괴되어 게임 패배를 처리하는 함수
    {
        destroyedCastlePosition = castlePos;
        gameState.DeclareGameOver(!localPlayerLost);
    }

    private void HandleGameOver(bool localPlayerWon) // 게임 종료 연출을 처리하는 함수
    {
        if (deckHotkeyHandler != null)
            deckHotkeyHandler.IsEnabled = false;

        StartCoroutine(GameEndSequence(localPlayerWon));
    }

    private IEnumerator GameEndSequence(bool localPlayerWon) // 게임 종료 연출을 수행하는 코루틴 
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
            SoundManager.instance?.Play(SoundKey.GameWin);
        else
            SoundManager.instance?.Play(SoundKey.GameLose);

        gameUIManager?.ShowGameResultPanel(localPlayerWon, PhotonNetwork.NickName);
    }

    private void PauseAllGameSystems() // 모든 게임 시스템을 중지시키는 함수
    {
        Time.timeScale = 0f;
        SoundManager.instance?.StopAll();
        energyManager?.Stop();
        CastleAttackManager.instance?.Stop();
    }

    private void HandleMapSetupCompleted(MapData spawnedMap) // 맵 생성 완료 시 실행되는 함수
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
            myCastleSpawnPoint = PhotonNetwork.IsMasterClient
                ? spawnedMap.Player1CastlePoint
                : spawnedMap.Player2CastlePoint;
        }
    }

    private IEnumerator GameStartSequence(AudioClip mapBGM) // 게임 시작 연출을 수행하는 코루틴
    {
        ShowGameStartPanel();
        PlayGameStartBGM();
        yield return new WaitForSeconds(3f);
        HideGameStartPanel();
        PlayMapBGM(mapBGM);
        if (deckHotkeyHandler != null)
            deckHotkeyHandler.IsEnabled = true;
    }

    private void ShowGameStartPanel() // 게임 시작 연출을 시작하는 함수
    {
        string p1 = PhotonNetwork.PlayerList[0].NickName;
        string p2 = PhotonNetwork.PlayerList.Length > 1
            ? PhotonNetwork.PlayerList[1].NickName
            : PlayerConstants.Default.Nickname;

        gameUIManager?.ShowGameStartPanel(p1, p2);

    }

    private static void PlayGameStartBGM()
    {
        SoundManager.instance?.Play(SoundKey.GameStartCue);
    }

    private void HideGameStartPanel() // 게임 시작 연출을 끝내는 함수
    {
        gameUIManager?.HideGameStartPanel();
    }

    private void PlayMapBGM(AudioClip mapBGM)
    {
        if (mapBGM != null)
            SoundManager.instance?.PlayDynamicBGM(mapBGM);
    }

    private void HandleMapLoadProgress(float normalized) // 맵 로딩 진행도를 UI 에 전달
    {
        gameUIManager?.UpdateLoadingProgress(normalized);
    }

    private void HandleEnergyChanged(float currentEnergy) // 에너지 변동을 UI 에 전파하는 함수
    {
        gameUIManager?.RefreshSlotsEnergyState(currentEnergy);
    }

    protected override void UnsubscribeAll()
    {
        base.UnsubscribeAll();
        Castle.OnAnyCastleDestroyed -= HandleCastleDestroyed;
        if (gameState != null)
        {
            gameState.OnGameOver -= HandleGameOver;
        }
        if (energyManager != null)
        {
            energyManager.OnEnergyChanged -= HandleEnergyChanged;
        }
        if (mapManager != null)
        {
            mapManager.OnMapSetupCompleted -= HandleMapSetupCompleted;
            mapManager.OnLoadProgress -= HandleMapLoadProgress;
        }
        if (deckHotkeyHandler != null)
        {
            deckHotkeyHandler.OnSlotHotkeyPressed -= HandleHotkeyUnitSpawn;
        }
        UnitRegistry.Clear();
    }
}