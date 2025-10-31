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

    [Header("난이도별 설정 (128 BPM 기준)")]
    // Easy 설정 (초보자용)
    public float easyBPM = 96f;          // 128의 75%
    public float easyDensity = 0.5f;     // 노트 절반
    public float easyPerfectWindow = 0.2f;
    public float easyGreatWindow = 0.35f;
    public float easyGoodWindow = 0.5f;
    public float easyHighlightDuration = 0.8f;

    // Normal 설정 (적당함)
    public float normalBPM = 112f;       // 128의 87.5%
    public float normalDensity = 0.7f;   // 노트 70%
    public float normalPerfectWindow = 0.15f;
    public float normalGreatWindow = 0.25f;
    public float normalGoodWindow = 0.35f;
    public float normalHighlightDuration = 0.6f;

    // Hard 설정 (원곡 속도)
    public float hardBPM = 128f;         // 원곡 BPM
    public float hardDensity = 1.0f;     // 노트 100%
    public float hardPerfectWindow = 0.1f;
    public float hardGreatWindow = 0.15f;
    public float hardGoodWindow = 0.2f;
    public float hardHighlightDuration = 0.4f;

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
                difficultyName = "쉬움 😊 (96 BPM)";
                break;
            case Difficulty.Normal:
                difficultyName = "보통 🙂 (112 BPM)";
                break;
            case Difficulty.Hard:
                difficultyName = "어려움 😰 (128 BPM - 원곡)";
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
