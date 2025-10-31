using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DrumController : MonoBehaviour
{
    [Header("Judgement Lines")]
    public Transform[] judgementLines; // 4�� ������ ��ġ

    [Header("Drum Visual Feedback")]
    public GameObject[] drumObjects; // �巳 3D �� �Ǵ� ��������Ʈ (4��)
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
        // �� ������ �Է� Ȯ��
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
        // �ش� ������ ��� ��Ʈ ã��
        GameObject[] allNotes = GameObject.FindGameObjectsWithTag("Note");
        Note closestNote = null;
        float closestDistance = float.MaxValue;

        foreach (GameObject noteObj in allNotes)
        {
            Note note = noteObj.GetComponent<Note>();
            if (note != null && note.lane == lane)
            {
                // ���������� �Ÿ� ���
                float distance = Mathf.Abs(noteObj.transform.position.y - judgementLines[lane].position.y);

                // ���� ���� ���� ������ ���� ����� ��Ʈ ã��
                if (distance < 0.5f && distance < closestDistance)
                {
                    closestDistance = distance;
                    closestNote = note;
                }
            }
        }

        // ���� ����� ��Ʈ�� ��Ʈ
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
        // �巳 ���� ���� (��Ʈ ȿ��)
        Renderer renderer = drumObjects[lane].GetComponent<Renderer>();
        if (renderer != null && drumHitMaterial != null)
        {
            Material originalMaterial = renderer.material;
            renderer.material = drumHitMaterial;

            yield return new WaitForSeconds(drumHitDuration);

            renderer.material = originalMaterial;
        }

        // �Ǵ� ������ �ִϸ��̼�
        Transform drumTransform = drumObjects[lane].transform;
        Vector3 originalScale = drumTransform.localScale;
        drumTransform.localScale = originalScale * 1.1f;

        yield return new WaitForSeconds(drumHitDuration);

        drumTransform.localScale = originalScale;
    }
}
