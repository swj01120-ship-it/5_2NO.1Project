using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("마스터 볼륨")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Text masterVolumeText;

    [Header("SFX (드럼) 볼륨")]
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Text sfxVolumeText;

    [Header("PlayerPrefs 키")]
    [SerializeField] private string masterVolumeKey = "MasterVolume";
    [SerializeField] private string sfxVolumeKey = "SFXVolume";

    void Start()
    {
        // 저장된 볼륨 값 불러오기
        LoadVolumes();

        // 슬라이더 이벤트 연결
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    void LoadVolumes()
    {
        // 마스터 볼륨 불러오기 (기본값 0.75)
        float masterVolume = PlayerPrefs.GetFloat(masterVolumeKey, 0.75f);
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = masterVolume;
        }
        SetMasterVolume(masterVolume);

        // SFX 볼륨 불러오기 (기본값 0.75)
        float sfxVolume = PlayerPrefs.GetFloat(sfxVolumeKey, 0.75f);
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = sfxVolume;
        }
        SetSFXVolume(sfxVolume);
    }

    public void SetMasterVolume(float volume)
    {
        // 볼륨을 dB로 변환 (-80dB ~ 0dB)
        float dB = volume > 0.0001f ? 20f * Mathf.Log10(volume) : -80f;
        audioMixer.SetFloat("MasterVolume", dB);

        // UI 텍스트 업데이트
        if (masterVolumeText != null)
        {
            masterVolumeText.text = Mathf.RoundToInt(volume * 100) + "%";
        }

        // 설정 저장
        PlayerPrefs.SetFloat(masterVolumeKey, volume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        // 볼륨을 dB로 변환
        float dB = volume > 0.0001f ? 20f * Mathf.Log10(volume) : -80f;
        audioMixer.SetFloat("SFXVolume", dB);

        // UI 텍스트 업데이트
        if (sfxVolumeText != null)
        {
            sfxVolumeText.text = Mathf.RoundToInt(volume * 100) + "%";
        }

        // 설정 저장
        PlayerPrefs.SetFloat(sfxVolumeKey, volume);
        PlayerPrefs.Save();
    }

    // 뮤트 기능 (선택사항)
    public void ToggleMute()
    {
        float currentVolume;
        audioMixer.GetFloat("MasterVolume", out currentVolume);

        if (currentVolume > -79f)
        {
            // 뮤트
            audioMixer.SetFloat("MasterVolume", -80f);
        }
        else
        {
            // 뮤트 해제
            float savedVolume = PlayerPrefs.GetFloat(masterVolumeKey, 0.75f);
            SetMasterVolume(savedVolume);
        }
    }

    // 디버그용 - 현재 볼륨 출력
    public void LogCurrentVolumes()
    {
        float masterVol, sfxVol;
        audioMixer.GetFloat("MasterVolume", out masterVol);
        audioMixer.GetFloat("SFXVolume", out sfxVol);

        Debug.Log($"🔊 Master: {masterVol}dB, SFX: {sfxVol}dB");
    }
}