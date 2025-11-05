using UnityEngine;

public class BallComingIconManager : MonoBehaviour
{
    private GameObject ball;
    private Transform midLine;

    [SerializeField] private GameObject iconPrefab;
    private GameObject iconInstance;

    [Header("Server state")]
    [SerializeField] private BallStateSync state; 

    void Start()
    {
        ball = GameObject.Find("Ball");
        midLine = GameObject.Find("MiddleLine").transform;

        iconInstance = Instantiate(iconPrefab, new Vector3(13, 13, 0), Quaternion.identity);
        iconInstance.SetActive(false);
    }

    void Update()
    {
        
        bool inBand = state.InBand.Value;
        sbyte side = state.BallSide.Value;  // -1 is down player screem, +1 up player screen
        sbyte vdir = state.VertDir.Value;   // -1 going down, 0 stopping, +1 going up

        if (inBand && side == -1 && vdir == 1) // ball is going up from down screen
        {
            iconInstance.SetActive(true);
            iconInstance.transform.position = new Vector3(ball.transform.position.x, midLine.position.y + 1f, 0f);
            iconInstance.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
        }
        else if (inBand && side == 1 && vdir == -1) // ball is going down from up screen
        {
            iconInstance.SetActive(true);
            iconInstance.transform.position = new Vector3(ball.transform.position.x, midLine.position.y - 1f, 0f);
            iconInstance.transform.rotation = Quaternion.identity;
        }
        else
        {
            iconInstance.SetActive(false);
        }
    }
}
