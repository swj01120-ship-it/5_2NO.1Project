using UnityEngine;
using UnityEngine.UI;

public class BlinkingPrompt : MonoBehaviour
{
    private Text text;
    private float speed = 2f;

    void Start()
    {
        text = GetComponent<Text>();
    }

    void Update()
    {
        if (text != null)
        {
            float alpha = Mathf.PingPong(Time.time * speed, 1f);
            Color color = text.color;
            color.a = alpha;
            text.color = color;
        }
    }
}
