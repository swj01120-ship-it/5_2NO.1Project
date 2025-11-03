using UnityEngine;
using System.Collections.Generic;

public class StickManager : MonoBehaviour
{
    public Animator left_stick;   // 왼쪽 스틱 Animator
    public Animator right_stick;  // 오른쪽 스틱 Animator

    // 각 스틱의 현재 위치 (0=A, 1=S, 2=D, 3=F)
    private int leftStickPosition = 0;   // 왼손은 A에서 시작
    private int rightStickPosition = 3;  // 오른손은 F에서 시작

    void Update()
    {
        // 현재 프레임에서 입력된 키 확인
        List<int> pressedDrums = new List<int>();

        if (Input.GetKeyDown(KeyCode.A)) pressedDrums.Add(0);
        if (Input.GetKeyDown(KeyCode.S)) pressedDrums.Add(1);
        if (Input.GetKeyDown(KeyCode.D)) pressedDrums.Add(2);
        if (Input.GetKeyDown(KeyCode.F)) pressedDrums.Add(3);

        // 입력 개수에 따라 처리
        if (pressedDrums.Count == 1)
        {
            // 단일 드럼: 가까운 스틱이 친다
            HitSingleDrum(pressedDrums[0]);
        }
        else if (pressedDrums.Count == 2)
        {
            // 2개 동시: 왼쪽 드럼은 왼손, 오른쪽 드럼은 오른손
            HitDoubleDrum(pressedDrums[0], pressedDrums[1]);
        }
        else if (pressedDrums.Count > 2)
        {
            // 3개 이상: 가장 왼쪽과 오른쪽만 친다
            pressedDrums.Sort();
            HitDoubleDrum(pressedDrums[0], pressedDrums[pressedDrums.Count - 1]);
        }
    }

    // 단일 드럼을 칠 때: 가까운 스틱 선택
    void HitSingleDrum(int drumIndex)
    {
        int leftDistance = Mathf.Abs(drumIndex - leftStickPosition);
        int rightDistance = Mathf.Abs(drumIndex - rightStickPosition);

        // 거리 비교 후 가까운 스틱 선택
        if (leftDistance < rightDistance)
        {
            // 왼쪽 스틱이 더 가깝다
            HitDrumWithStick(left_stick, drumIndex, true);
        }
        else if (rightDistance < leftDistance)
        {
            // 오른쪽 스틱이 더 가깝다
            HitDrumWithStick(right_stick, drumIndex, false);
        }
        else
        {
            // 거리가 같으면 드럼 위치로 결정 (A,S는 왼손, D,F는 오른손)
            if (drumIndex <= 1)
            {
                HitDrumWithStick(left_stick, drumIndex, true);
            }
            else
            {
                HitDrumWithStick(right_stick, drumIndex, false);
            }
        }
    }

    // 2개 드럼을 동시에 칠 때: 왼손은 왼쪽 드럼, 오른손은 오른쪽 드럼
    void HitDoubleDrum(int drum1, int drum2)
    {
        // 작은 값(왼쪽)과 큰 값(오른쪽) 구분
        int leftDrum = Mathf.Min(drum1, drum2);
        int rightDrum = Mathf.Max(drum1, drum2);

        // 왼손 → 왼쪽 드럼
        HitDrumWithStick(left_stick, leftDrum, true);

        // 오른손 → 오른쪽 드럼
        HitDrumWithStick(right_stick, rightDrum, false);

        Debug.Log($"Double Hit: Left stick → Drum_{GetDrumName(leftDrum)}, Right stick → Drum_{GetDrumName(rightDrum)}");
    }

    // 스틱으로 드럼 치기
    void HitDrumWithStick(Animator stick, int drumIndex, bool isLeftStick)
    {
        string drumName = GetDrumName(drumIndex);
        string triggerName = "Drum_" + drumName;

        stick.SetTrigger(triggerName);

        // 스틱 위치 업데이트
        if (isLeftStick)
        {
            leftStickPosition = drumIndex;
        }
        else
        {
            rightStickPosition = drumIndex;
        }

        Debug.Log($"{(isLeftStick ? "Left" : "Right")} stick hit {triggerName}");
    }

    // 드럼 인덱스를 이름으로 변환
    string GetDrumName(int drumIndex)
    {
        switch (drumIndex)
        {
            case 0: return "A";
            case 1: return "S";
            case 2: return "D";
            case 3: return "F";
            default: return "A";
        }
    }
}