using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionManager : MonoBehaviour
{
    [Header("���̵� ���� ��ư")]
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
    public AudioSource audioSource; // �׽�Ʈ�� (����)

    void Start()
    {
        // ����� ���� �ҷ�����
        LoadSettings();

        // �����̴� �̺�Ʈ ����
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        // ��Ӵٿ� �̺�Ʈ ����
        if (qualityDropdown != null)
        {
            qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        }

        // �ʱ� ���� ǥ��
        UpdateVolumeDisplay();

        // ���̵� ��ư �̺�Ʈ ����
        if (difficultyEasyButton != null)
            difficultyEasyButton.onClick.AddListener(() => SetDifficulty(DifficultySettings.Difficulty.Easy));

        if (difficultyNormalButton != null)
            difficultyNormalButton.onClick.AddListener(() => SetDifficulty(DifficultySettings.Difficulty.Normal));

        if (difficultyHardButton != null)
            difficultyHardButton.onClick.AddListener(() => SetDifficulty(DifficultySettings.Difficulty.Hard));

        // ����� ���̵� �ҷ�����
        if (DifficultySettings.Instance != null)
        {
            DifficultySettings.Instance.LoadSavedDifficulty();
            UpdateDifficultyText();
        }
    }

    // ���� ����
    public void OnVolumeChanged(float value)
    {
        // ���� ���� ����
        AudioListener.volume = value;

        // ǥ�� ������Ʈ
        UpdateVolumeDisplay();

        // ����
        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();
    }

    // ���� ǥ�� ������Ʈ
    void UpdateVolumeDisplay()
    {
        if (volumeValue != null && volumeSlider != null)
        {
            int percentage = Mathf.RoundToInt(volumeSlider.value * 100f);
            volumeValue.text = percentage + "%";
        }
    }

    // �׷��� ǰ�� ����
    public void OnQualityChanged(int index)
    {
        QualitySettings.SetQualityLevel(index);

        // ����
        PlayerPrefs.SetInt("QualityLevel", index);
        PlayerPrefs.Save();

        Debug.Log("�׷��� ǰ�� ����: " + index);
    }

    // �ɼ� �г� ����
    public void OpenOptions()
    {
        if (optionPanel != null)
        {
            optionPanel.SetActive(true);
        }
    }

    // �ɼ� �г� �ݱ�
    public void CloseOptions()
    {
        if (optionPanel != null)
        {
            optionPanel.SetActive(false);
        }
    }

    // ���� �ҷ�����
    void LoadSettings()
    {
        // ���� �ҷ�����
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
        }
        AudioListener.volume = savedVolume;

        // �׷��� ǰ�� �ҷ�����
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
            // ���Ұ�
            PlayerPrefs.SetFloat("TempVolume", volumeSlider.value);
            volumeSlider.value = 0;
        }
        else
        {
            // ���Ұ� ����
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
            currentDifficultyText.text = $"����: {DifficultySettings.Instance.GetDifficultyName()}";
        }
    }
}