using PolyAndCode.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Lobbies.Models;

public class LobbyListCell : MonoBehaviour, ICell
{
    public static Lobby SelectedLobby { get; private set; }

    [SerializeField] private TextMeshProUGUI _lobbyNameText;
    [SerializeField] private TextMeshProUGUI _lobbyPlayersCountText; // e.g "2/5"

    [SerializeField] private Button _joinButton;
    
    private Lobby _lobby;
    
    private static bool _isJoining;

    private void OnEnable()
    {
        _joinButton.interactable = true;
        _isJoining = false;
    }

    public void SetLobbyInfo(Lobby lobby)
    {
        if (lobby == null)
        {
            Logger.Log("LobbyInfo is null!", Logger.LogLevel.Error);
            return;
        }
        
        _lobby = lobby;
        
        _lobbyNameText.text = lobby.Name;
        _lobbyPlayersCountText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
    }

    // Button.
    public void OnJoinButtonPressed()
    {
        if (_isJoining)
        {
            return;
        }

        _isJoining = true;
        _joinButton.interactable = false;
        SelectedLobby = _lobby;
        ClientLobbyManager.Instance.JoinLobby(_lobby);
    }
}