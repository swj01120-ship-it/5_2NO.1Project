using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DrumGameManager : MonoBehaviour
{
    public static DrumGameManager instance;

    [Header("UI")]
    public Text scoreText;
    public Text comboText;
    public Text judgementText;
    public Text[] laneComboTexts; // ���κ� �޺� �ؽ�Ʈ (4��)

    [Header("Effects")]
    public GameObject greatEffect;
    public GameObject goodEffect;
    public GameObject missEffect;
    public Transform[] effectSpawnPoints; // ����Ʈ ���� ��ġ (4��)

    [Header("Audio")]
    public AudioSource bgmSource;
    public AudioClip hitSound;
    private AudioSource hitSoundSource;

    [Header("Game Settings")]
    public NoteSpawner noteSpawner;

    private int score = 0;
    private int totalCombo = 0;
    private int maxCombo = 0;
    private int[] laneCombo = new int[4]; // ���κ� �޺�

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        // ��Ʈ ����� AudioSource �߰�
        hitSoundSource = gameObject.AddComponent<AudioSource>();
        hitSoundSource.playOnAwake = false;
    }

    void Start()
    {
        UpdateUI();
        StartCoroutine(StartGameWithDelay());
    }

    IEnumerator StartGameWithDelay()
    {
        yield return new WaitForSeconds(2f);
        if (bgmSource != null)
            bgmSource.Play();
    }

    public void Great(int lane)
    {
        score += 150;
        totalCombo++;
        laneCombo[lane]++;
        UpdateMaxCombo();
        CameraShake.instance.Shake(0.1f, 0.15f);

        ShowJudgement("GREAT!", new Color(1f, 0.84f, 0f)); // �ݻ�
        PlayEffect(greatEffect, lane);
        PlayHitSound();
        UpdateUI();
    }

    public void Good(int lane)
    {
        score += 100;
        totalCombo++;
        laneCombo[lane]++;
        UpdateMaxCombo();

        ShowJudgement("GOOD", Color.green);
        PlayEffect(goodEffect, lane);
        PlayHitSound();
        UpdateUI();
    }

    public void Miss(int lane)
    {
        totalCombo = 0;
        laneCombo[lane] = 0;

        ShowJudgement("MISS", Color.red);
        PlayEffect(missEffect, lane);
        UpdateUI();
    }

    void UpdateMaxCombo()
    {
        if (totalCombo > maxCombo)
            maxCombo = totalCombo;
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
            judgementText.text = "";
    }

    void PlayEffect(GameObject effectPrefab, int lane)
    {
        if (effectPrefab != null && effectSpawnPoints != null && lane < effectSpawnPoints.Length)
        {
            Vector3 pos = effectSpawnPoints[lane].position;
            GameObject effect = Instantiate(effectPrefab, pos, Quaternion.identity);
            Destroy(effect, 1f);
        }
    }

    void PlayHitSound()
    {
        if (hitSound != null && hitSoundSource != null)
        {
            hitSoundSource.PlayOneShot(hitSound);
        }
    }

    void UpdateUI()
    {
        // ����
        if (scoreText != null)
            scoreText.text = "Score: " + score.ToString("N0");

        // �� �޺�
        if (comboText != null)
        {
            comboText.text = "Combo: " + totalCombo + " (Max: " + maxCombo + ")";

            // �޺��� ���� ũ��� ���� ����
            if (totalCombo >= 50)
            {
                comboText.fontSize = 70;
                comboText.color = new Color(1f, 0f, 0.5f); // ��ũ
            }
            else if (totalCombo >= 30)
            {
                comboText.fontSize = 65;
                comboText.color = Color.red;
            }
            else if (totalCombo >= 10)
            {
                comboText.fontSize = 60;
                comboText.color = Color.yellow;
            }
            else
            {
                comboText.fontSize = 60;
                comboText.color = Color.white;
            }
        }

        // ���κ� �޺�
        if (laneComboTexts != null)
        {
            for (int i = 0; i < 4 && i < laneComboTexts.Length; i++)
            {
                if (laneComboTexts[i] != null)
                {
                    if (laneCombo[i] > 0)
                    {
                        laneComboTexts[i].text = laneCombo[i].ToString();
                        laneComboTexts[i].color = Color.yellow;
                    }
                    else
                    {
                        laneComboTexts[i].text = "";
                    }
                }
            }
        }
    }

    public int GetScore() { return score; }
    public int GetCombo() { return totalCombo; }
    public int GetMaxCombo() { return maxCombo; }
}
