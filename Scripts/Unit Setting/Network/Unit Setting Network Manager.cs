using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;

public class UnitSettingNetworkManager : MonoBehaviourPunCallbacks
{
    public event Action OnBothPlayersReady; // 모든 플레이어 준비 시 실행되는 이벤트
    public event Action<Player> OnOpponentLeftRoom; // 상대 플레이어의 방 퇴장 이벤트


    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) // 플레이어 속성을 업데이트하는 함수
    {
        if (CheckAllPlayersReady())
            OnBothPlayersReady?.Invoke();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer) // 상대 플레이어의 방 퇴장 시 실행되는 함수
    {
        OnOpponentLeftRoom?.Invoke(otherPlayer);
    }

    private bool CheckAllPlayersReady() // 두 플레이어의 준비 상태를 확인하는 함수
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
}