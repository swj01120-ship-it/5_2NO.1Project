using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRNoteHitter : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // ��Ʈ�� �浹�ϸ�
        if (other.CompareTag("Note"))
        {
            Note note = other.GetComponent<Note>();
            if (note != null)
            {
                note.Hit();
            }
        }
    }
}