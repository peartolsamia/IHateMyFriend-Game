using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody2D))]
public class BallAutoLauncher : NetworkBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    Rigidbody2D rb;
    bool launched = false;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    void Start()
    {
        if (GameStateManager.Instance)
            GameStateManager.Instance.GameStarted.OnValueChanged += OnGameStartedChanged;

        
        if (IsServer && GameStateManager.Instance && GameStateManager.Instance.GameStarted.Value)
            Launch();
    }

    void OnDestroy()
    {
        if (GameStateManager.Instance)
            GameStateManager.Instance.GameStarted.OnValueChanged -= OnGameStartedChanged;
    }

    void OnGameStartedChanged(bool oldV, bool newV)
    {
        if (IsServer && newV) Launch();
    }

    void Launch()
    {
        if (launched) return;
        launched = true;
        rb.linearVelocity = Vector2.up * moveSpeed;
    }
}
