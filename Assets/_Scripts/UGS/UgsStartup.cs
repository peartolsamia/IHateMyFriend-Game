using UnityEngine;

public class UgsStartup : MonoBehaviour
{
    async void Start()
    {
        await UgsBootstrap.InitAsync();
    }
}
