using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SongButton : MonoBehaviour
{
    [Header("노래 데이터")]
    public SongData songData; // 이 버튼이 나타내는 노래

    [Header("UI 요소 - Legacy Text")]
    public Text songNameText; // 노래 제목
    public Text artistText; // 아티스트명
    public Image coverImage; // 커버 이미지
    public Text difficultyText; // 난이도 텍스트
    public Image[] difficultyStars; // 난이도 별 (1~5개)

    [Header("씬 설정")]
    public string gameSceneName = "GameScene"; // 게임 씬 이름

    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnSongButtonClicked);
        }

        // UI 업데이트
        UpdateUI();
    }

    // UI 업데이트
    void UpdateUI()
    {
        if (songData == null) return;

        // 노래 제목
        if (songNameText != null)
        {
            songNameText.text = songData.songName;
        }

        // 아티스트
        if (artistText != null)
        {
            artistText.text = songData.artist;
        }

        // 커버 이미지
        if (coverImage != null && songData.coverImage != null)
        {
            coverImage.sprite = songData.coverImage;
        }

        // 난이도 텍스트
        if (difficultyText != null)
        {
            difficultyText.text = songData.difficultyText;
        }

        // 난이도 별 표시
        if (difficultyStars != null && difficultyStars.Length > 0)
        {
            for (int i = 0; i < difficultyStars.Length; i++)
            {
                if (difficultyStars[i] != null)
                {
                    // 난이도만큼 별 활성화
                    difficultyStars[i].gameObject.SetActive(i < songData.difficulty);
                }
            }
        }
    }

    // 노래 선택 버튼 클릭
    void OnSongButtonClicked()
    {
        if (songData == null)
        {
            Debug.LogError("❌ 노래 데이터가 없습니다!");
            return;
        }

        // SongSelectionManager에 선택된 노래 저장
        if (SongSelectionManager.Instance != null)
        {
            SongSelectionManager.Instance.SelectSong(songData);
        }
        else
        {
            Debug.LogError("❌ SongSelectionManager를 찾을 수 없습니다!");
            return;
        }

        // 게임 씬으로 이동
        Debug.Log($"🎵 {songData.songName} 선택! 게임 씬으로 이동합니다.");
        SceneManager.LoadScene(gameSceneName);
    }

    // Inspector에서 노래 데이터 설정 시 UI 즉시 업데이트
    void OnValidate()
    {
        UpdateUI();
    }
}