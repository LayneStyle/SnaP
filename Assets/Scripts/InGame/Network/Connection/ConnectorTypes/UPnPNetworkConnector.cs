using System;
using System.Net;
using System.Threading.Tasks;
using LobbyService;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;

public class UPnPNetworkConnector : INetworkConnector
{
    public NetworkConnectorType Type => NetworkConnectorType.UPnP;
    
    private string _localIpAddress;
    private ushort _port;
    private string _ruleName;
    
    // This is fake-Init.
    // Real Init is in TryCreateGame() method.
    public Task Init()
    {
        return Task.CompletedTask;
    }

    public async Task<bool> TryCreateGame()
    {
        #region Init

        if (ConnectionDataPresenter.TryGetAvailableUdpPort(out ushort port) == false)
        {
            Logger.Log($"Can`t find available port.", Logger.LogLevel.Error);
            return false;
        }
        
        Logger.Log($"Available port set to {port}.");
        IPAddress localIpAddress = await ConnectionDataPresenter.GetLocalIpAddressAsync();
        
        string ruleName = "SnaP-UPnP-" + port;

        #endregion
        
        if (NetworkManager.Singleton.IsListening == true)
        {
            Logger.Log("Can`t create game: NetworkManager is already listening.", Logger.LogLevel.Error);
            return false;
        }
        
        UnityTransport unityTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        
        bool redirectionResult = await UPnP.RedirectExternalConnectionToThisDevice(port, ruleName);
        if (redirectionResult == false)
        {
            Logger.Log("UPnP: Unable to redirect external connection to this device", Logger.LogLevel.Error);
            return false;
        }

        unityTransport.SetConnectionData(localIpAddress.ToString(), port);

        try
        {
            NetworkManager.Singleton.StartHost();
            
            Logger.Log("Forwarding to public IP...");
        
            await ConnectionDataPresenter.GetOrUpdatePublicIpAddressAsync();
        
            NetworkManager.Singleton.SceneManager.LoadScene(Constants.SceneNames.Desk, LoadSceneMode.Single);
        }
        catch (Exception e)
        {
            Logger.Log($"Can`t StartHost(). {e}", Logger.LogLevel.Error);
            return false;
        }

        return true;
    }

    public async Task<bool> TryJoinGame()
    {
        // Esta lógica ha sido movida al ClientLobbyManager para usar UGS
        // Esta implementación de TryJoinGame es ahora obsoleta para el flujo de UGS
        // pero la mantenemos para evitar errores en otras partes del código que puedan llamarla.
        Logger.Log("TryJoinGame in UPnPNetworkConnector is obsolete. Use ClientLobbyManager to join via Lobby.", Logger.LogLevel.Warning);
        return false;
    }
}