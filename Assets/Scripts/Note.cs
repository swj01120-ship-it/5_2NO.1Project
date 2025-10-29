using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    public float speed = 3f; // ��Ʈ �������� �ӵ�
    private bool hasBeenHit = false;
    private float judgeLine = 1f; // ������ Y ��ġ

    void Update()
    {
        // ��Ʈ�� �Ʒ��� �̵�
        transform.Translate(Vector3.down * speed * Time.deltaTime);

        // �������� �ʹ� ���� ����ġ�� Miss ó��
        if (transform.position.y < judgeLine - 0.5f && !hasBeenHit)
        {
            GameManager.instance.Miss();
            Destroy(gameObject);
        }
    }

    // Ű �Է����� ��Ʈ ��Ʈ
    public void Hit()
    {
        if (!hasBeenHit)
        {
            hasBeenHit = true;

            // ���������� �Ÿ��� ��Ȯ�� ���
            float distance = Mathf.Abs(transform.position.y - judgeLine);

            if (distance < 0.15f)
            {
                GameManager.instance.Perfect();
                Debug.Log("Perfect!");
            }
            else if (distance < 0.35f)
            {
                GameManager.instance.Good();
                Debug.Log("Good!");
            }
            else
            {
                GameManager.instance.Miss();
                Debug.Log("Miss!");
            }

            Destroy(gameObject);
        }
    }
}
