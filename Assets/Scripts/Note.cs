using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    public float speed = 2f; // ��Ʈ�� �������� �ӵ�
    private bool hasBeenHit = false;

    void Update()
    {
        // ��Ʈ�� �Ʒ��� �̵�
        transform.Translate(Vector3.down * speed * Time.deltaTime);

        // �������� ����ġ�� Miss ó���ϰ� ����
        if (transform.position.y < 0.5f && !hasBeenHit)
        {
            GameManager.instance.Miss();
            Destroy(gameObject);
        }
    }

    public void Hit()
    {
        if (!hasBeenHit)
        {
            hasBeenHit = true;

            // ���������� �Ÿ��� ��Ȯ�� ����
            float distance = Mathf.Abs(transform.position.y - 1.5f);

            if (distance < 0.1f)
            {
                GameManager.instance.Perfect();
            }
            else if (distance < 0.3f)
            {
                GameManager.instance.Good();
            }
            else
            {
                GameManager.instance.Miss();
            }

            Destroy(gameObject);
        }
    }
}
