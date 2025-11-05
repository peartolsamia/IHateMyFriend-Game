using UnityEngine;
using UnityEngine.InputSystem;

public class TouchActionReader : MonoBehaviour
{
    [SerializeField] private InputAction touchXPosition;

    public float CurrentX { get; private set; }

    void OnEnable()
    {
        touchXPosition.Enable();
    }

    void OnDisable()
    {
        touchXPosition.Disable();
    }

    void Update()
    {
        CurrentX = touchXPosition.ReadValue<float>();
        //Debug.Log($"Touch X position: {CurrentX}");
    }
}
