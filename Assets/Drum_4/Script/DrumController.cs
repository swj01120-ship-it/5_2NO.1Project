using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DrumController : MonoBehaviour
{
    [Header("Judgement Lines")]
    public Transform[] judgementLines; // 4개 판정선 위치

    [Header("Drum Visual Feedback")]
    public GameObject[] drumObjects; // 드럼 3D 모델 또는 스프라이트 (4개)
    public Material drumNormalMaterial;
    public Material drumHitMaterial;
    private float drumHitDuration = 0.1f;

    [Header("Input Keys")]
    public KeyCode[] inputKeys = new KeyCode[4]
    {
        KeyCode.A,
        KeyCode.S,
        KeyCode.D,
        KeyCode.F
    };

    void Update()
    {
        // 각 레인의 입력 확인
        for (int lane = 0; lane < 4; lane++)
        {
            if (Input.GetKeyDown(inputKeys[lane]))
            {
                CheckNoteHit(lane);
                ShowDrumHit(lane);
            }
        }
    }

    void CheckNoteHit(int lane)
    {
        // 해당 레인의 모든 노트 찾기
        GameObject[] allNotes = GameObject.FindGameObjectsWithTag("Note");
        Note closestNote = null;
        float closestDistance = float.MaxValue;

        foreach (GameObject noteObj in allNotes)
        {
            Note note = noteObj.GetComponent<Note>();
            if (note != null && note.lane == lane)
            {
                // 판정선과의 거리 계산
                float distance = Mathf.Abs(noteObj.transform.position.y - judgementLines[lane].position.y);

                // 판정 가능 범위 내에서 가장 가까운 노트 찾기
                if (distance < 0.5f && distance < closestDistance)
                {
                    closestDistance = distance;
                    closestNote = note;
                }
            }
        }

        // 가장 가까운 노트를 히트
        if (closestNote != null)
        {
            closestNote.Hit();
        }
    }

    void ShowDrumHit(int lane)
    {
        if (drumObjects != null && lane < drumObjects.Length && drumObjects[lane] != null)
        {
            StartCoroutine(DrumHitEffect(lane));
        }
    }

    System.Collections.IEnumerator DrumHitEffect(int lane)
    {
        // 드럼 색상 변경 (히트 효과)
        Renderer renderer = drumObjects[lane].GetComponent<Renderer>();
        if (renderer != null && drumHitMaterial != null)
        {
            Material originalMaterial = renderer.material;
            renderer.material = drumHitMaterial;

            yield return new WaitForSeconds(drumHitDuration);

            renderer.material = originalMaterial;
        }

        // 또는 스케일 애니메이션
        Transform drumTransform = drumObjects[lane].transform;
        Vector3 originalScale = drumTransform.localScale;
        drumTransform.localScale = originalScale * 1.1f;

        yield return new WaitForSeconds(drumHitDuration);

        drumTransform.localScale = originalScale;
    }
}
