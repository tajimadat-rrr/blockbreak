using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class Ball : MonoBehaviour
{
    [SerializeField] private Transform paddle;

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip wallHitSE;  //壁当たった時

    private bool isLaunched = false;  //発射中か

    private bool isGameOver = false;  //ゲーム中か

    private bool canLaunch = false;  //開始前は発射できない

    private Rigidbody2D rb;


    private float speed = 5f;

    private float maxSpeed = 8f;
    private float minScale = 0.5f;
    private float maxScale = 1.5f;


    private Vector3 initialScale;


    [SerializeField] private float horizontalLimit = 0.3f;
    [SerializeField] private float horizontalTimeLimit = 3f;

    private float horizontalTimer = 0f;

    // スタック対策
    [SerializeField] private float stuckTimeLimit = 2f;
    [SerializeField] private float stuckDistance = 0.3f;

    private float stuckTimer = 0f;
    private Vector3 lastCheckPosition;





    private static int activeBallCount = 1;

    private const int maxBallCount = 5;

    private bool isSubBall = false;

    private TrailRenderer trailRenderer;




    private void Awake()
    {
        trailRenderer = GetComponent<TrailRenderer>();

        rb = GetComponent<Rigidbody2D>();

        initialScale = transform.localScale;

        lastCheckPosition = transform.position;

        if (trailRenderer != null)
        {
            // 軌跡の太さ
            trailRenderer.widthMultiplier = 0.05f;

            // 軌跡が残る時間
            trailRenderer.time = 0.12f;

            // 軌跡をリセット
            trailRenderer.Clear();

            // 発射前は表示しない
            trailRenderer.emitting = false;
        }
    }





    public void InitializeBallCount()
    {
        activeBallCount = 1;
    }



    // Update is called once per frame
    void Update()
    {
        if(isGameOver)
        {
            return;
        }


        if (isLaunched == false)
        {
            transform.position =
                new Vector3(paddle.position.x, paddle.position.y + 0.45f, 0);

            if (Mouse.current != null &&
                Mouse.current.leftButton.wasPressedThisFrame &&
                canLaunch)
            {
                isLaunched = true;

                if (trailRenderer != null)
                {
                    trailRenderer.Clear();
                    trailRenderer.emitting = true;
                }

                float randomX = Random.Range(-3f, 3f);

                Vector2 direction =
                    new Vector2(randomX, 5f).normalized;

                rb.linearVelocity = direction * speed;
            }
        }

       
    }


    private void FixedUpdate()
    {
        if (!isLaunched || isGameOver)
        {
            horizontalTimer = 0f;
            stuckTimer = 0f;
            lastCheckPosition = transform.position;
            return;
        }

        Vector2 velocity = rb.linearVelocity;

        float totalSpeed = velocity.magnitude;

        if (totalSpeed < 0.1f)
        {
            horizontalTimer = 0f;
            return;
        }

        // ★ ボールの速度を常に一定にする
        rb.linearVelocity = velocity.normalized * speed;

        // 横移動が続いているか確認
        float verticalRatio =
            Mathf.Abs(velocity.y) / totalSpeed;

        if (verticalRatio < horizontalLimit)
        {
            horizontalTimer += Time.fixedDeltaTime;

            if (horizontalTimer >= horizontalTimeLimit)
            {
                ForceDown();
                horizontalTimer = 0f;
            }
        }
        else
        {
            horizontalTimer = 0f;
        }

        // スタックしているか確認
        stuckTimer += Time.fixedDeltaTime;

        if (stuckTimer >= stuckTimeLimit)
        {
            float distance = Vector3.Distance(
                transform.position,
                lastCheckPosition
            );

            if (distance < stuckDistance)
            {
                EscapeStuck();
            }

            lastCheckPosition = transform.position;
            stuckTimer = 0f;
        }
    }



    private void ForceDown()
    {
        float randomX = Random.Range(-0.8f, 0.8f);

        Vector2 direction =
            new Vector2(randomX, -1f).normalized;

        rb.linearVelocity = direction * speed;

        Debug.Log("横移動が3秒続いたため強制下降");
    }




    //下の板との反射処理
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("衝突：" + collision.gameObject.name);

        // 壁に衝突したとき
        if (collision.gameObject.CompareTag("Wall"))
        {
            if (audioSource != null && wallHitSE != null)
            {
                audioSource.PlayOneShot(wallHitSE);
            }
        }

        // Paddleとの反射処理
        if (collision.gameObject.CompareTag("Paddle"))
        {
            float hitPosition =
                transform.position.x - collision.transform.position.x;

            Vector2 direction =
                new Vector2(hitPosition, 1f).normalized;

            rb.linearVelocity = direction * speed;
        }
    }


    private void EscapeStuck()
    {
        Debug.Log("スタック検知！ ボールを脱出させます");

        // 中央に向かう方向
        float centerDirection = transform.position.x > 0f ? -1f : 1f;

        Vector2 direction = new Vector2(
            centerDirection * 0.8f,
            -1f
        ).normalized;

        rb.linearVelocity = direction * speed;

        // 壁やブロックとの重なりから少し離す
        rb.position += direction * 0.15f;

        stuckTimer = 0f;
        horizontalTimer = 0f;
        lastCheckPosition = transform.position;
    }


    public void BallMiss()
    {
        if (!isLaunched || isGameOver)
        {
            return;
        }

        isLaunched = false;
        rb.linearVelocity = Vector2.zero;

        // ピンクの追加ボールが落下した場合
        if (isSubBall)
        {
            activeBallCount = Mathf.Max(0, activeBallCount - 1);

            Destroy(gameObject);
            return;
        }

        // 白いメインボールが落下した場合
        Debug.Log("メインボール落下！");

        // すべての追加ボールを消す
        Ball[] balls = FindObjectsByType<Ball>(
            FindObjectsSortMode.None
        );

        foreach (Ball b in balls)
        {
            if (b != this && b.isSubBall)
            {
                Destroy(b.gameObject);
            }
        }

        // ボール数をリセット
        activeBallCount = 1;

        // ライフを1減らす
        GameManager manager =
            FindFirstObjectByType<GameManager>();

        if (manager != null)
        {
            manager.MissBall();

            // ライフが残っている場合
            if (manager.HasLife())
            {
                ResetBall();
            }
        }
    }

    public void ResetBall()
    {
        if (isSubBall)
        {
            return;
        }

        gameObject.SetActive(true);

        // 復活時はボール1個から再開
        activeBallCount = 1;

        isLaunched = false;
        isGameOver = false;
        canLaunch = true;

        horizontalTimer = 0f;

        // 速度と大きさを初期状態に戻す
        speed = 5f;
        transform.localScale = initialScale;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        transform.position =
            new Vector3(
                paddle.position.x,
                paddle.position.y + 0.45f,
                0f
            );

        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
            trailRenderer.Clear();
        }
    }


    public void Startball()
    {
        canLaunch = true;
    }



    public void StopBall()
    {
        isGameOver = true;
        rb.linearVelocity = Vector2.zero;
    }



    //速度変更


    public void AddSpeed()
    {
        speed = Mathf.Min(speed + 0.5f, maxSpeed);

        if (isLaunched)
        {
            rb.linearVelocity =
                rb.linearVelocity.normalized * speed;
        }

        Debug.Log("現在のBall速度 : " + speed);
    }




    //弾を縮小

    public void ShrinkBall()
    {
        float newScale = transform.localScale.x * 0.95f;

        newScale = Mathf.Max(newScale, minScale);

        transform.localScale =
            new Vector3(newScale, newScale, 1f);
    }



    //弾を拡大

    public void GrowBall()
    {
        // 現在の大きさを1.05倍
        float newScale = transform.localScale.x * 1.15f;

        // 最大サイズを制限
        newScale = Mathf.Min(newScale, maxScale);

        transform.localScale =
            new Vector3(newScale, newScale, 1f);

        Debug.Log(gameObject.name + " サイズ : " + newScale);
    }


    //弾を増やす

    public void MultiBall()
    {
        if (!isLaunched || isGameOver)
        {
            return;
        }

        int createCount =
            Mathf.Min(2, maxBallCount - activeBallCount);

        for (int i = 0; i < createCount; i++)
        {
            GameObject newBall =
                Instantiate(
                    gameObject,
                    transform.position,
                    Quaternion.identity
                );

            //追加ボールのサイズを初期サイズに戻す
            newBall.transform.localScale = initialScale;

            Ball newBallScript = newBall.GetComponent<Ball>();

            newBallScript.initialScale = initialScale;

            newBallScript.isSubBall = true;
            newBallScript.isLaunched = true;

            if (newBallScript.trailRenderer != null)
            {
                newBallScript.trailRenderer.Clear();
                newBallScript.trailRenderer.emitting = true;
            }


            newBallScript.canLaunch = false;
            newBallScript.horizontalTimer = 0f;

            // 追加ボールだけシルバーに変更
            SpriteRenderer sprite =
                newBall.GetComponent<SpriteRenderer>();

            if (sprite != null)
            {
                sprite.color = new Color(1f, 0.4f, 0.7f, 1f);
            }

            Rigidbody2D newRb =
                newBall.GetComponent<Rigidbody2D>();

            float angle = i == 0 ? 25f : -25f;

            Vector2 direction =
                Quaternion.Euler(0f, 0f, angle) *
                rb.linearVelocity.normalized;

            newRb.linearVelocity = direction * speed;

            activeBallCount++;
        }

        Debug.Log("現在のボール数 : " + activeBallCount);
    }
}
