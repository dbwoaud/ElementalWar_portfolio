using Photon.Realtime;
using PlayFab;

public static class SceneName
{
    public static readonly string MainMenu = "MainMenu";
    public static readonly string Lobby = "Lobby";
    public static readonly string Room = "Room";
    public static readonly string UnitSetting = "UnitSetting";
    public static readonly string Game = "Game";
}

public static class PlayerConstants
{
    public static class Default
    {
        public static readonly string Nickname = "Unknown";
    }

    public static class Properties
    {
        public static readonly string GameReady = "IsGameReady";
        public static readonly string DeckReady = "IsDeckReady";
        public static readonly string DeckList = "DeckList";
    }
}

public static class RoomConstants
{
    public static class Properties
    {
        public static readonly string RoomName = "RoomName";
        public static readonly string RoomNumber = "RoomNumber";
        public static readonly string PublicOrPrivate = "IsPublic";
        public static readonly string Password = "Password";
        public static readonly string GameStart = "IsGameStarted";
        public static readonly string MapIndex = "MapIndex";
    }

    public static class Status
    {
        public static readonly string OnGoing = "ÁøÇà Áß";
        public static readonly string Waiting = "´ë±â Áß";
    }

    public static class ButtonText
    {
        public static readonly string Start = "°ÔÀÓ ½ÃÀÛ";
        public static readonly string Ready = "ÁØºñ ¿Ï·á";
        public static readonly string CancelReady = "ÁØºñ Ãë¼Ò";
    }
}

public static class PopupMessage
{
    public static class Waiting
    {
        public static readonly string Login = "·Î±×ÀÎ ÁßÀÔ´Ï´Ù";
        public static readonly string ServerConnection = "°ÔÀÓ ¼­¹ö¿¡ Á¢¼Ó ÁßÀÔ´Ï´Ù";
        public static readonly string LobbyConnection = "·Îºñ¿¡ Á¢¼Ó ÁßÀÔ´Ï´Ù";
        public static readonly string RandomMatching = "·£´ı ¸ÅÄª ÁßÀÔ´Ï´Ù";
        public static readonly string RoomEntry = "¹æ¿¡ ÀÔÀå ÁßÀÔ´Ï´Ù";
        public static readonly string RoomCreate = "¹æÀ» »ı¼º ÁßÀÔ´Ï´Ù";
        public static readonly string WaitingOpponent = "»ó´ë¹æÀÇ À¯´Ö ¼±ÅÃÀ» ±â´Ù¸®°í ÀÖ½À´Ï´Ù";
        public static readonly string GameLoading = "°ÔÀÓÀ» ·Îµù ÁßÀÔ´Ï´Ù";
    }

    public static class Error
    {
        public static readonly string InvalidNickname = "´Ğ³×ÀÓÀº °ø¹é°ú Æ¯¼ö¹®ÀÚ¸¦ Á¦¿ÜÇÑ\nÇÑ±Û/¿µ¹®/¼ıÀÚ 2~12ÀÚ·Î ÀÔ·ÂÇØ ÁÖ¼¼¿ä";
        public static readonly string InvalidRoomName = "¹æ ÀÌ¸§Àº 2ÀÚ ÀÌ»ó, 16ÀÚ ÀÌÇÏ·Î ÀÔ·ÂÇØ ÁÖ¼¼¿ä";
        public static readonly string InvalidPassword = "ºñ¹Ğ¹øÈ£´Â 4ÀÚ¸® ¼ıÀÚ·Î ÀÔ·ÂÇØ ÁÖ¼¼¿ä";
        public static readonly string NotMatchPassword = "ºñ¹Ğ¹øÈ£°¡ ÀÏÄ¡ÇÏÁö ¾Ê½À´Ï´Ù";
        public static readonly string NeedMorePlayer = "È¥ÀÚ¼­´Â °ÔÀÓÀ» ½ÃÀÛÇÒ ¼ö ¾ø½À´Ï´Ù";
        public static readonly string NeedAllReady = "¸ğµç ÇÃ·¹ÀÌ¾î°¡ ÁØºñ¸¦ ¿Ï·áÇØ¾ß ÇÕ´Ï´Ù";
        public static readonly string NeedDeckFull = "10¸íÀÇ À¯´ÖÀ» ¸ğµÎ ¼³Á¤ÇØ¾ß ÇÕ´Ï´Ù";
        public static readonly string OpponentLeft = "»ó´ë¹æÀÌ ¹æÀ» ³ª°¬½À´Ï´Ù. ¹æÀ¸·Î µ¹¾Æ°©´Ï´Ù";
    }

    public static class Confirm
    {
        public static readonly string SuccessRegister = "È¸¿ø °¡ÀÔ¿¡ ¼º°øÇÏ¿´½À´Ï´Ù";
    }

    public static class Selection
    {
        public static readonly string GameExit = "°ÔÀÓÀ» Á¤¸» Á¾·áÇÏ½Ã°Ú½À´Ï±î?";
        public static readonly string RoomExit = "¹æ¿¡¼­ Á¤¸» ³ª°¡½Ã°Ú½À´Ï±î?";
    }
}

public static class RegexPattern
{
    public static class User
    {
        public static readonly string ValidNickname = @"^[a-zA-Z°¡-ÆR0-9]{2,12}$";
    }

    public static class Room
    {
        public static readonly string ValidPassword = @"^[0-9]{4}$";
    }
}

public static class GameSystem
{
    public static class Cost
    {
        public static string GetUnitCostText(int spawnCost) => $"{spawnCost} ¿ø";
    }

    public static class Energy
    {
        public static string GetEnergyText(int current, int max) => $"{current} / {max}¿ø";
        public static string GetLevelText(int level, bool isMax) => isMax ? "Lv. Max" : $"Lv. {level}";
        public static string GetUpgradeCostText(int cost, bool isMax) => isMax ? "MAX" : $"{cost} ¿ø";
    }

    public static class CastleConstants
    {
        public static readonly string PlayerLayer = "PlayerCastle";
        public static readonly string EnemyLayer = "EnemyCastle";
        public static string GetHPText(float current, float max) => $"{current} / {max}";
    }

    public static class UnitConstants
    {
        public static readonly string PlayerLayer = "PlayerUnit";
        public static readonly string EnemyLayer = "EnemyUnit";
    }

    public static class Ground
    {
        public static readonly string ColliderTag = "Ground";
    }

    public static class Gameresult
    {
        public static string GetGameResultText(string playerName, bool isWinner)
        {
            return $"{playerName} : {(isWinner ? "½Â¸®" : "ÆĞ¹è")}";
        }
    }
}

public static class ChattingSystem
{
    public static class Color
    {
        public enum ChatColor { White, Red, Yellow, Green, Blue, Purple, Black }

        public static string GetColor(ChatColor colorType) // Ã¤ÆÃ »ö»óÀ» HEX ÄÚµå·Î ¹İÈ¯ÇÏ´Â ÇÔ¼ö
        {
            switch (colorType)
            {
                case ChatColor.White: return "#FFFFFF";
                case ChatColor.Red: return "#FF0000";
                case ChatColor.Yellow: return "#FFFF00";
                case ChatColor.Green: return "#00FF00";
                case ChatColor.Blue: return "#0000FF";
                case ChatColor.Purple: return "#99086E";
                case ChatColor.Black: return "#000000";
                default: return "#000000";
            }
        }
    }

    public static class Lobby
    {
        public static readonly string ChannelName = "GlobalLobby";
    }

    public static class SystemMessage
    {
        public static readonly string PlayerEntered = " ´ÔÀÌ ¹æ¿¡ Âü°¡Çß½À´Ï´Ù";
        public static readonly string PlayerExited = " ´ÔÀÌ ¹æ¿¡¼­ ³ª°¬½À´Ï´Ù";
    }
}

public static class ErrorTranslator
{
    public static string GetPhotonErrorMessage(short returnCode) // ¹æ »ı¼º ¹× ÀÔÀå °ü·Ã ¿À·ù ¸Ş½ÃÁö¸¦ ¾ò´Â ÇÔ¼ö
    {
        switch (returnCode)
        {
            case ErrorCode.GameIdAlreadyExists:
                return "ÀÌ¹Ì Á¸ÀçÇÏ´Â ¹æ ÀÌ¸§ÀÔ´Ï´Ù.";
            case ErrorCode.GameFull:
                return "¹æÀÌ °¡µæ Â÷¼­ ÀÔÀåÇÒ ¼ö ¾ø½À´Ï´Ù.";
            case ErrorCode.GameClosed:
                return "ÇöÀç ÀÔÀåÇÒ ¼ö ¾ø´Â ¹æÀÔ´Ï´Ù.";
            case ErrorCode.GameDoesNotExist:
                return "Á¸ÀçÇÏÁö ¾Ê´Â ¹æÀÔ´Ï´Ù.";
            case ErrorCode.MaxCcuReached:
                return "¼­¹ö Á¢¼Ó ÃÖ´ë ÀÎ¿øÀÌ ÃÊ°úµÇ¾ú½À´Ï´Ù. Àá½Ã ÈÄ ´Ù½Ã ½ÃµµÇØ ÁÖ¼¼¿ä.";
            case ErrorCode.InvalidOperation:
                return "Àß¸øµÈ ¿äÃ»ÀÔ´Ï´Ù.";
            case ErrorCode.NoRandomMatchFound:
                return "ÀÔÀå °¡´ÉÇÑ °ø°³ ¹æÀÌ ¾ø½À´Ï´Ù.";
            default:
                return $"³×Æ®¿öÅ© ¿äÃ» Ã³¸®¿¡ ½ÇÆĞÇß½À´Ï´Ù. (¿¡·¯ ÄÚµå: {returnCode})";
        }
    }

    public static string GetDisconnectMessage(DisconnectCause cause) // Æ÷Åæ ¼­¹öÀÇ ¿¬°á ²÷±è °ü·Ã ¿À·ù ¸Ş½ÃÁö¸¦ ¾ò´Â ÇÔ¼ö
    {
        switch (cause)
        {
            case DisconnectCause.ClientTimeout:
            case DisconnectCause.ServerTimeout:
                return "³×Æ®¿öÅ© »óÅÂ°¡ ºÒ¾ÈÁ¤ÇÏ¿© ¼­¹ö¿ÍÀÇ ¿¬°áÀÌ Áö¿¬/²÷¾îÁ³½À´Ï´Ù.";
            case DisconnectCause.DisconnectByServerLogic:
            case DisconnectCause.DisconnectByServerReasonUnknown:
                return "¼­¹ö Ãø ¹®Á¦·Î ÀÎÇØ ¿¬°áÀÌ Á¾·áµÇ¾ú½À´Ï´Ù.";
            case DisconnectCause.InvalidAuthentication:
                return "ÀÎÁõ¿¡ ½ÇÆĞÇÏ¿© ¿¬°áÀÌ ²÷¾îÁ³½À´Ï´Ù.";
            case DisconnectCause.MaxCcuReached:
                return "¼­¹ö Á¢¼Ó ÇÑµµ¸¦ ÃÊ°úÇÏ¿© ¿¬°áÀÌ Á¾·áµÇ¾ú½À´Ï´Ù.";
            default:
                return $"¼­¹ö¿ÍÀÇ ¿¬°áÀÌ ²÷¾îÁ³½À´Ï´Ù. (¿øÀÎ: {cause})";
        }
    }

    public static string GetPlayFabErrorMessage(PlayFabErrorCode errorCode) // PlayFab ¼­¹öÀÇ °ü·Ã ¿À·ù ¸Ş½ÃÁö¸¦ ¾ò´Â ÇÔ¼ö
    {
        switch (errorCode)
        {
            case PlayFabErrorCode.InvalidParams: return "ÀÔ·ÂÇÏ½Å Á¤º¸ÀÇ Çü½ÄÀÌ ¿Ã¹Ù¸£Áö ¾Ê½À´Ï´Ù.";
            case PlayFabErrorCode.InvalidEmailAddress: return "ÀÌ¸ŞÀÏ Çü½ÄÀÌ ¿Ã¹Ù¸£Áö ¾Ê½À´Ï´Ù.";
            case PlayFabErrorCode.InvalidPassword: return "ºñ¹Ğ¹øÈ£´Â 6ÀÚ¸® ÀÌ»óÀÌ¾î¾ß ÇÕ´Ï´Ù.";
            case PlayFabErrorCode.AccountNotFound: return "°¡ÀÔµÇÁö ¾ÊÀº ÀÌ¸ŞÀÏÀÔ´Ï´Ù.";
            case PlayFabErrorCode.InvalidEmailOrPassword: return "ÀÌ¸ŞÀÏ ¶Ç´Â ºñ¹Ğ¹øÈ£°¡ ÀÏÄ¡ÇÏÁö ¾Ê½À´Ï´Ù.";
            case PlayFabErrorCode.EmailAddressNotAvailable: return "ÀÌ¹Ì »ç¿ë ÁßÀÎ ÀÌ¸ŞÀÏÀÔ´Ï´Ù.";
            case PlayFabErrorCode.UsernameNotAvailable: return "ÀÌ¹Ì »ç¿ë ÁßÀÎ ´Ğ³×ÀÓÀÔ´Ï´Ù.";
            default: return $"ÀÎÁõ ¼­¹ö Åë½Å ¿À·ù°¡ ¹ß»ıÇß½À´Ï´Ù. ({errorCode})";
        }
    }
}