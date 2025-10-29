using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro ���

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public AudioSource audioSource;

    private int score = 0;
    private int combo = 0;

    void Awake()
    {
        // �̱��� ����
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        // ���� ���� �� ���� ���
        audioSource.Play();
        UpdateUI();
    }

    public void Perfect()
    {
        score += 100;
        combo++;
        Debug.Log("Perfect!");
        UpdateUI();
    }

    public void Good()
    {
        score += 50;
        combo++;
        Debug.Log("Good!");
        UpdateUI();
    }

    public void Miss()
    {
        combo = 0;
        Debug.Log("Miss!");
        UpdateUI();
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;

        if (comboText != null)
            comboText.text = "Combo: " + combo;
    }
}
