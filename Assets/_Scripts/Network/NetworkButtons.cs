using UnityEngine;
using Unity.Netcode;

public class NetworkButtons : MonoBehaviour
{
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        try
        {
            var nm = NetworkManager.Singleton;

            if (nm == null)
            {
                GUILayout.Label("No NetworkManager in the scene.");
                return;
            }

            if (!nm.IsClient && !nm.IsServer)
            {
                if (GUILayout.Button("Host")) nm.StartHost();
                if (GUILayout.Button("Server")) nm.StartServer();
                if (GUILayout.Button("Client")) nm.StartClient();
            }
        }
        finally
        {
            GUILayout.EndArea();
        }
    }
}
