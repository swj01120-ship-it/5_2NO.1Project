using UnityEngine;

public class Note : MonoBehaviour
{
    [Header("Lane Settings")]
    public int lane; // 0=A, 1=S, 2=D, 3=F

    [Header("Movement Settings")]
    public float speed = 5f;
    private float judgementLineY = -3f; // ������ ��ġ

    void Start()
    {
        Debug.Log($"��Ʈ ������! Lane: {lane}, Speed: {speed}, ��ġ: {transform.position}");
    }

    void Update()
    {
        // ��Ʈ �Ʒ��� �̵�
        transform.position += Vector3.down * speed * Time.deltaTime;

        // ȭ�� ������ ������ Miss ó��
        if (transform.position.y < judgementLineY - 1f)
        {
            Debug.Log($"��Ʈ Miss! Lane: {lane}");

            // DrumGameManager�� ������ Miss ȣ��
            if (DrumGameManager.instance != null)
            {
                DrumGameManager.instance.Miss(lane);
            }

            Destroy(gameObject);
        }
    }

    public void Hit()
    {
        // ��Ʈ�� ������ ������ �Ÿ� ���
        float distance = Mathf.Abs(transform.position.y - judgementLineY);

        Debug.Log($"��Ʈ ��Ʈ! Lane: {lane}, Distance: {distance}");

        // ����
        if (distance < 0.15f) // Great: ��0.15
        {
            if (DrumGameManager.instance != null)
                DrumGameManager.instance.Great(lane);
        }
        else if (distance < 0.35f) // Good: ��0.35
        {
            if (DrumGameManager.instance != null)
                DrumGameManager.instance.Good(lane);
        }
        else // Miss
        {
            if (DrumGameManager.instance != null)
                DrumGameManager.instance.Miss(lane);
        }

        Destroy(gameObject);
    }
}
