using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public Text scoreText;
    public Text comboText;
    public Text judgementText;
    public AudioSource audioSource;
    public NoteSpawner noteSpawner;

    // ��ƼŬ ����Ʈ
    public GameObject perfectEffect;
    public GameObject goodEffect;
    public GameObject missEffect;

    private int score = 0;
    private int combo = 0;
    private int maxCombo = 0;
    private Vector3 lastHitPosition; // ������ ��Ʈ ��ġ

    void Awake()
    {
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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HitNote();
        }
    }

    void HitNote()
    {
        GameObject[] notes = GameObject.FindGameObjectsWithTag("Note");
        GameObject closestNote = null;
        float closestDistance = float.MaxValue;

        foreach (GameObject noteObj in notes)
        {
            float distance = Mathf.Abs(noteObj.transform.position.y - 1f);
            if (distance < closestDistance && distance < 0.5f)
            {
                closestDistance = distance;
                closestNote = noteObj;
            }
        }

        if (closestNote != null)
        {
            // ��Ʈ ��ġ ����
            lastHitPosition = closestNote.transform.position;

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
        PlayEffect(perfectEffect, lastHitPosition);

        //ī�޶� ����ũ �߰�
        if(CameraShake.instance != null)
        {
            CameraShake.instance.Shake(0.1f, 0.2f);
        }

        UpdateUI();
    }

    public void Good()
    {
        score += 50;
        combo++;
        UpdateMaxCombo();
        ShowJudgement("GOOD", Color.green);
        PlayEffect(goodEffect, lastHitPosition);
        UpdateUI();
    }

    public void Miss()
    {
        combo = 0;
        ShowJudgement("MISS", Color.red);
        PlayEffect(missEffect, lastHitPosition);
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

    // ��ƼŬ ����Ʈ ���
    void PlayEffect(GameObject effectPrefab, Vector3 position)
    {
        if (effectPrefab != null)
        {
            GameObject effect = Instantiate(effectPrefab, position, Quaternion.identity);
            Destroy(effect, 1f); // 1�� �� �ڵ� ����
        }
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;

        if (comboText != null)
            comboText.text = "Combo: " + combo + " (Max: " + maxCombo + ")";

        //�޺��� �������� �ؽ�Ʈ ũ�� ����
        if(combo >= 50)
        {
            comboText.fontSize = 60;
            comboText.color = Color.red;
        }
        else if (combo >= 30)
        {
            comboText.fontSize = 50;
            comboText.color = new Color(1f, 0.5f, 0f);
        }
        else if (combo >= 10)
        {
            comboText.fontSize = 45;
            comboText.color = Color.yellow;
        }
        else
        {
            comboText.fontSize = 36;
            comboText.color = Color.white;
        }
    }
}
