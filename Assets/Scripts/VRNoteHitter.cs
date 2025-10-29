using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRNoteHitter : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // 노트와 충돌하면
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