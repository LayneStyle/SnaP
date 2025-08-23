using System;
using UnityEngine;

[RequireComponent(typeof(LobbyServiceConnectionResultUI))]
public class LobbyServiceConnectionStatusHoverTooltip : HoverTooltip
{
    private LobbyServiceConnectionResultUI _connectionResultUI;

    private void Awake()
    {
        _connectionResultUI = GetComponent<LobbyServiceConnectionResultUI>();
    }

    public override void SetupText()
    {
        Text.text = _connectionResultUI.CurrentState switch
        {
            LobbyServiceConnectionResultUI.ConnectionState.Connecting => "Connecting to lobby...",
            LobbyServiceConnectionResultUI.ConnectionState.Successful => "Connected to lobby!",
            LobbyServiceConnectionResultUI.ConnectionState.Failed => "Failed to connect to lobby.",
            LobbyServiceConnectionResultUI.ConnectionState.Disconnected => "Disconnected from lobby.",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}