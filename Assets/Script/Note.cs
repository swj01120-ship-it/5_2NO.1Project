using UnityEngine;

public class Note : MonoBehaviour
{
    public int lane;        // 노트가 떨어지는 레인 번호 (0~3)
    public float spawnTime; // 노트가 생성된 시간 (게임 시간 기준)

    private float hitLineY = 1f;  // 히트라인 위치 (판정선)

    // 생성될 때 설정하는 초기값 함수
    public void Init(int lane, float spawnTime)
    {
        this.lane = lane;
        this.spawnTime = spawnTime;
    }

    // 매 프레임마다 노트를 떨어뜨리는 함수
    public void MoveDown(float distance)
    {
        transform.position += Vector3.down * distance;
    }

    // ? 새로 추가: 노트 히트 판정 메서드
    public void Hit()
    {
        // 현재 노트 위치와 히트라인 사이의 거리 계산
        float distance = Mathf.Abs(transform.position.y - hitLineY);

        // 거리에 따라 판정
        if (distance < 0.1f)  // Perfect: ±0.1 이내
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.Perfect();
            }
        }
        else if (distance < 0.3f)  // Good: ±0.3 이내
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.Good();
            }
        }
        else  // Miss: 그 외
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.Miss();
            }
        }

        // 노트 오브젝트 삭제
        Destroy(gameObject);
    }
}