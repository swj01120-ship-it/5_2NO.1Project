using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultySettings : MonoBehaviour
{
    // 싱글톤 (씬 전환 시에도 유지)
    public static DifficultySettings Instance;

    public enum Difficulty
    {
        Easy,
        Normal,
        Hard
    }

    [Header("현재 선택된 난이도")]
    public Difficulty currentDifficulty = Difficulty.Normal;

    [Header("난이도별 설정 (전체 하향 조정)")]

    // Easy 설정 (초보자용 - 매우 느림)
    public float easyBPM = 80f;          // 96 → 80 (더 느리게)
    public float easyDensity = 0.4f;     // 0.5 → 0.4 (노트 40%)
    public float easyPerfectWindow = 0.25f;  // 0.2 → 0.25 (더 여유롭게)
    public float easyGreatWindow = 0.4f;     // 0.35 → 0.4
    public float easyGoodWindow = 0.6f;      // 0.5 → 0.6
    public float easyHighlightDuration = 1.0f; // 0.8 → 1.0 (더 길게 표시)

    // Normal 설정 (적당함 - 편안한 속도)
    public float normalBPM = 95f;        // 112 → 95 (17 감소)
    public float normalDensity = 0.6f;   // 0.7 → 0.6 (노트 60%)
    public float normalPerfectWindow = 0.18f;  // 0.15 → 0.18
    public float normalGreatWindow = 0.3f;     // 0.25 → 0.3
    public float normalGoodWindow = 0.45f;     // 0.35 → 0.45
    public float normalHighlightDuration = 0.75f; // 0.6 → 0.75

    // Hard 설정 (도전적 - 기존 Normal 수준)
    public float hardBPM = 110f;         // 128 → 110 (18 감소)
    public float hardDensity = 0.8f;     // 1.0 → 0.8 (노트 80%)
    public float hardPerfectWindow = 0.12f;  // 0.1 → 0.12
    public float hardGreatWindow = 0.2f;     // 0.15 → 0.2
    public float hardGoodWindow = 0.3f;      // 0.2 → 0.3
    public float hardHighlightDuration = 0.5f; // 0.4 → 0.5

    void Awake()
    {
        // 싱글톤 설정 (씬 전환 시에도 유지)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 난이도 설정
    public void SetDifficulty(Difficulty difficulty)
    {
        currentDifficulty = difficulty;

        string difficultyName = "";
        switch (difficulty)
        {
            case Difficulty.Easy:
                difficultyName = "쉬움 😊 (80 BPM - 여유로운 템포)";
                break;
            case Difficulty.Normal:
                difficultyName = "보통 🙂 (95 BPM - 편안한 템포)";
                break;
            case Difficulty.Hard:
                difficultyName = "어려움 😰 (110 BPM - 도전적)";
                break;
        }

        Debug.Log($"🎯 난이도 설정: {difficultyName}");

        // PlayerPrefs에 저장 (다음에도 유지)
        PlayerPrefs.SetInt("SelectedDifficulty", (int)difficulty);
        PlayerPrefs.Save();
    }

    // 저장된 난이도 불러오기
    public void LoadSavedDifficulty()
    {
        if (PlayerPrefs.HasKey("SelectedDifficulty"))
        {
            int savedDifficulty = PlayerPrefs.GetInt("SelectedDifficulty");
            currentDifficulty = (Difficulty)savedDifficulty;
            Debug.Log($"💾 저장된 난이도 불러오기: {currentDifficulty}");
        }
    }

    // 현재 난이도의 BPM 가져오기
    public float GetBPM()
    {
        switch (currentDifficulty)
        {
            case Difficulty.Easy: return easyBPM;
            case Difficulty.Normal: return normalBPM;
            case Difficulty.Hard: return hardBPM;
            default: return normalBPM;
        }
    }

    // 현재 난이도의 Density 가져오기
    public float GetDensity()
    {
        switch (currentDifficulty)
        {
            case Difficulty.Easy: return easyDensity;
            case Difficulty.Normal: return normalDensity;
            case Difficulty.Hard: return hardDensity;
            default: return normalDensity;
        }
    }

    // 현재 난이도의 판정 시간들 가져오기
    public void GetJudgmentWindows(out float perfect, out float great, out float good)
    {
        switch (currentDifficulty)
        {
            case Difficulty.Easy:
                perfect = easyPerfectWindow;
                great = easyGreatWindow;
                good = easyGoodWindow;
                break;
            case Difficulty.Normal:
                perfect = normalPerfectWindow;
                great = normalGreatWindow;
                good = normalGoodWindow;
                break;
            case Difficulty.Hard:
                perfect = hardPerfectWindow;
                great = hardGreatWindow;
                good = hardGoodWindow;
                break;
            default:
                perfect = normalPerfectWindow;
                great = normalGreatWindow;
                good = normalGoodWindow;
                break;
        }
    }


    // 현재 난이도의 강조 지속 시간 가져오기
    public float GetHighlightDuration()
    {
        switch (currentDifficulty)
        {
            case Difficulty.Easy: return easyHighlightDuration;
            case Difficulty.Normal: return normalHighlightDuration;
            case Difficulty.Hard: return hardHighlightDuration;
            default: return normalHighlightDuration;
        }
    }

    // 난이도 이름 가져오기 (UI 표시용)
    public string GetDifficultyName()
    {
        switch (currentDifficulty)
        {
            case Difficulty.Easy: return "쉬움 😊";
            case Difficulty.Normal: return "보통 🙂";
            case Difficulty.Hard: return "어려움 😰";
            default: return "보통 🙂";
        }
    }
}
