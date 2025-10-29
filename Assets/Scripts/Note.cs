using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    public float speed = 3f; // 노트 떨어지는 속도
    private bool hasBeenHit = false;
    private float judgeLine = 1f; // 판정선 Y 위치

    void Update()
    {
        // 노트를 아래로 이동
        transform.Translate(Vector3.down * speed * Time.deltaTime);

        // 판정선을 너무 많이 지나치면 Miss 처리
        if (transform.position.y < judgeLine - 0.5f && !hasBeenHit)
        {
            GameManager.instance.Miss();
            Destroy(gameObject);
        }
    }

    // 키 입력으로 노트 히트
    public void Hit()
    {
        if (!hasBeenHit)
        {
            hasBeenHit = true;

            // 판정선과의 거리로 정확도 계산
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
