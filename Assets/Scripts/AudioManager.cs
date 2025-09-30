using UnityEngine;

// 전역에서 사운드를 관리하기 위한 싱글톤 클래스
public class AudioManager : MonoBehaviour
{
    // 외부에서 참조할 수 있도록 static 인스턴스 생성
    public static AudioManager Instance;

    // 배경음악 재생용 AudioSource
    public AudioSource bgmSource;
    // 효과음 재생용 AudioSource
    public AudioSource sfxSource;
    public AudioClip defaultBGM;

    // Awake는 오브젝트가 생성될 때 실행됨
    void Awake()
    {
        // 인스턴스가 없으면 이 오브젝트를 인스턴스로 사용
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 파괴되지 않게 설정
        }
        else Destroy(gameObject); // 이미 있다면 중복 제거
    }

    void Start()
    {
        PlayBGM(defaultBGM);
    }

    // 배경음악 재생 메서드
    public void PlayBGM(AudioClip clip, float volume = 0.2f)
    {
        bgmSource.clip = clip;     // 클립 지정
        bgmSource.loop = true;     // 루프 재생
        bgmSource.volume = volume; // 볼륨 설정
        bgmSource.Play();          // 재생 시작
    }

    // 효과음 재생 메서드 (겹쳐 재생 가능)
    public void PlaySFX(AudioClip clip, float volume = 1.0f)
    {
        if (clip != null)
            sfxSource.PlayOneShot(clip, volume); // 한 번만 재생
    }
}