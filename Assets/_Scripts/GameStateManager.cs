using Unity.Netcode;
using UnityEngine;

public class GameStateManager : NetworkBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [SerializeField] private int requiredPlayers = 2;

    // everyone reads, server writes
    public readonly NetworkVariable<bool> GameStarted =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        
        GameStarted.OnValueChanged += OnGameStartedChangedLog;

        
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;

            
            TryStartOrStopGame();
        }
    }

    public override void OnNetworkDespawn()
    {
        GameStarted.OnValueChanged -= OnGameStartedChangedLog;

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        }
    }

    void OnServerStarted()
    {
        if (!IsServer) return;
        TryStartOrStopGame();
    }

    void OnClientConnected(ulong _)
    {
        if (!IsServer) return;
        TryStartOrStopGame();
    }

    void OnClientDisconnected(ulong _)
    {
        if (!IsServer) return;
        TryStartOrStopGame();
    }

    void TryStartOrStopGame()
    {
        
        if (NetworkManager.Singleton == null) return;

        int connected = NetworkManager.Singleton.ConnectedClientsList.Count;
        bool shouldStart = connected >= requiredPlayers;

        if (GameStarted.Value != shouldStart)
        {
            GameStarted.Value = shouldStart;
            Debug.Log($"[GameState] GameStarted set => {shouldStart} (connected: {connected})");
        }
    }

    void OnGameStartedChangedLog(bool oldV, bool newV)
    {
        Debug.Log($"[GameState] GameStarted changed {oldV} -> {newV}");
    }
}
