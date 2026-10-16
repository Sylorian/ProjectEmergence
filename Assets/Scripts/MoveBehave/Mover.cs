using UnityEngine;

public class Mover : MonoBehaviour
{
    private bool paper_position = false;
    private bool IsOpen = false;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backslash))
        {
            var PaperMover = ScenePropertyRegistry.GetInstance<Transform>("0006");

            paper_position = !paper_position;
            Debug.Log("触发");
            if (paper_position)
            {
                PaperMover.GetComponent<TransforMotor>().MoveTo(new Vector3(0f, -7f, -5f), 70f);
            }
            else
            {
                PaperMover.GetComponent<TransforMotor>().MoveTo(new Vector3(0f, 0f, -5f), 0f);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2))
        {
            var ur = ScenePropertyRegistry.GetInstance<Transform>("0005");
            var ul = ScenePropertyRegistry.GetInstance<Transform>("0004");
            var dr = ScenePropertyRegistry.GetInstance<Transform>("0003");
            var dl = ScenePropertyRegistry.GetInstance<Transform>("0002");
            var core = ScenePropertyRegistry.GetInstance<Transform>("0001");
            IsOpen = !IsOpen;
            if (IsOpen)
            {
                ur.GetComponent<BoxTransforMotor>().MoveTo(new Vector3(16f, 0f, 5f));
                ul.GetComponent<BoxTransforMotor>().MoveTo(new Vector3(-16f, 0f, 5f));
                dr.GetComponent<BoxTransforMotor>().MoveTo(new Vector3(16f, 0f, 5f));
                dl.GetComponent<BoxTransforMotor>().MoveTo(new Vector3(-16f, 0f, 5f));
                core.GetComponent<BoxTransforMotor>().MoveTo(new Vector3(-22f, 0f, 5f));

            }
            else
            {
                ur.GetComponent<BoxTransforMotor>().MoveTo(new Vector3(0f, 0f, 5f));
                ul.GetComponent<BoxTransforMotor>().MoveTo(new Vector3(0f, 0f, 5f));
                dr.GetComponent<BoxTransforMotor>().MoveTo(new Vector3(0f, 0f, 5f));
                dl.GetComponent<BoxTransforMotor>().MoveTo(new Vector3(0f, 0f, 5f));
                core.GetComponent<BoxTransforMotor>().MoveTo(new Vector3(0f, 0f, 5f));
            }
        }
    }
}