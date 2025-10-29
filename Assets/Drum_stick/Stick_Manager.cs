using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stick_Manager : MonoBehaviour
{
    public Animator right_stick;
    public Animator left_Stick;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.S))
        {
            left_Stick.SetTrigger("tick");
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            right_stick.SetTrigger("tick");
        }
    }
}
