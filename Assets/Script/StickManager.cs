using UnityEngine;

public class StickManager : MonoBehaviour
{
    public Animator left_stick;   // 哭率 胶平 Animator
    public Animator right_stick;  // 坷弗率 胶平 Animator

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            left_stick.SetTrigger("Drum_A");
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            left_stick.SetTrigger("Drum_S");
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            right_stick.SetTrigger("Drum_D");
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            right_stick.SetTrigger("Drum_F");
        }
    }
}
