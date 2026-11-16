using UnityEngine;

public class MissArea : MonoBehaviour
{

    private GameManager gameManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
    }




    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            Ball ball = collision.GetComponent<Ball>();

            if (ball != null)
            {
                ball.BallMiss();
            }
        }
    }

}
