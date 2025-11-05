using UnityEngine;

public class MissBall : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            Debug.Log("----- ball got out ------");
            Destroy(collision.gameObject);
        }
    }
}
