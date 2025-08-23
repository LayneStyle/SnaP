
using System;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator), typeof(LobbyServiceConnectionStatusHoverTooltip))]
public class LobbyServiceConnectionResultUI : MonoBehaviour
{
    public struct Sprites
    {
        public Sprite DisconnectedSprite { get; }
        public Sprite LoadingSprite { get; }
        public Sprite SuccessSprite { get; }
        public Sprite FailSprite { get; }

        public Sprites(Sprite disconnectedSprite, Sprite loadingSprite, Sprite successSprite, Sprite failSprite)
        {
            DisconnectedSprite = disconnectedSprite;
            LoadingSprite = loadingSprite;
            SuccessSprite = successSprite;
            FailSprite = failSprite;
        }
    }

    [SerializeField] private Image _image;
    private LobbyServiceConnectionStatusHoverTooltip _hoverTooltip;
    private static readonly int Loading = Animator.StringToHash("Loading");
    private Animator _animator;
    private Sprites _sprites;

    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Successful,
        Failed
    }
    public ConnectionState CurrentState { get; private set; } = ConnectionState.Disconnected;

    private void Awake()
    {
        _hoverTooltip = GetComponent<LobbyServiceConnectionStatusHoverTooltip>();
        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (ClientLobbyManager.Instance != null)
        {
            ClientLobbyManager.Instance.OnJoining += HandleConnecting;
            ClientLobbyManager.Instance.OnJoinFailed += HandleConnectionFailed;
        }
    }

    private void OnDisable()
    {
        if (ClientLobbyManager.Instance != null)
        {
            ClientLobbyManager.Instance.OnJoining -= HandleConnecting;
            ClientLobbyManager.Instance.OnJoinFailed -= HandleConnectionFailed;
        }
    }
    
    private void Start()
    {
        if (UnityServices.State == ServicesInitializationState.Initialized)
        {
            SetConnectionState(ConnectionState.Successful);
        }
    }

    public void SetSprites(Sprites sprites)
    {
        _sprites = sprites;
        SetConnectionState(CurrentState);
    }
    
    private void HandleConnecting()
    {
        SetConnectionState(ConnectionState.Connecting);
    }

    private void HandleConnectionFailed(string reason)
    {
        SetConnectionState(ConnectionState.Failed);
    }

    private void SetConnectionState(ConnectionState connectionState)
    {
        CurrentState = connectionState;
        if (_sprites.LoadingSprite == null) return;

        _animator.SetBool(Loading, connectionState == ConnectionState.Connecting);
        _image.sprite = connectionState switch
        {
            ConnectionState.Connecting => _sprites.LoadingSprite,
            ConnectionState.Successful => _sprites.SuccessSprite,
            ConnectionState.Failed => _sprites.FailSprite,
            ConnectionState.Disconnected => _sprites.DisconnectedSprite,
            _ => throw new ArgumentOutOfRangeException(nameof(connectionState), connectionState, null)
        };
    }
}
