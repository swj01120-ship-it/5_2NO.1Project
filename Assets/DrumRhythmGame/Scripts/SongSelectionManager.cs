using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongSelectionManager : MonoBehaviour
{
    public static SongSelectionManager Instance;

    [Header("선택된 노래")]
    public SongData selectedSong; // 현재 선택된 노래

    [Header("사용 가능한 모든 노래")]
    public SongData[] availableSongs; // Inspector에서 등록

    void Awake()
    {
        // 싱글톤 패턴 - 씬 전환 시에도 유지
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시에도 삭제되지 않음
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 노래 선택
    public void SelectSong(SongData song)
    {
        selectedSong = song;
        Debug.Log($"✅ 선택된 노래: {song.songName} - {song.artist}");
    }

    // 인덱스로 노래 선택
    public void SelectSongByIndex(int index)
    {
        if (index >= 0 && index < availableSongs.Length)
        {
            SelectSong(availableSongs[index]);
        }
        else
        {
            Debug.LogError($"❌ 잘못된 노래 인덱스: {index}");
        }
    }

    // 선택된 노래 가져오기
    public SongData GetSelectedSong()
    {
        return selectedSong;
    }

    // 사용 가능한 노래 목록 가져오기
    public SongData[] GetAvailableSongs()
    {
        return availableSongs;
    }
}
