using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource seSource;
    [SerializeField] private AudioSource bgmSource;

    [Header("SE")]
    [SerializeField] private AudioClip blockBreakSE;

    [SerializeField] private AudioClip lifeSE;
    [SerializeField] private AudioClip bombSE;
    [SerializeField] private AudioClip bigBallSE;
    [SerializeField] private AudioClip multiBallSE;
    [SerializeField] private AudioClip speedUpSE;
    [SerializeField] private AudioClip highScoreSE;

    [Header("BGM")]
    [SerializeField] private AudioClip titleBGM;
    [SerializeField] private AudioClip battleBGM;
    [SerializeField] private AudioClip gameOverBGM;

    private void Awake()
    {
        Instance = this;

        seSource.playOnAwake = false;
        seSource.spatialBlend = 0f;
        seSource.volume = 1f;

        bgmSource.playOnAwake = false;
        bgmSource.spatialBlend = 0f;
        bgmSource.volume = 0.3f;
        bgmSource.loop = true;
    }

    public void PlayBlockBreak(BlockType type)
    {
        AudioClip clip = blockBreakSE;

        switch (type)
        {
            case BlockType.Life:
                clip = lifeSE;
                break;

            case BlockType.Bomb:
                clip = bombSE;
                break;

            case BlockType.BigBall:
                clip = bigBallSE;
                break;

            case BlockType.MultiBall:
                clip = multiBallSE;
                break;

            case BlockType.SpeedUp:
                clip = speedUpSE;
                break;

            case BlockType.HighScore:
                clip = highScoreSE;
                break;
        }

        // êÍópSEÇ™ñ¢ê›íËÇ»ÇÁí èÌîjâÛSEÇégóp
        if (clip == null)
        {
            clip = blockBreakSE;
        }

        if (clip != null && seSource != null)
        {
            seSource.PlayOneShot(clip, 1f);
        }
    }

    public void PlayTitleBGM()
    {
        PlayBGM(titleBGM, true);
    }

    public void PlayBattleBGM()
    {
        PlayBGM(battleBGM, true);
    }

    public void PlayGameOverBGM()
    {
        PlayBGM(gameOverBGM, false);
    }

    private void PlayBGM(AudioClip clip, bool loop)
    {
        bgmSource.Stop();

        if (clip == null)
        {
            return;
        }

        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void PauseBGM()
    {
        bgmSource.Pause();
    }

    public void ResumeBGM()
    {
        bgmSource.UnPause();
    }
}