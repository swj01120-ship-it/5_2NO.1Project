using UnityEngine;
using System.Collections.Generic;

public class StickManager : MonoBehaviour
{
    public Animator left_stick;   // 왼쪽 스틱 Animator
    public Animator right_stick;  // 오른쪽 스틱 Animator

    [Header("Input Settings")]
    [Tooltip("동시 입력으로 인정할 시간 간격 (초)")]
    public float inputBufferTime = 0.05f;  // 50ms

    // 각 스틱의 현재 위치 (0=F, 1=G, 2=K, 3=L)
    private int leftStickPosition = 0;   // 왼손은 F에서 시작
    private int rightStickPosition = 3;  // 오른손은 G에서 시작

    // 입력 버퍼
    private Dictionary<int, float> inputBuffer = new Dictionary<int, float>();
    private float lastProcessTime = -1f;

    void Update()
    {
        float currentTime = Time.time;

        // 현재 프레임에서 새로 입력된 키 확인
        if (Input.GetKeyDown(KeyCode.A)) inputBuffer[0] = currentTime;
        if (Input.GetKeyDown(KeyCode.S)) inputBuffer[1] = currentTime;
        if (Input.GetKeyDown(KeyCode.D)) inputBuffer[2] = currentTime;
        if (Input.GetKeyDown(KeyCode.F)) inputBuffer[3] = currentTime;

        // 버퍼에 있는 입력 중 유효한 것들 수집
        List<int> validInputs = new List<int>();
        List<int> toRemove = new List<int>();

        foreach (var kvp in inputBuffer)
        {
            float timeSinceInput = currentTime - kvp.Value;

            if (timeSinceInput <= inputBufferTime)
            {
                // 아직 유효한 입력
                validInputs.Add(kvp.Key);
            }
            else
            {
                // 시간이 지난 입력은 제거 대상
                toRemove.Add(kvp.Key);
            }
        }

        // 유효 입력이 있으면 처리
        if (validInputs.Count > 0)
        {
            // 가장 오래된 입력 시간 찾기
            float oldestInputTime = float.MaxValue;
            foreach (int drum in validInputs)
            {
                if (inputBuffer[drum] < oldestInputTime)
                {
                    oldestInputTime = inputBuffer[drum];
                }
            }

            // 가장 오래된 입력 기준으로 시간 경과 확인
            float timeSinceOldest = currentTime - oldestInputTime;

            // 처리 조건:
            // 1. 2개 이상 입력이 모였거나
            // 2. 버퍼 시간이 지났거나
            // 3. 단독 입력이 이미 충분한 시간(버퍼의 절반) 경과
            bool shouldProcess = validInputs.Count >= 2 ||
                               timeSinceOldest >= inputBufferTime ||
                               (validInputs.Count == 1 && timeSinceOldest >= inputBufferTime * 0.5f);

            if (shouldProcess)
            {
                // 처리 시점
                ProcessInputs(validInputs);

                // 처리한 입력들 제거
                inputBuffer.Clear();
                lastProcessTime = currentTime;
            }
        }

        // 오래된 입력 제거
        foreach (int drum in toRemove)
        {
            inputBuffer.Remove(drum);
        }
    }

    // 수집된 입력 처리
    void ProcessInputs(List<int> drums)
    {
        if (drums.Count == 0) return;

        // 정렬
        drums.Sort();

        // 디버그: 입력된 드럼 출력
        string drumsPressed = string.Join(", ", drums);
        Debug.Log($"<color=orange>Processing drums: [{drumsPressed}] (Count: {drums.Count})</color>");

        if (drums.Count == 1)
        {
            // 단일 드럼: 가까운 스틱이 친다
            HitSingleDrum(drums[0]);
        }
        else if (drums.Count >= 2)
        {
            // 2개 이상: 가장 왼쪽과 오른쪽 드럼을 양손으로
            HitDoubleDrum(drums[0], drums[drums.Count - 1]);
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
    void HitDoubleDrum(int leftDrumIndex, int rightDrumIndex)
    {
        Debug.Log($"<color=magenta>==== DOUBLE HIT ====</color>");
        Debug.Log($"<color=magenta>Left drum: {leftDrumIndex} ({GetDrumName(leftDrumIndex)}), Right drum: {rightDrumIndex} ({GetDrumName(rightDrumIndex)})</color>");

        // 왼손 → 왼쪽 드럼
        HitDrumWithStick(left_stick, leftDrumIndex, true);

        // 오른손 → 오른쪽 드럼
        HitDrumWithStick(right_stick, rightDrumIndex, false);

        Debug.Log($"<color=magenta>==================</color>");
    }

    // 스틱으로 드럼 치기
    void HitDrumWithStick(Animator stick, int drumIndex, bool isLeftStick)
    {
        string drumName = GetDrumName(drumIndex);
        string triggerName = "Drum_" + drumName;

        // 애니메이터가 null이 아닌지 확인
        if (stick == null)
        {
            Debug.LogError($"Animator is null! Cannot play {triggerName}");
            return;
        }

        // 트리거 실행
        stick.SetTrigger(triggerName);

        // 스틱 위치 업데이트
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