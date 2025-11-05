using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlateSpawner : NetworkBehaviour
{
    [Header("Prefab")]
    [SerializeField] private NetworkObject platePrefab;

    [Header("Spawn Noktalarý")]
    [SerializeField] private Transform hostSpawnPoint;   
    [SerializeField] private Transform clientSpawnPoint; 

    [Header("Transform atanmazsa fallback")]
    [SerializeField] private Vector3 fallbackHostPosition = new Vector3(0f, -15.5f, 0f);
    [SerializeField] private Vector3 fallbackClientPosition = new Vector3(0f, 15f, 0f);

    private readonly Dictionary<ulong, NetworkObject> spawnedPlates = new();

    private void OnEnable()
    {
        if (NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    private void OnServerStarted()
    {
        if (!IsServer) return;

        var hostId = NetworkManager.ServerClientId;
        var pos = hostSpawnPoint ? hostSpawnPoint.position : fallbackHostPosition;
        var rot = hostSpawnPoint ? hostSpawnPoint.rotation : Quaternion.identity;
        TrySpawnFor(hostId, pos, rot);
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;
        if (clientId == NetworkManager.ServerClientId) return;

        var pos = clientSpawnPoint ? clientSpawnPoint.position : fallbackClientPosition;
        var rot = clientSpawnPoint ? clientSpawnPoint.rotation : Quaternion.identity;
        TrySpawnFor(clientId, pos, rot);
        Debug.Log($"[PlateSpawner] Client baðlandý, plate spawn edildi -> clientId={clientId}");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer) return;
        if (spawnedPlates.TryGetValue(clientId, out var plate) && plate && plate.IsSpawned)
        {
            plate.Despawn(true);
        }
        spawnedPlates.Remove(clientId);
    }

    private void TrySpawnFor(ulong clientId, Vector3 position, Quaternion rotation)
    {
        if (!IsServer) return;

        if (!platePrefab)
        {
            Debug.LogError("[PlateSpawner] Plate Prefab atanmadý!");
            return;
        }
        if (spawnedPlates.ContainsKey(clientId)) return;

        
        if (!platePrefab.gameObject.activeSelf)
            Debug.LogWarning("[PlateSpawner] Plate prefab root inactive! Aktif olduðundan emin ol.");

        var instance = Instantiate(platePrefab, position, rotation);

        
        foreach (var nb in instance.GetComponentsInChildren<NetworkBehaviour>(true))
        {
            if (!nb.enabled || !nb.gameObject.activeInHierarchy)
                Debug.LogWarning($"[PlateSpawner] Spawn edilen plate içinde disabled NetworkBehaviour var: {nb.GetType().Name}");
        }

        instance.SpawnWithOwnership(clientId, destroyWithScene: true);
        spawnedPlates[clientId] = instance;

        Debug.Log($"[PlateSpawner] Spawn OK -> owner={clientId} pos={position}");
    }
}
