using UnityEngine;

public class Note : MonoBehaviour
{
    [Header("Lane Settings")]
    public int lane; // 0=A, 1=S, 2=D, 3=F

    [Header("Movement Settings")]
    public float speed = 5f;
    private float judgementLineY = -3f; // 판정선 위치

    void Start()
    {
        Debug.Log($"노트 생성됨! Lane: {lane}, Speed: {speed}, 위치: {transform.position}");
    }

    void Update()
    {
        // 노트 아래로 이동
        transform.position += Vector3.down * speed * Time.deltaTime;

        // 화면 밖으로 나가면 Miss 처리
        if (transform.position.y < judgementLineY - 1f)
        {
            Debug.Log($"노트 Miss! Lane: {lane}");

            // DrumGameManager가 있으면 Miss 호출
            if (DrumGameManager.instance != null)
            {
                DrumGameManager.instance.Miss(lane);
            }

            Destroy(gameObject);
        }
    }

    public void Hit()
    {
        // 노트와 판정선 사이의 거리 계산
        float distance = Mathf.Abs(transform.position.y - judgementLineY);

        Debug.Log($"노트 히트! Lane: {lane}, Distance: {distance}");

        // 판정
        if (distance < 0.15f) // Great: ≤0.15
        {
            if (DrumGameManager.instance != null)
                DrumGameManager.instance.Great(lane);
        }
        else if (distance < 0.35f) // Good: ≤0.35
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
