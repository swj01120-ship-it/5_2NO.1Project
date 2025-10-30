using UnityEngine;

public class Note : MonoBehaviour
{
    public int lane;        // ��Ʈ�� �������� ���� ��ȣ (0~3)
    public float spawnTime; // ��Ʈ�� ������ �ð� (���� �ð� ����)

    private float hitLineY = 1f;  // ��Ʈ���� ��ġ (������)

    // ������ �� �����ϴ� �ʱⰪ �Լ�
    public void Init(int lane, float spawnTime)
    {
        this.lane = lane;
        this.spawnTime = spawnTime;
    }

    // �� �����Ӹ��� ��Ʈ�� ����߸��� �Լ�
    public void MoveDown(float distance)
    {
        transform.position += Vector3.down * distance;
    }

    // ? ���� �߰�: ��Ʈ ��Ʈ ���� �޼���
    public void Hit()
    {
        // ���� ��Ʈ ��ġ�� ��Ʈ���� ������ �Ÿ� ���
        float distance = Mathf.Abs(transform.position.y - hitLineY);

        // �Ÿ��� ���� ����
        if (distance < 0.1f)  // Perfect: ��0.1 �̳�
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.Perfect();
            }
        }
        else if (distance < 0.3f)  // Good: ��0.3 �̳�
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.Good();
            }
        }
        else  // Miss: �� ��
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.Miss();
            }
        }

        // ��Ʈ ������Ʈ ����
        Destroy(gameObject);
    }
}