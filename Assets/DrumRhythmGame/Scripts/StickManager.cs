using UnityEngine;
using System.Collections.Generic;

public class StickManager : MonoBehaviour
{
    public Animator left_stick;   // 왼쪽 스틱 Animator
    public Animator right_stick;  // 오른쪽 스틱 Animator

    [Header("Input Settings")]
    [Tooltip("입력 버퍼 시간 (초 단위)")]
    public float inputBufferTime = 0.05f;  // 50ms

    // 현재 스틱 위치 (0=f, 1=g, 2=k, 3=l)
    private int leftStickPosition = 0;   // 왼쪽 스틱 기본 위치 f
    private int rightStickPosition = 3;  // 오른쪽 스틱 기본 위치 l

    // 입력 버퍼 (입력된 드럼 인덱스와 입력 시간)
    private Dictionary<int, float> inputBuffer = new Dictionary<int, float>();
    private float lastProcessTime = -1f;

    void Update()
    {
        float currentTime = Time.time;

        // 특정 키 입력 체크 및 입력 시간 기록
        if (Input.GetKeyDown(KeyCode.F)) inputBuffer[0] = currentTime;  // f 위치 드럼 입력
        if (Input.GetKeyDown(KeyCode.G)) inputBuffer[1] = currentTime;  // g 위치 드럼 입력
        if (Input.GetKeyDown(KeyCode.K)) inputBuffer[2] = currentTime;  // k 위치 드럼 입력
        if (Input.GetKeyDown(KeyCode.L)) inputBuffer[3] = currentTime;  // l 위치 드럼 입력

        // 유효한 입력 필터링
        List<int> validInputs = new List<int>();
        List<int> toRemove = new List<int>();

        foreach (var kvp in inputBuffer)
        {
            float timeSinceInput = currentTime - kvp.Value;

            if (timeSinceInput <= inputBufferTime)
            {
                validInputs.Add(kvp.Key);
            }
            else
            {
                toRemove.Add(kvp.Key);
            }
        }

        // 입력 처리가 필요한 경우 판단
        if (validInputs.Count > 0)
        {
            float oldestInputTime = float.MaxValue;
            foreach (int drum in validInputs)
            {
                if (inputBuffer[drum] < oldestInputTime)
                {
                    oldestInputTime = inputBuffer[drum];
                }
            }

            float timeSinceOldest = currentTime - oldestInputTime;

            bool shouldProcess = validInputs.Count >= 2 ||
                               timeSinceOldest >= inputBufferTime ||
                               (validInputs.Count == 1 && timeSinceOldest >= inputBufferTime * 0.5f);

            if (shouldProcess)
            {
                ProcessInputs(validInputs);

                inputBuffer.Clear();
                lastProcessTime = currentTime;
            }
        }

        // 시간 초과된 입력 제거
        foreach (int drum in toRemove)
        {
            inputBuffer.Remove(drum);
        }
    }

    void ProcessInputs(List<int> drums)
    {
        if (drums.Count == 0) return;

        drums.Sort();

        string drumsPressed = string.Join(", ", drums);
        Debug.Log($"<color=orange>Processing drums: [{drumsPressed}] (Count: {drums.Count})</color>");

        if (drums.Count == 1)
        {
            HitSingleDrum(drums[0]);
        }
        else if (drums.Count >= 2)
        {
            HitDoubleDrum(drums[0], drums[drums.Count - 1]);
        }
    }

    void HitSingleDrum(int drumIndex)
    {
        int leftDistance = Mathf.Abs(drumIndex - leftStickPosition);
        int rightDistance = Mathf.Abs(drumIndex - rightStickPosition);

        if (leftDistance < rightDistance)
        {
            HitDrumWithStick(left_stick, drumIndex, true);
        }
        else if (rightDistance < leftDistance)
        {
            HitDrumWithStick(right_stick, drumIndex, false);
        }
        else
        {
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

    void HitDoubleDrum(int leftDrumIndex, int rightDrumIndex)
    {
        Debug.Log($"<color=magenta>==== DOUBLE HIT ====</color>");
        Debug.Log($"<color=magenta>Left drum: {leftDrumIndex} ({GetDrumName(leftDrumIndex)}), Right drum: {rightDrumIndex} ({GetDrumName(rightDrumIndex)})</color>");

        HitDrumWithStick(left_stick, leftDrumIndex, true);
        HitDrumWithStick(right_stick, rightDrumIndex, false);

        Debug.Log($"<color=magenta>==================</color>");
    }

    void HitDrumWithStick(Animator stick, int drumIndex, bool isLeftStick)
    {
        string drumName = GetDrumName(drumIndex);
        string triggerName = "Drum_" + drumName;

        if (stick == null)
        {
            Debug.LogError($"Animator is null! Cannot play {triggerName}");
            return;
        }

        // bool 제거, 트리거만 사용하여 애니메이션 실행
        stick.SetTrigger(triggerName);

        if (isLeftStick)
        {
            leftStickPosition = drumIndex;
            Debug.Log($"<color=cyan>LEFT STICK</color> hit <color=yellow>{triggerName}</color> (position: {drumIndex})");
        }
        else
        {
            rightStickPosition = drumIndex;
            Debug.Log($"<color=green>RIGHT STICK</color> hit <color=yellow>{triggerName}</color> (position: {drumIndex})");
        }
    }

    string GetDrumName(int drumIndex)
    {
        switch (drumIndex)
        {
            case 0: return "f";
            case 1: return "g";
            case 2: return "k";
            case 3: return "l";
            default: return "f";
        }
    }
}
