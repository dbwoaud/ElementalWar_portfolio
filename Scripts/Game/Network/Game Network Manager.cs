using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;

public class GameNetworkManager : MonoBehaviourPunCallbacks
{
    public event Action<Player> OnOpponentLeftRoom;
    public event Action<int> OnMapIndexReceived;
    public event Action OnLeftRoomSuccess;

    private void Start()
    {
        StartCoroutine(TryGetMapIndexCoroutine());
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

        OnMapIndexReceived?.Invoke(mapIndex);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer) // 상대방의 탈주 시 실행되는 함수
    {
        OnOpponentLeftRoom?.Invoke(otherPlayer);
    }

    public override void OnLeftRoom() // 방 퇴장이 완료되었을 때 실행되는 함수
    {
        OnLeftRoomSuccess?.Invoke();
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged) // 방의 정보가 갱신될 때 실행되는 함수
    {
        if (TryReadMapIndex(propertiesThatChanged, out int mapIndex))
            OnMapIndexReceived?.Invoke(mapIndex);
    }

    private bool TryReadMapIndex(ExitGames.Client.Photon.Hashtable props, out int mapIndex) // 방 프로퍼티에서 맵 인덱스를 얻는 함수
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

    public string[] GetMyDeckNames() // 포톤 서버에 저장된 유닛 이름들을 가져오는 함수
    {
        if (HasDeckProperty(out object deckData))
        {
            if (deckData is string[] deckNames)
                return deckNames;
        }        
        return null;
    }

    private bool HasDeckProperty(out object deckData) // 포톤 서버에 저장된 유닛 이름들이 있는지 확인하는 함수
    {
        return PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(PlayerConstants.Properties.DeckList, out deckData);
    }
}