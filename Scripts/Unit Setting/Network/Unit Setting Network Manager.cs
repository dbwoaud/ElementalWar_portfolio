using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;

public class UnitSettingNetworkManager : MonoBehaviourPunCallbacks
{
    public event Action OnBothPlayersReady;
    public event Action<Player> OnOpponentLeftRoom;


    public void SetPlayerReadyState(string[] deckUnitNames) // 플레이어의 덱과 준비 상태를 설정하는 함수
    {
        Hashtable props = new Hashtable
        {
            { PlayerConstants.Properties.DeckReady, true },
            { PlayerConstants.Properties.DeckList, deckUnitNames }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    public void ResetPlayerReadyState() // 플레이어의 덱과 준비 상태를 리셋하는 함수
    {
        Hashtable props = new Hashtable
        {
            { PlayerConstants.Properties.GameReady, false },
            { PlayerConstants.Properties.DeckReady, false },
            { PlayerConstants.Properties.DeckList, new string[0] }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) // 준비 완료 버튼을 눌렀을 때 실행되는 함수
    {
        if (CheckAllPlayersReady())
            OnBothPlayersReady?.Invoke();     
    }

    private bool CheckAllPlayersReady() // 두 플레이어가 모두 준비 완료 상태인지 검사하는 함수
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
            return false;

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (!IsPlayerReady(p))
                return false;
        }
        return true;
    }

    private bool IsPlayerReady(Player p) // 플레이어의 준비 상태를 확인하는 함수
    {
        return p.CustomProperties.ContainsKey(PlayerConstants.Properties.DeckReady) && (bool)p.CustomProperties[PlayerConstants.Properties.DeckReady];
    }

    public override void OnPlayerLeftRoom(Player otherPlayer) // 상대 플레이어가 나갔을 때 실행되는 함수
    {
        OnOpponentLeftRoom?.Invoke(otherPlayer);
    }

    public void PrepareMapForGameScene(int mapCount)
    {
        int idx = UnityEngine.Random.Range(0, mapCount);
        var props = new Hashtable { { RoomConstants.Properties.MapIndex, idx } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }
}