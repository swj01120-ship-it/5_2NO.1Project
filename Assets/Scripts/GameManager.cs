using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public Text scoreText;
    public Text comboText;
    public Text judgementText; // Perfect/Good/Miss ǥ��
    public AudioSource audioSource;
    public NoteSpawner noteSpawner;

    private int score = 0;
    private int combo = 0;
    private int maxCombo = 0;

    void Awake()
    {
        // �̱��� ����
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateUI();
        // 1�� �� ���� ����
        Invoke("StartGame", 1f);
    }

    void StartGame()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }

        if (noteSpawner != null)
        {
            noteSpawner.StartSpawning();
        }
    }

    void Update()
    {
        // �����̽��ٷ� ��Ʈ ġ��
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HitNote();
        }
    }

    void HitNote()
    {
        // ������ ��ó�� ��Ʈ ã��
        GameObject[] notes = GameObject.FindGameObjectsWithTag("Note");
        GameObject closestNote = null;
        float closestDistance = float.MaxValue;

        foreach (GameObject noteObj in notes)
        {
            float distance = Mathf.Abs(noteObj.transform.position.y - 1f);
            if (distance < closestDistance && distance < 0.5f) // ���� ����
            {
                closestDistance = distance;
                closestNote = noteObj;
            }
        }

        // ���� ����� ��Ʈ ��Ʈ
        if (closestNote != null)
        {
            Note note = closestNote.GetComponent<Note>();
            if (note != null)
            {
                note.Hit();
            }
        }
    }

    public void Perfect()
    {
        score += 100;
        combo++;
        UpdateMaxCombo();
        ShowJudgement("PERFECT!", Color.yellow);
        UpdateUI();
    }

    public void Good()
    {
        score += 50;
        combo++;
        UpdateMaxCombo();
        ShowJudgement("GOOD", Color.green);
        UpdateUI();
    }

    public void Miss()
    {
        combo = 0;
        ShowJudgement("MISS", Color.red);
        UpdateUI();
    }

    void UpdateMaxCombo()
    {
        if (combo > maxCombo)
        {
            maxCombo = combo;
        }
    }

    void ShowJudgement(string text, Color color)
    {
        if (judgementText != null)
        {
            judgementText.text = text;
            judgementText.color = color;
            CancelInvoke("ClearJudgement");
            Invoke("ClearJudgement", 0.5f);
        }
    }

    void ClearJudgement()
    {
        if (judgementText != null)
        {
            judgementText.text = "";
        }
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;

        if (comboText != null)
            comboText.text = "Combo: " + combo + " (Max: " + maxCombo + ")";
    }
}
