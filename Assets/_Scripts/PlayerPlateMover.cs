using UnityEngine;
using Unity.Netcode;

public class PlayerPlateMover : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 10f;

    private GameObject midLine;
    private TouchActionReader touchReader;
    private Camera DownCam;
    private Camera UpCam;
    private Camera cam;


    public override void OnNetworkSpawn()
    {
        if(!IsOwner) Destroy(this);
    }

    void Start()
    {
        // find reffernces on runtime
        DownCam = GameObject.Find("DownCam")?.GetComponent<Camera>();
        UpCam = GameObject.Find("UpCam")?.GetComponent<Camera>();
        midLine = GameObject.Find("MiddleLine");
        touchReader = FindFirstObjectByType<TouchActionReader>();

        
        if (DownCam == null || UpCam == null)
            Debug.LogWarning("DownCam or UpCam not found");
        if (midLine == null)
            Debug.LogWarning("MiddleLine not found, check its actual name");
        if (touchReader == null)
            Debug.LogWarning("TouchActionReader not found");

        // figure out player is up plate or down plate
        bool isDownSide = midLine != null && transform.position.y <= midLine.transform.position.y;

        if (isDownSide)
        {
            cam = DownCam != null ? DownCam : Camera.main;
            if (DownCam != null) DownCam.enabled = true;
            if (UpCam != null) UpCam.enabled = false;
        }
        else
        {
            cam = UpCam != null ? UpCam : Camera.main;
            if (UpCam != null) UpCam.enabled = true;
            if (DownCam != null) DownCam.enabled = false;
        }

        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        if (touchReader == null || cam == null) return;

        float touchX = touchReader.CurrentX;
        float z = Mathf.Abs(cam.transform.position.z - transform.position.z);

        Vector3 world = cam.ScreenToWorldPoint(
            new Vector3(touchX, UnityEngine.Screen.height * 0.5f, z)
        );

        Vector3 target = new Vector3(world.x, transform.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * moveSpeed);
    }
}
