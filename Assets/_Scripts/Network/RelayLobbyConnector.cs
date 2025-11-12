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


using Debug = UnityEngine.Debug;
using Application = UnityEngine.Application;


public class RelayLobbyConnector : MonoBehaviour
{
    static readonly System.Text.RegularExpressions.Regex s_joinCodeRx = new System.Text.RegularExpressions.Regex("^[6789BCDFGHJKLMNPQRTWbcdfghjklmnpqrtw]{6,12}$");

    [Header("UI")]
    public TMPro.TMP_InputField joinCodeInput;
    public TMPro.TMP_Text joinCodeText;
    public UnityEngine.UI.Button hostBtn;
    public UnityEngine.UI.Button joinBtn;

    bool _busy = false;

    Lobby currentLobby;
    Coroutine heartbeatCo;

    [SerializeField] GameObject lobbyUIRoot;

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

        if (_busy) { Debug.LogWarning("[RLC] Meşgulken Join tekrar çağrıldı"); return; }
        _busy = true;
        try
        {
            var raw = joinCodeInput ? joinCodeInput.text : "";
            var code = NormalizeJoinCode(raw);
            Debug.Log($"[RLC] OnJoinButton, code='{code}'");

            if (!IsValidJoinCode(code))
            {
                Debug.LogError("[Relay] Join code hatalı formatta. 6-12 uzunluk, sadece 6789BCDFGHJKLMNPQRTW karakterleri.");
                return;
            }

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
            
            Allocation alloc = await RelayService.Instance.CreateAllocationAsync(2);
            Debug.Log($"Allocation OK: {alloc.AllocationId}");

            
            string code = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);
            if (joinCodeText) joinCodeText.text = $"CODE: {code}";
            Debug.Log($"[Relay] Host code: {code}");

            
            currentLobby = await LobbyService.Instance.CreateLobbyAsync(
                "PongRoom", 2,
                new CreateLobbyOptions
                {
                    IsPrivate = false,
                    Data = new System.Collections.Generic.Dictionary<string, DataObject> {
                    { "joincode", new DataObject(DataObject.VisibilityOptions.Public, code) }
                    }
                });

            
            heartbeatCo = StartCoroutine(LobbyHeartbeat());

            
            var utp = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
            utp.SetRelayServerData(
                alloc.RelayServer.IpV4,
                (ushort)alloc.RelayServer.Port,
                alloc.AllocationIdBytes,
                alloc.Key,
                alloc.ConnectionData,
                alloc.ConnectionData, 
                false                 
            );

            
            NetworkManager.Singleton.StartHost();
            Debug.Log("[NGO] Host started.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Relay] CreateAllocationAsync hata: {e.Message}\n{e}");
        }
    }


    async Task StartClientFlow(string code)
    {
        await UgsBootstrap.InitAsync();

        code = NormalizeJoinCode(code);
        if (string.IsNullOrWhiteSpace(code) || !IsValidJoinCode(code))
        {
            Debug.LogError("[Relay] Join code boş/format dışı.");
            return;
        }


        JoinAllocation joinAlloc = await RelayService.Instance.JoinAllocationAsync(code);


        var utp = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        utp.SetRelayServerData(
            joinAlloc.RelayServer.IpV4,
            (ushort)joinAlloc.RelayServer.Port,
            joinAlloc.AllocationIdBytes,
            joinAlloc.Key,
            joinAlloc.ConnectionData,
            joinAlloc.HostConnectionData, 
            false                           
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
            catch { }
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

    void OnEnable()
    {
        if (GameStateManager.Instance)
            GameStateManager.Instance.GameStarted.OnValueChanged += OnGameStartedChanged;
    }
    void OnDisable()
    {
        if (GameStateManager.Instance)
            GameStateManager.Instance.GameStarted.OnValueChanged -= OnGameStartedChanged;
    }
    void OnGameStartedChanged(bool oldV, bool newV)
    {
        if (newV && lobbyUIRoot) lobbyUIRoot.SetActive(false);
    }


    
    static string NormalizeJoinCode(string raw)
    {
        raw = (raw ?? "").Trim();             
        raw = raw.Replace(" ", "");         
        raw = raw.Replace("CODE:", "", System.StringComparison.OrdinalIgnoreCase); 
        return raw.ToUpperInvariant();         
    }

    static bool IsValidJoinCode(string code) => s_joinCodeRx.IsMatch(code);

}
