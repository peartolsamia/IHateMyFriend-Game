// UgsStartup.cs
using UnityEngine;

public class UgsStartup : MonoBehaviour
{
    async void Start()
    {
        // Oyun açýlýr açýlmaz UGS init + anon login
        await UgsBootstrap.InitAsync();
    }
}
