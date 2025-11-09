using UnityEngine;

[CreateAssetMenu(fileName = "New Song", menuName = "Rhythm Game/Song Data")]
public class SongData : ScriptableObject
{
    [Header("노래 정보")]
    public string songName = "노래 제목";
    public string artist = "아티스트";
    public Sprite coverImage; // 노래 커버 이미지

    [Header("게임 데이터")]
    public AudioClip musicClip; // 음악 파일
    public BeatChart beatChart; // 비트 차트

    [Header("난이도")]
    public int difficulty = 1; // 1~5 별로 표시 가능
    public string difficultyText = "Normal"; // Easy, Normal, Hard

    [Header("추가 정보")]
    [TextArea]
    public string description = "노래 설명";
    public float duration = 0f; // 곡 길이 (초)
}
