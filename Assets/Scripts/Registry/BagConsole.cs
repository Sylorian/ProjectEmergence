using UnityEngine;
public class BagConsole : MonoBehaviour

{

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            HighlightAnimate.HighlightCall("01-01-01-01");
        }
    }
}
