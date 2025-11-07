// RelayLobbyConnector.cs (UTP "uzun parametreli" sürüm)
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;

using System.Collections;
using System.Threading.Tasks;

// Çakışmaları önlemek için alias
using Debug = UnityEngine.Debug;
using Application = UnityEngine.Application;

public class RelayLobbyConnector : MonoBehaviour
{
    [Header("UI")]
    public TMPro.TMP_InputField joinCodeInput;
    public TMPro.TMP_Text joinCodeText;
    public UnityEngine.UI.Button hostBtn;
    public UnityEngine.UI.Button joinBtn;

    bool _busy = false;

    Lobby currentLobby;
    Coroutine heartbeatCo;

    void Awake()
    {
        Debug.Log("[RLC] Awake çağrıldı");

        Application.runInBackground = true;

        
    }

    void Start()
    {
        Debug.Log("[RLC] Start çağrıldı");
    }

    public async void OnHostButton()
    {
        
        if (_busy) { Debug.LogWarning("[RLC] Meşgulken Host tekrar çağrıldı, yok sayıldı."); return; }
        _busy = true;
        try {
            Debug.Log("[RLC] OnHostButton tetiklendi"); 
            await StartHostFlow(); 
        }

        finally { _busy = false; }
    }

    public async void OnJoinButton()
    {

        if (_busy) { Debug.LogWarning("[RLC] Meşgulken Join tekrar çağrıldı, yok sayıldı."); return; }
        _busy = true;
        try {
            var code = joinCodeInput ? joinCodeInput.text : "";
            Debug.Log($"[RLC] OnJoinButton tetiklendi, code='{code}'");
            await StartClientFlow(code);
        }
        finally { _busy = false; }
    }

    async Task StartHostFlow()
    {
        if (NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
            await System.Threading.Tasks.Task.Delay(100);
        }

        Debug.Log("▶ StartHostFlow başladı");
        await UgsBootstrap.InitAsync();
        Debug.Log("▶ UGS init tamam, Relay allocation deneniyor...");

        try
        {
            // 1) Relay allocation (2 oyuncu)
            Allocation alloc = await RelayService.Instance.CreateAllocationAsync(2);
            Debug.Log($"Allocation OK: {alloc.AllocationId}");

            // 2) Join code
            string code = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);
            if (joinCodeText) joinCodeText.text = $"CODE: {code}";
            Debug.Log($"[Relay] Host code: {code}");

            //var probe = await RelayService.Instance.JoinAllocationAsync(code);
            //Debug.Log($"[Probe] Join code GEÇERLİ. ip={probe.RelayServer.IpV4}:{probe.RelayServer.Port}");

            // 3) Lobby oluştur (kodu metadata'ya yaz)
            currentLobby = await LobbyService.Instance.CreateLobbyAsync(
                "PongRoom", 2,
                new CreateLobbyOptions
                {
                    IsPrivate = false,
                    Data = new System.Collections.Generic.Dictionary<string, DataObject> {
                    { "joincode", new DataObject(DataObject.VisibilityOptions.Public, code) }
                    }
                });

            // 4) Lobby heartbeat
            heartbeatCo = StartCoroutine(LobbyHeartbeat());

            // 5) UTP'yi Relay'e yönlendir (uzun parametreli overload)
            var utp = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
            utp.SetRelayServerData(
                alloc.RelayServer.IpV4,
                (ushort)alloc.RelayServer.Port,
                alloc.AllocationIdBytes,
                alloc.Key,
                alloc.ConnectionData,
                alloc.ConnectionData, // host için hostConnectionData = kendi connectionData
                false                  // DTLS (secure)
            );

            // 6) Host başlat
            NetworkManager.Singleton.StartHost();
            Debug.Log("[NGO] Host started.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Relay] CreateAllocationAsync hata: {e.Message}\n{e}");
            //Debug.LogError($"[Probe] Join code GEÇERSİZ! -> {e.Message}");
        }
    }


    async Task StartClientFlow(string code)
    {
        await UgsBootstrap.InitAsync();

        if (string.IsNullOrWhiteSpace(code))
        {
            Debug.LogError("[Relay] Join code boş.");
            return;
        }

        // 1) (opsiyonel) Lobby join
        //try { await LobbyService.Instance.JoinLobbyByCodeAsync(code); }
        //catch { Debug.LogWarning("[Lobby] Join opsiyoneldi, atlandı."); }

        // 2) Relay join
        JoinAllocation joinAlloc = await RelayService.Instance.JoinAllocationAsync(code);

        // 3) UTP'yi Relay'e yönlendir (UZUN PARAMETRELİ overload)
        var utp = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        utp.SetRelayServerData(
            joinAlloc.RelayServer.IpV4,
            (ushort)joinAlloc.RelayServer.Port,
            joinAlloc.AllocationIdBytes,
            joinAlloc.Key,
            joinAlloc.ConnectionData,
            joinAlloc.HostConnectionData, // host’tan gelen
            false                           // DTLS (secure)
        );

        NetworkManager.Singleton.StartClient();
        Debug.Log("[NGO] Client started.");
    }

    IEnumerator LobbyHeartbeat()
    {
        var wait = new WaitForSeconds(30f);
        while (currentLobby != null)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
            yield return wait;
        }
    }

    async void OnApplicationQuit()
    {
        if (currentLobby != null)
        {
            try { await LobbyService.Instance.DeleteLobbyAsync(currentLobby.Id); }
            catch { /* sessiz geç */ }
        }
    }

    void OnDestroy()
    {
        if (heartbeatCo != null) StopCoroutine(heartbeatCo);
    }


    public void SetJoinCodeFromGUI(string code)
    {
        if (joinCodeInput) joinCodeInput.text = code;
    }

    public async void OnJoinButtonWithCode(string code)
    {


        if (_busy) { Debug.LogWarning("[RLC] Meşgulken Join tekrar çağrıldı, yok sayıldı."); return; }
        _busy = true;
        try
        {
            code = (code ?? "").Trim().ToUpperInvariant();
            Debug.Log($"[RLC] OnJoinButtonWithCode tetiklendi, code='{code}'");
            if (string.IsNullOrWhiteSpace(code))
            {
                Debug.LogError("[Relay] Join code boş.");
                return;
            }
            await StartClientFlow(code);
        }
        finally { _busy = false; }
    }

}
