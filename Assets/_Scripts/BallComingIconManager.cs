using UnityEngine;

public class BallComingIconManager : MonoBehaviour
{
    private GameObject ball;
    private Rigidbody2D ballRigidBody;
    private GameObject midLine;

    [SerializeField] private GameObject iconPrefab;
    private GameObject iconInstance;

    private int ballSide; // if ball is at up screen 1, if ball is at down screen -1


    void Start()
    {
        ball = GameObject.Find("Ball");
        ballRigidBody = GameObject.Find("Ball").GetComponent<Rigidbody2D>();
        midLine = GameObject.Find("MiddleLine");
        

        iconInstance = Instantiate(iconPrefab, new Vector3(13, 13, 0), Quaternion.identity);
        iconInstance.SetActive(false);

    }

    void Update()
    {

        ballSide = findBall();


        if (ballIsComing() && ballSide == -1 && ballRigidBody.linearVelocity.y > 0) // ball is going up (now its at down screen)
        {
            iconInstance.SetActive(true);
            iconInstance.transform.position = new Vector3(ball.transform.position.x, midLine.transform.position.y + 1, 0);
            iconInstance.transform.rotation = Quaternion.Euler(180, 0, 0);
        }
        else if (ballIsComing() && ballSide == 1 && ballRigidBody.linearVelocity.y < 0) // ball is going down (now its at up screen)
        {
            iconInstance.SetActive(true);
            iconInstance.transform.position = new Vector3(ball.transform.position.x, midLine.transform.position.y - 1, 0);
            iconInstance.transform.rotation = Quaternion.identity;
        }
        else
        {
            iconInstance.SetActive(false);
        }
    }


    private int findBall()
    {
        if (ball.transform.position.y > midLine.transform.position.y) // ball is at up player's screen
        {
            return 1;
        }
        else if (ball.transform.position.y < midLine.transform.position.y) // ball is at down player's screen
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }

    private bool ballIsComing()
    {
        if (ball.transform.position.y <= midLine.transform.position.y + 4 && ball.transform.position.y >= midLine.transform.position.y - 4)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
