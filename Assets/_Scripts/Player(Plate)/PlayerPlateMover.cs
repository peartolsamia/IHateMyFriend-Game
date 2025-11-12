using UnityEngine;
using Unity.Netcode;

public class PlayerPlateMover : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 10f;

    
    [SerializeField] private GameObject lobbyUIRoot;

    private GameObject midLine;
    private TouchActionReader touchReader;
    private Camera DownCam;
    private Camera UpCam;
    private Camera cam;
    private bool cameraArmed = false; 

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { Destroy(this); return; }
    }

    void Start()
    {
        DownCam = GameObject.Find("DownCam")?.GetComponent<Camera>();
        UpCam = GameObject.Find("UpCam")?.GetComponent<Camera>();
        midLine = GameObject.Find("MiddleLine");
        touchReader = FindFirstObjectByType<TouchActionReader>();

        
        if (DownCam) DownCam.enabled = false;
        if (UpCam) UpCam.enabled = false;

        
        if (GameStateManager.Instance != null)
        {
            
            if (GameStateManager.Instance.GameStarted.Value)
                ArmGameCameraAndHideUI();

            GameStateManager.Instance.GameStarted.OnValueChanged += OnGameStartedChanged;
        }
    }

    void OnDestroy()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.GameStarted.OnValueChanged -= OnGameStartedChanged;
    }

    void OnGameStartedChanged(bool oldV, bool newV)
    {
        if (newV) ArmGameCameraAndHideUI();
    }

    void ArmGameCameraAndHideUI()
    {
        if (cameraArmed) return;
        cameraArmed = true;

        bool isDownSide = midLine != null && transform.position.y <= midLine.transform.position.y;

        if (isDownSide)
        {
            cam = DownCam != null ? DownCam : Camera.main;
            if (DownCam) DownCam.enabled = true;
            if (UpCam) UpCam.enabled = false;
        }
        else
        {
            cam = UpCam != null ? UpCam : Camera.main;
            if (UpCam) UpCam.enabled = true;
            if (DownCam) DownCam.enabled = false;
        }
        if (cam == null) cam = Camera.main;

        
        if (lobbyUIRoot) lobbyUIRoot.SetActive(false);
    }

    void Update()
    {
        
        if (!cameraArmed || touchReader == null || cam == null) return;

        float touchX = touchReader.CurrentX;
        float z = Mathf.Abs(cam.transform.position.z - transform.position.z);

        Vector3 world = cam.ScreenToWorldPoint(new Vector3(touchX, UnityEngine.Screen.height * 0.5f, z));
        Vector3 target = new Vector3(world.x, transform.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * moveSpeed);
    }
}
