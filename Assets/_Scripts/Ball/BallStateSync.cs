using Unity.Netcode;
using UnityEngine;

public class BallStateSync : NetworkBehaviour
{
    [Header("Refs")]
    [SerializeField] private Rigidbody2D rb;
    private GameObject midLine;

    [Header("Band settings")]
    [SerializeField] private float bandHalfHeight = 4f;
    [SerializeField] private float dirEpsilon = 0.01f;

    
    public NetworkVariable<sbyte> BallSide = new NetworkVariable<sbyte>(
        default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    
    public NetworkVariable<sbyte> VertDir = new NetworkVariable<sbyte>(
        default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> InBand = new NetworkVariable<bool>(
        default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private float lastY;

    public override void OnNetworkSpawn()
    {
        midLine = GameObject.Find("MiddleLine");

        
        if (rb) rb.simulated = IsServer;

        if (IsServer)
            lastY = transform.position.y;
    }

    void FixedUpdate()
    {
        if (!IsServer) return; // only server calculates

        float y = transform.position.y;
        float mid = midLine.transform.position.y;

        // on witch screen
        sbyte side = 0;
        if (y > mid) side = 1;
        else if (y < mid) side = -1;
        BallSide.Value = side;

        // is it on the band
        InBand.Value = (y <= mid + bandHalfHeight && y >= mid - bandHalfHeight);

        // rotation
        float dy = rb ? rb.linearVelocity.y : (y - lastY) / Time.fixedDeltaTime;

        sbyte vdir = 0;
        if (dy > dirEpsilon) vdir = 1;   // going up
        else if (dy < -dirEpsilon) vdir = -1; // going down
        VertDir.Value = vdir;

        lastY = y;
    }
}
