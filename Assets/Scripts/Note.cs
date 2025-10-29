using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    public float speed = 2f; // 노트가 떨어지는 속도
    private bool hasBeenHit = false;

    void Update()
    {
        // 노트를 아래로 이동
        transform.Translate(Vector3.down * speed * Time.deltaTime);

        // 판정선을 지나치면 Miss 처리하고 삭제
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

            // 판정선과의 거리로 정확도 판정
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
