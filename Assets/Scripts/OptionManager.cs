using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionManager : MonoBehaviour
{
    [Header("난이도 선택 버튼")]
    public Button difficultyEasyButton;
    public Button difficultyNormalButton;
    public Button difficultyHardButton;
    public Text currentDifficultyText;

    [Header("UI References")]
    public GameObject optionPanel;
    public Slider volumeSlider;
    public TextMeshProUGUI volumeValue;
    public TMP_Dropdown qualityDropdown;

    [Header("Audio")]
    public AudioSource audioSource; // 테스트용 (선택)

    void Start()
    {
        // 저장된 설정 불러오기
        LoadSettings();

        // 슬라이더 이벤트 연결
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        // 드롭다운 이벤트 연결
        if (qualityDropdown != null)
        {
            qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        }

        // 초기 볼륨 표시
        UpdateVolumeDisplay();

        // 난이도 버튼 이벤트 연결
        if (difficultyEasyButton != null)
            difficultyEasyButton.onClick.AddListener(() => SetDifficulty(DifficultySettings.Difficulty.Easy));

        if (difficultyNormalButton != null)
            difficultyNormalButton.onClick.AddListener(() => SetDifficulty(DifficultySettings.Difficulty.Normal));

        if (difficultyHardButton != null)
            difficultyHardButton.onClick.AddListener(() => SetDifficulty(DifficultySettings.Difficulty.Hard));

        // 저장된 난이도 불러오기
        if (DifficultySettings.Instance != null)
        {
            DifficultySettings.Instance.LoadSavedDifficulty();
            UpdateDifficultyText();
        }
    }

    // 볼륨 변경
    public void OnVolumeChanged(float value)
    {
        // 전역 볼륨 설정
        AudioListener.volume = value;

        // 표시 업데이트
        UpdateVolumeDisplay();

        // 저장
        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();
    }

    // 볼륨 표시 업데이트
    void UpdateVolumeDisplay()
    {
        if (volumeValue != null && volumeSlider != null)
        {
            int percentage = Mathf.RoundToInt(volumeSlider.value * 100f);
            volumeValue.text = percentage + "%";
        }
    }

    // 그래픽 품질 변경
    public void OnQualityChanged(int index)
    {
        QualitySettings.SetQualityLevel(index);

        // 저장
        PlayerPrefs.SetInt("QualityLevel", index);
        PlayerPrefs.Save();

        Debug.Log("그래픽 품질 변경: " + index);
    }

    // 옵션 패널 열기
    public void OpenOptions()
    {
        if (optionPanel != null)
        {
            optionPanel.SetActive(true);
        }
    }

    // 옵션 패널 닫기
    public void CloseOptions()
    {
        if (optionPanel != null)
        {
            optionPanel.SetActive(false);
        }
    }

    // 설정 불러오기
    void LoadSettings()
    {
        // 볼륨 불러오기
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
        }
        AudioListener.volume = savedVolume;

        // 그래픽 품질 불러오기
        int savedQuality = PlayerPrefs.GetInt("QualityLevel", 2);
        if (qualityDropdown != null)
        {
            qualityDropdown.value = savedQuality;
        }
        QualitySettings.SetQualityLevel(savedQuality);
    }
    public void ToggleMute()
    {
        if (AudioListener.volume > 0)
        {
            // 음소거
            PlayerPrefs.SetFloat("TempVolume", volumeSlider.value);
            volumeSlider.value = 0;
        }
        else
        {
            // 음소거 해제
            float tempVolume = PlayerPrefs.GetFloat("TempVolume", 1f);
            volumeSlider.value = tempVolume;
        }
    }
    void SetDifficulty(DifficultySettings.Difficulty difficulty)
    {
        if (DifficultySettings.Instance != null)
        {
            DifficultySettings.Instance.SetDifficulty(difficulty);
            UpdateDifficultyText();
        }
    }

    void UpdateDifficultyText()
    {
        if (currentDifficultyText != null && DifficultySettings.Instance != null)
        {
            currentDifficultyText.text = $"현재: {DifficultySettings.Instance.GetDifficultyName()}";
        }
    }
}