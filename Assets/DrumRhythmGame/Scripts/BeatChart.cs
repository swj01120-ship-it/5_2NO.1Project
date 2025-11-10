using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Beat Chart", menuName = "Rhythm Game/Beat Chart")]
public class BeatChart : ScriptableObject
{
    public string chartName = "새로운 차트";
    public float bpm = 128f;
    public List<BeatNote> beats = new List<BeatNote>();

    [Header("⭐ 음악 파일 연결")]
    [Tooltip("음악 파일을 여기에 드래그하면 자동으로 길이 설정됨")]
    public AudioClip musicClip;

    [Header("음악 길이 설정")]
    [Tooltip("음악 파일의 실제 길이 (자동 설정됨)")]
    public float audioFileLength = 98f;

    [Tooltip("실제 음악이 끝나는 시간 (무음 제외) - 수동 조정 가능")]
    public float actualMusicEndTime = 95f;

    [Header("자동 생성 설정")]
    public float noteDensity = 0.5f;
    public float startTime = 2f;

    [Header("드럼 설정")]
    public int numberOfDrums = 4;

    [Header("동시 노트 제한")]
    public int maxNotesAtOnce = 2;
    public float simultaneousWindow = 0.05f;

    // ⭐ 음악 파일에서 자동으로 길이 가져오기
    [ContextMenu("Auto Set Music Length (음악에서 길이 자동 설정)")]
    public void AutoSetMusicLength()
    {
        if (musicClip == null)
        {
            Debug.LogError("❌ Music Clip이 비어있습니다! Inspector에서 음악 파일을 드래그하세요.");
            return;
        }

        audioFileLength = musicClip.length;
        actualMusicEndTime = audioFileLength; // 기본값은 파일 길이와 동일

        Debug.Log($"✅ 음악 길이 자동 설정!");
        Debug.Log($"   파일 이름: {musicClip.name}");
        Debug.Log($"   파일 길이: {audioFileLength:F2}초");
        Debug.Log($"   → 무음이 있다면 'Actual Music End Time'을 수동으로 조정하세요.");

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    [ContextMenu("Fix Existing Chart (기존 차트 수정)")]
    public void FixExistingChart()
    {
        if (beats.Count == 0)
        {
            Debug.LogWarning("차트가 비어있습니다.");
            return;
        }

        Debug.Log("==================");
        Debug.Log("🔧 기존 차트 수정");
        Debug.Log($"원본: {beats.Count}개");
        Debug.Log("==================");

        beats = beats.OrderBy(b => b.time).ToList();

        // actualMusicEndTime 이후 노트 제거
        List<BeatNote> notesToRemove = new List<BeatNote>();

        foreach (var beat in beats)
        {
            if (beat.time > actualMusicEndTime)
            {
                notesToRemove.Add(beat);
            }
        }

        int removedLateNotes = notesToRemove.Count;
        foreach (var note in notesToRemove)
        {
            beats.Remove(note);
        }

        if (removedLateNotes > 0)
        {
            Debug.Log($"🎵 음악 끝 이후 노트 {removedLateNotes}개 삭제");
        }

        // 3개 이상 겹친 것 수정
        Dictionary<float, List<BeatNote>> timeGroups = new Dictionary<float, List<BeatNote>>();

        foreach (var beat in beats)
        {
            float roundedTime = Mathf.Round(beat.time * 20f) / 20f;

            if (!timeGroups.ContainsKey(roundedTime))
            {
                timeGroups[roundedTime] = new List<BeatNote>();
            }

            timeGroups[roundedTime].Add(beat);
        }

        int removedExcessNotes = 0;
        notesToRemove.Clear();

        foreach (var group in timeGroups.OrderBy(g => g.Key))
        {
            if (group.Value.Count > maxNotesAtOnce)
            {
                for (int i = maxNotesAtOnce; i < group.Value.Count; i++)
                {
                    notesToRemove.Add(group.Value[i]);
                    removedExcessNotes++;
                }
            }
        }

        foreach (var note in notesToRemove)
        {
            beats.Remove(note);
        }

        Debug.Log("--- 결과 ---");
        Debug.Log($"음악 후 삭제: {removedLateNotes}개");
        Debug.Log($"3개 초과 삭제: {removedExcessNotes}개");
        Debug.Log($"최종: {beats.Count}개");
        Debug.Log("==================");

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif

        ValidateChart();
    }

    [ContextMenu("Generate Test Chart")]
    public void GenerateTestChart()
    {
        beats.Clear();

        float beatInterval = 60f / bpm;
        float currentTime = startTime;

        System.Random random = new System.Random();

        Debug.Log($"🎵 차트 생성: {startTime}초 ~ {actualMusicEndTime:F2}초");

        while (currentTime < actualMusicEndTime)
        {
            int howMany = random.Next(1, maxNotesAtOnce + 1);

            List<int> availableDrums = new List<int>();
            for (int d = 0; d < numberOfDrums; d++)
            {
                availableDrums.Add(d);
            }

            for (int i = 0; i < howMany && availableDrums.Count > 0; i++)
            {
                int randomIndex = random.Next(availableDrums.Count);
                int drumIndex = availableDrums[randomIndex];

                beats.Add(new BeatNote(currentTime, drumIndex, 0.6f));
                availableDrums.RemoveAt(randomIndex);
            }

            currentTime += beatInterval / noteDensity;
        }

        beats = beats.OrderBy(b => b.time).ToList();

        Debug.Log($"✅ 생성 완료: {beats.Count}개");
        if (beats.Count > 0)
        {
            Debug.Log($"   범위: {beats[0].time:F2}초 ~ {beats[beats.Count - 1].time:F2}초");
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif

        ValidateChart();
    }

    [ContextMenu("Validate Chart (검증)")]
    public void ValidateChart()
    {
        if (beats.Count == 0)
        {
            Debug.LogWarning("차트가 비어있습니다.");
            return;
        }

        Debug.Log("==================");
        Debug.Log("🔍 차트 검증");
        Debug.Log("==================");

        int lateNotes = 0;
        float latestNoteTime = 0f;

        foreach (var beat in beats)
        {
            if (beat.time > actualMusicEndTime)
            {
                lateNotes++;
            }
            if (beat.time > latestNoteTime)
            {
                latestNoteTime = beat.time;
            }
        }

        if (lateNotes > 0)
        {
            Debug.LogError($"❌ 음악 끝 ({actualMusicEndTime:F2}초) 이후 노트 {lateNotes}개!");
            Debug.LogError($"   → 'Fix Existing Chart'를 실행하세요!");
        }
        else
        {
            Debug.Log($"✅ 모든 노트가 {actualMusicEndTime:F2}초 이전");
            if (beats.Count > 0)
            {
                Debug.Log($"   마지막 노트: {latestNoteTime:F2}초");
            }
        }

        Dictionary<float, List<BeatNote>> timeGroups = new Dictionary<float, List<BeatNote>>();

        foreach (var beat in beats)
        {
            float roundedTime = Mathf.Round(beat.time * 20f) / 20f;

            if (!timeGroups.ContainsKey(roundedTime))
            {
                timeGroups[roundedTime] = new List<BeatNote>();
            }

            timeGroups[roundedTime].Add(beat);
        }

        int violations = 0;
        int slots1 = 0;
        int slots2 = 0;
        int slots3Plus = 0;

        foreach (var group in timeGroups)
        {
            int count = group.Value.Count;

            if (count == 1) slots1++;
            else if (count == 2) slots2++;
            else if (count >= 3)
            {
                slots3Plus++;
                violations++;
            }
        }

        Debug.Log($"1개: {slots1}회, 2개: {slots2}회");
        if (slots3Plus > 0)
        {
            Debug.LogError($"3개 이상: {slots3Plus}회 ❌");
        }

        if (violations == 0 && lateNotes == 0)
        {
            Debug.Log($"✅✅✅ 완벽!");
        }

        Debug.Log("==================");
    }

    [ContextMenu("Show Chart Info")]
    public void ShowChartInfo()
    {
        Debug.Log($"=== 차트 정보 ===");
        Debug.Log($"이름: {chartName}");
        if (musicClip != null)
        {
            Debug.Log($"음악: {musicClip.name}");
        }
        Debug.Log($"BPM: {bpm}");
        Debug.Log($"노트: {beats.Count}개");
        Debug.Log($"파일 길이: {audioFileLength:F2}초");
        Debug.Log($"실제 음악 끝: {actualMusicEndTime:F2}초");
        if (beats.Count > 0)
        {
            Debug.Log($"노트 범위: {beats[0].time:F2}초 ~ {beats[beats.Count - 1].time:F2}초");
        }
        Debug.Log($"================");
    }

    [ContextMenu("Clear Chart")]
    public void ClearChart()
    {
        beats.Clear();
        Debug.Log("차트가 비워졌습니다.");

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}