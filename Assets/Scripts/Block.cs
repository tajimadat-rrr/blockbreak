using UnityEngine;


public enum BlockType
{
    Normal,   //white
    Life,   //yerrow
    HighScore, //orange
    BigBall,  //light blue
    MultiBall,  //green
    Bomb,  //red
    SpeedUp  // purple speed ip
}



public class Block : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private BlockSpawner blockSpawner;

    private GameManager gameManager;

    private Ball ball;


    private Renderer blockRenderer;

    private bool hasExploded = false; //もう爆発したか

    private bool isDestroyed = false;  //もう破壊されたか

    private BlockType blockType;


    [Header("Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip blockBreakSE;
    [SerializeField] private float breakVolume = 0.4f;


    [Header("Particle")]
    [SerializeField] private ParticleSystem breakParticlePrefab;

    [SerializeField] private ParticleSystem bombParticlePrefab;  //exeplosion particle prefab



    private void Start()
    {
        //FindFirstobject Sceneないからオブジェクト名参照
        //コードで入れるか、インスペクターでいれるか
        blockSpawner = FindFirstObjectByType<BlockSpawner>();
        gameManager = FindFirstObjectByType<GameManager>();
       // ball = FindFirstObjectByType<Ball>();
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (isDestroyed)
        {
            return;
        }



        if (collision.gameObject.CompareTag("Ball"))
        {
            isDestroyed = true;

            Ball ball = collision.gameObject.GetComponent<Ball>();

            Debug.Log("Block Hit");


            gameManager.AddCombo();


            gameManager.AddScore();


            if(blockType == BlockType.Life)
            {
                gameManager.AddLife();
            }

            if(blockType == BlockType.HighScore)
            {
                gameManager.AddHighScore();
            }

            if(blockType == BlockType.BigBall)
            {
                ball.GrowBall();
            }

            if(blockType == BlockType.MultiBall)
            {
                ball.MultiBall();
            }

            if(blockType == BlockType.Bomb)
            {
                BombBlock();
            }

            if(blockType == BlockType.SpeedUp)
            {
                ball.AddSpeed();
            }

            PlayBreakSE();
            PlayBreakParticle();

            blockSpawner.BlockDestroyed(transform.position);
            Destroy(gameObject);
        }
        
    }


    //spwnerからの設定を通す
    public void SetBlockType(BlockType type)
    {
        blockType = type; 
    }


    private void PlayBreakSE()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBlockBreak(blockType);
        }
    }


    //爆発始動時

    public void BombBlock()
    {
        if (hasExploded)
        {
            return;
        }

        hasExploded = true;

        // 爆発時に画面を揺らす
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake();
        }


        // スローモーション
        if (SlowMotionManager.Instance != null)
        {
            SlowMotionManager.Instance.PlaySlowMotion();
        }

        // 爆発フラッシュ
        if (ExplosionFlash.Instance != null)
        {
            ExplosionFlash.Instance.Flash();
        }

        PlayBombParticle();

        // 爆発範囲
        float radius = 0.8f;

        Collider2D[] hitColliders =
            Physics2D.OverlapCircleAll(transform.position, radius);

        int destroyedCount = 0;

        foreach (Collider2D hit in hitColliders)
        {
            Block nearbyBlock = hit.GetComponent<Block>();

            if (nearbyBlock == null ||
                nearbyBlock == this ||
                nearbyBlock.isDestroyed)
            {
                continue;
            }

            nearbyBlock.isDestroyed = true;

            nearbyBlock.PlayBreakSE();
            nearbyBlock.PlayBreakParticle();

            blockSpawner.BlockDestroyed(
                nearbyBlock.transform.position
            );

            gameManager.AddCombo();
            gameManager.AddScore();

            Destroy(nearbyBlock.gameObject);

            destroyedCount++;
        }

        Debug.Log("爆発で破壊したブロック数: " + destroyedCount);
    }


    public BlockType GetBlockType()
    {
        return blockType;
    }


    private void PlayBreakParticle()
    {
        if (breakParticlePrefab == null)
        {
            Debug.LogError(
                "BlockBreakParticleのPrefabが設定されていません！",
                gameObject
            );
            return;
        }

        Debug.Log("Particle生成成功：" + gameObject.name);

        ParticleSystem particle = Instantiate(
            breakParticlePrefab,
            transform.position,
            Quaternion.identity
        );

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();

        if (sprite != null)
        {
            var main = particle.main;
            main.startColor = sprite.color;
        }

        particle.Play();

        var particleMain = particle.main;

        float destroyTime =
            particleMain.duration +
            particleMain.startLifetime.constantMax + 0.1f;

        Destroy(particle.gameObject, destroyTime);
    }


    private void PlayBombParticle()
    {
        if (bombParticlePrefab == null)
        {
            return;
        }

        ParticleSystem particle = Instantiate(
            bombParticlePrefab,
            transform.position,
            Quaternion.identity
        );

        particle.Play();

        var main = particle.main;

        float destroyTime =
            main.duration +
            main.startLifetime.constantMax + 0.1f;

        Destroy(particle.gameObject, destroyTime);
    }

}
