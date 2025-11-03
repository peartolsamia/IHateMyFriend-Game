using UnityEngine;
using Unity.Netcode;

public class Ball : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private GameObject midLine;

    void Start()
    {
        midLine = GameObject.Find("MiddleLine");

        GetComponent<Rigidbody2D>().linearVelocity = Vector2.up * moveSpeed;
    }

    float hitFactor(Vector2 ballPos, Vector2 platePos, float plateHeight)
    {
        return (ballPos.x - platePos.x) / plateHeight;
    }



    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.CompareTag("Plate")) // ball collided with a plate (player) object
        {
            if (collision.transform.position.y < midLine.transform.position.y) // collided plate's y-axis value is smaller than middle line's y-axix value -> plate is DownPlate
            {
                float x = hitFactor(transform.position, collision.transform.position, collision.collider.bounds.size.x);

                Vector2 direction = new Vector2(x, 1).normalized; // for down plate, bounce ball positive y-axis (up) and x direction

                GetComponent<Rigidbody2D>().linearVelocity = direction * moveSpeed;
            }

            else if (collision.transform.position.y > midLine.transform.position.y) // collided plate's y-axis value is bigger than middle line's y-axix value -> plate is UpPlate
            {
                float x = hitFactor(transform.position, collision.transform.position, collision.collider.bounds.size.x);

                Vector2 direction = new Vector2(x, -1).normalized; // for up plate, bounce ball negative y-axis (down) and x direction

                GetComponent<Rigidbody2D>().linearVelocity = direction * moveSpeed;
            }
        }

        
    }
}
