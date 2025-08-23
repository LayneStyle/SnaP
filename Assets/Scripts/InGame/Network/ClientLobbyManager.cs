
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using UnityEngine;

public class ClientLobbyManager : MonoBehaviour
{
    public static ClientLobbyManager Instance { get; private set; }
    
    public event Action<List<Lobby>> OnLobbyListUpdated;
    public event Action OnJoining;
    public event Action<string> OnJoinFailed;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async void Start()
    {
        await InitializeUGS();
    }

    private async Task InitializeUGS()
    {
        try
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            Debug.Log("Client Signed In.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize UGS: {e.Message}");
        }
    }

    public async void QueryLobbies()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                }
            };

            QueryResponse response = await global::Unity.Services.Lobbies.LobbyService.Instance.QueryLobbiesAsync(options);
            OnLobbyListUpdated?.Invoke(response.Results);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to query lobbies: {e.Message}");
        }
    }

    public async void JoinLobby(Lobby lobby, string displayName)
    {
        OnJoining?.Invoke();
        try
        {
            var joinOptions = new JoinLobbyByIdOptions
            {
                Player = new global::Unity.Services.Lobbies.Models.Player(
                    id: AuthenticationService.Instance.PlayerId,
                    data: new Dictionary<string, PlayerDataObject>
                    {
                        { "DisplayName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, displayName) }
                    }
                )
            };

            var joinedLobby = await global::Unity.Services.Lobbies.LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, joinOptions);

            var joinCode = joinedLobby.Data["joinCode"].Value;

            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            unityTransport.SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData);

            NetworkManager.Singleton.StartClient();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to join lobby: {e.Message}");
            OnJoinFailed?.Invoke(e.Message);
        }
    }
}
