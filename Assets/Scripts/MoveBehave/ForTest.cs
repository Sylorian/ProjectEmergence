using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForTest : MonoBehaviour
{
    private bool paper_position = false;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backslash))
        {
            var mover = Re;
            paper_position = !paper_position;
            Debug.Log("触发");
            if (paper_position)
            {
                mover.GetComponent<TransforMotor>().MoveTo(new Vector3(0f, -7f, -5f), 70f);
            }
            else
            {
                mover.GetComponent<TransforMotor>().MoveTo(new Vector3(0f, 0f, -5f), 0f);
            }
        }
    }
}