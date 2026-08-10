using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;

public class GameNetworkManager : MonoBehaviourPunCallbacks
{
    private bool isMapIndexSet;

    public event Action<Player> OnOpponentLeftRoom; // 상대 플레이어 탈주 시 실행되는 이벤트 
    public event Action<int> OnMapIndexSet; // 맵 인덱스 설정 시 실행되는 이벤트
    public event Action OnLeftRoomSuccess; // 방 나가기 성공 시 실행되는 이벤트
    public event Action OnReturnToRoomRequested; // 방으로 돌아가기 요청 이벤트
#if ENABLE_PROFILING
    public event Action<int> OnProfilingStart; // 프로파일링 시작 이벤트
#endif


    private void Start()
    {
        StartCoroutine(TryGetMapIndexCoroutine());
    }

    public override void OnPlayerLeftRoom(Player otherPlayer) // 상대 플레이어 탈주 시 실행되는 함수
    {
        OnOpponentLeftRoom?.Invoke(otherPlayer);
    }

    public override void OnLeftRoom() // 방 나가기 성공 시 실행되는 함수
    {
        OnLeftRoomSuccess?.Invoke();
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged) // 방 정보를 업데이트하는 함수
    {
        if (TryReadMapIndex(propertiesThatChanged, out int mapIndex))
            SetMapIndex(mapIndex);
    }

    private IEnumerator TryGetMapIndexCoroutine() // 맵 인덱스 반환을 시도하는 코루틴
    {
        yield return null;
        TryGetMapIndex();
    }

    private void TryGetMapIndex() // 맵 인덱스 반환을 시도하는 함수
    {
        if (PhotonNetwork.CurrentRoom == null)
            return;

        if (!TryReadMapIndex(PhotonNetwork.CurrentRoom?.CustomProperties, out int mapIndex))
            return;

        SetMapIndex(mapIndex);
    }

    private bool TryReadMapIndex(ExitGames.Client.Photon.Hashtable props, out int mapIndex) // 방 프로퍼티에서 맵 인덱스를 읽어오는 함수
    {
        if (props != null && props.TryGetValue(RoomConstants.Properties.MapIndex, out object value))
        {
            if (value is int result)
            {
                mapIndex = result;
                return true;
            }
        }

        mapIndex = -1;
        return false;
    }

    private void SetMapIndex(int mapIndex) // 맵 인덱스를 설정하는 함수
    {
        if (isMapIndexSet)
            return;

        isMapIndexSet = true;
        OnMapIndexSet?.Invoke(mapIndex);
    }

    public string[] GetMyDeckNames() // 자신의 덱에 저장된 유닛 이름들을 반환하는 함수
    {
        if (CheckDeckProperty(out object deckData))
        {
            if (deckData is string[] deckNames)
                return deckNames;
        }        
        return null;
    }

    private bool CheckDeckProperty(out object deckData) // 플레이어 속성에서 자신의 덱에 저장된 유닛 이름들이 있는지 확인하는 함수
    {
        return PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(PlayerConstants.Properties.DeckList, out deckData);
    }

    public void HandleReturnToRoomRequest() // 방으로 돌아가기 요청을 처리하는 함수
    {
        photonView.RPC(nameof(RPC_HandleReturnToRoomRequest), RpcTarget.MasterClient);
    }

    [PunRPC]
    private void RPC_HandleReturnToRoomRequest() // 클라이언트의 방으로 돌아가기 요청을 처리하는 함수
    {
        OnReturnToRoomRequested?.Invoke();
    }

#if ENABLE_PROFILING
    public void BroadcastProfilingStart(int scenarioSeed) // 프로파일링 시작을 다른 플레이어와 동기화하는 함수
    {
        photonView.RPC(nameof(RPC_HandleProfilingStart), RpcTarget.All, scenarioSeed);
    }

    [PunRPC]
    private void RPC_HandleProfilingStart(int scenarioSeed) // 프로파일링 시작을 처리하는 함수
    {
        OnProfilingStart?.Invoke(scenarioSeed);
    }
#endif
}