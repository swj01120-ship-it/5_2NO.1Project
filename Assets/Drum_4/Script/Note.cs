using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    public int lane; // 0=A, 1=S, 2=D, 3=F
    public float speed = 5f;
    private float judgementLineY = -3f; // ������ ��ġ

    void Update()
    {
        // ��Ʈ �Ʒ��� �̵�
        transform.position += Vector3.down * speed * Time.deltaTime;

        // ȭ�� ������ ������ Miss ó��
        if (transform.position.y < judgementLineY - 1f)
        {
            DrumGameManager.instance.Miss(lane);
            Destroy(gameObject);
        }
    }

    public void Hit()
    {
        // ��Ʈ�� ������ ������ �Ÿ� ���
        float distance = Mathf.Abs(transform.position.y - judgementLineY);

        // ����
        if (distance < 0.15f) // Great: ��0.15
        {
            DrumGameManager.instance.Great(lane);
        }
        else if (distance < 0.35f) // Good: ��0.35
        {
            DrumGameManager.instance.Good(lane);
        }
        else // Miss
        {
            DrumGameManager.instance.Miss(lane);
        }

        Destroy(gameObject);
    }
}
