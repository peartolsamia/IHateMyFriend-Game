using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlateSpawner : NetworkBehaviour
{
    [Header("Prefab")]
    [SerializeField] private NetworkObject platePrefab;

    [Header("Spawn Noktalarý")]
    [SerializeField] private Transform hostSpawnPoint;   // Alt (Host)
    [SerializeField] private Transform clientSpawnPoint; // Üst (Client)

    // Ýsteðe baðlý: Transform kullanmak istemezsen Vector3 pozisyon kullan
    [Header("Transform atamazsan bu pozisyonlar kullanýlýr")]
    [SerializeField] private Vector3 fallbackHostPosition = new Vector3(0f, -15.5f, 0f);
    [SerializeField] private Vector3 fallbackClientPosition = new Vector3(0f, 15f, 0f);

    // Hangi client için hangi plate'in spawn edildiðini tutar
    private readonly Dictionary<ulong, NetworkObject> spawnedPlates = new Dictionary<ulong, NetworkObject>();

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

        var hostId = NetworkManager.ServerClientId;  // düzeltildi
        SpawnPlateForClient(
            hostId,
            hostSpawnPoint != null ? hostSpawnPoint.position : fallbackHostPosition,
            hostSpawnPoint != null ? hostSpawnPoint.rotation : Quaternion.identity
        );
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        if (clientId == NetworkManager.ServerClientId) return;  // düzeltildi

        SpawnPlateForClient(
            clientId,
            clientSpawnPoint != null ? clientSpawnPoint.position : fallbackClientPosition,
            clientSpawnPoint != null ? clientSpawnPoint.rotation : Quaternion.identity
        );
    }


    private void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer) return;

        // Ayrýlan oyuncunun plate'ini temizle (isteðe baðlý)
        if (spawnedPlates.TryGetValue(clientId, out var plate) && plate != null && plate.IsSpawned)
        {
            plate.Despawn(true); // true => server tarafýnda destroy et
        }
        spawnedPlates.Remove(clientId);
    }

    private void SpawnPlateForClient(ulong clientId, Vector3 position, Quaternion rotation)
    {
        if (platePrefab == null)
        {
            Debug.LogError("[PlateSpawner] Plate Prefab atanmadý!");
            return;
        }

        // Ayný client için ikinci kez spawn etmeyi engelle
        if (spawnedPlates.ContainsKey(clientId)) return;

        var plateInstance = Instantiate(platePrefab, position, rotation);

        // Sahipliði ilgili client'a vererek spawn et
        // (plate üzerinde hareket/scripts client owner'a göre çalýþabilir)
        plateInstance.SpawnWithOwnership(clientId, true);

        spawnedPlates[clientId] = plateInstance;
    }
}
