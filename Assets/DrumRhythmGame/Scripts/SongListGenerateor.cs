using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SongListGenerator : MonoBehaviour
{
    [Header("설정")]
    public GameObject songButtonPrefab; // SongButton 프리팹
    public Transform contentParent; // Scroll View의 Content

    void Start()
    {
        GenerateSongList();
    }

    void GenerateSongList()
    {
        if (SongSelectionManager.Instance == null)
        {
            Debug.LogError("❌ SongSelectionManager를 찾을 수 없습니다!");
            return;
        }

        if (songButtonPrefab == null)
        {
            Debug.LogError("❌ SongButton 프리팹이 설정되지 않았습니다!");
            return;
        }

        if (contentParent == null)
        {
            Debug.LogError("❌ Content Parent가 설정되지 않았습니다!");
            return;
        }

        // 기존 버튼들 삭제
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 모든 노래 가져오기
        SongData[] songs = SongSelectionManager.Instance.GetAvailableSongs();

        // 각 노래마다 버튼 생성
        foreach (SongData song in songs)
        {
            if (song != null)
            {
                // 버튼 생성
                GameObject buttonObj = Instantiate(songButtonPrefab, contentParent);

                // SongButton 컴포넌트 가져오기
                SongButton songButton = buttonObj.GetComponent<SongButton>();

                if (songButton != null)
                {
                    songButton.songData = song;
                    Debug.Log($"✅ 노래 버튼 생성: {song.songName}");
                }
            }
        }

        Debug.Log($"✅ 총 {songs.Length}개의 노래 버튼 생성 완료!");
    }
}
