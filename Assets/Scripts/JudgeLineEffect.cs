using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeLineEffect : MonoBehaviour
{
    public Material lineMaterial;
    public float pulseSpeed = 2f;
    public float minIntensity = 1f;
    public float maxIntensity = 3f;

    private float currentIntensity;

    void Start()
    {
        if (lineMaterial == null)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                lineMaterial = renderer.material;
            }
        }
    }

    void Update()
    {
        if (lineMaterial != null)
        {
            // Ping-Pong���� �Դٰ����ϴ� ȿ��
            currentIntensity = Mathf.Lerp(minIntensity, maxIntensity,
                Mathf.PingPong(Time.time * pulseSpeed, 1f));

            // Emission ���� ����
            Color emissionColor = Color.red * currentIntensity;
            lineMaterial.SetColor("_EmissionColor", emissionColor);
        }
    }
}
