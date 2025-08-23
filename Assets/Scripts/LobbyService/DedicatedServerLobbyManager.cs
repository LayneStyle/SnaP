
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using UnityEngine;
using Unity.Netcode;

public class DedicatedServerLobbyManager : MonoBehaviour
{
    private const string KEY_JOIN_CODE = "joinCode";
    private Lobby _currentLobby;
    private float _heartbeatTimer;

    private async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Dedicated Server Signed In.");

            await CreateLobby();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error during server startup: {e.Message}");
        }
    }

    private void Update()
    {
        if (_currentLobby != null)
        {
            _heartbeatTimer -= Time.deltaTime;
            if (_heartbeatTimer <= 0)
            {
                _heartbeatTimer = 15f;
                global::Unity.Services.Lobbies.LobbyService.Instance.SendHeartbeatPingAsync(_currentLobby.Id);
            }
        }
    }

    private async Task CreateLobby()
    {
        try
        {
            string lobbyName = "Mesa de Juego";
            int maxPlayers = (int)NetworkConnectorHandler.MaxPlayersAmount;
            
            var allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject>
                {
                    {
                        KEY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Public, joinCode)
                    }
                }
            };

            _currentLobby = await global::Unity.Services.Lobbies.LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            
            NetworkManager.Singleton.StartHost();

            Debug.Log($"Lobby created with ID: {_currentLobby.Id} and Join Code: {joinCode}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create lobby: {e.Message}");
        }
    }

    private async void OnApplicationQuit()
    {
        if (_currentLobby != null)
        {
            try
            {
                await global::Unity.Services.Lobbies.LobbyService.Instance.DeleteLobbyAsync(_currentLobby.Id);
                Debug.Log("Lobby deleted on server shutdown.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete lobby on shutdown: {e.Message}");
            }
        }
    }
}
