using System;
using System.Collections.Generic;
using PolyAndCode.UI;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyListDataSource : MonoBehaviour, IRecyclableScrollRectDataSource
{
    public event Action StartLoadingEvent;
    public event Action EndLoadingEvent;
    
    [SerializeField] private RecyclableScrollRect _recyclableScrollRect;
    [SerializeField] private KeyCode _updateRectKeyCode;
    
    private List<Lobby> _lobbies = new();

    private void Awake()
    {
#if !UNITY_STANDALONE
        Destroy(gameObject);
        return;
#endif
        
        _recyclableScrollRect.DataSource = this;
    }

    private void OnEnable()
    {
        if (ClientLobbyManager.Instance != null)
        {
            ClientLobbyManager.Instance.OnLobbyListUpdated += OnLobbyListUpdated;
        }
    }

    private void OnDisable()
    {
        if (ClientLobbyManager.Instance != null)
        {
            ClientLobbyManager.Instance.OnLobbyListUpdated -= OnLobbyListUpdated;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(_updateRectKeyCode) == false)
        {
            return;
        }

        UpdateScrollRect();
    }

    public int GetItemCount()
    {
        return _lobbies.Count;
    }

    public void SetCell(ICell cell, int index)
    {
        if (cell is LobbyListCell lobbyListCell)
        {
            lobbyListCell.SetLobbyInfo(_lobbies[index]);
        }
    }

    // Button.
    public void UpdateScrollRect()
    {
        StartLoadingEvent?.Invoke();
        ClientLobbyManager.Instance.QueryLobbies();
    }
    
    private void OnLobbyListUpdated(List<Lobby> lobbies)
    {
        _lobbies = lobbies;
        _recyclableScrollRect.ReloadData();
        EndLoadingEvent?.Invoke();
        Logger.Log("Scroll rect updated.");
    }
}