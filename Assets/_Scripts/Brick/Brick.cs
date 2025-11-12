using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject))]
public class Brick : NetworkBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Ball"))
            return;

        
        bool hasNO = TryGetComponent<NetworkObject>(out var no);

        
        Debug.Log($"[BRICK] IsServer={IsServer}, HasNO={hasNO}, IsSpawned={(hasNO && no.IsSpawned)} NetId={(hasNO ? no.NetworkObjectId : 0)}");

        
        if (!IsServer)
            return;

        
        if (hasNO && no.IsSpawned)
        {
            no.Despawn(true);  
        }
        else
        {
            
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"[BRICK] OnNetworkSpawn side={(IsServer ? "SERVER" : "CLIENT")} " +
                  $"hasNO={TryGetComponent<NetworkObject>(out var no)} spawned={(no && no.IsSpawned)} id={(no ? no.NetworkObjectId : 0)}");
    }

    public override void OnNetworkDespawn()
    {
        Debug.Log($"[BRICK] OnNetworkDespawn side={(IsServer ? "SERVER" : "CLIENT")}, name={name}");
        Destroy(gameObject);
    }
}
