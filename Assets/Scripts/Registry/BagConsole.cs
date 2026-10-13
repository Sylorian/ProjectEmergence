using UnityEngine;

public class BagConsole : MonoBehaviour

{
    
    
    //在这里提供遍历和调取方法
    void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            HighlightAnimate.HighlightCall("01-01-01-01");
        }
    }
}
