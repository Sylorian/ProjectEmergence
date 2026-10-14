using UnityEngine;
using TMPro;

public class EntyrManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    void OnEnable()
    {
        if (inputField != null)
            inputField.onSubmit.AddListener(OnSubmit);
    }

    void OnDisable()
    {
        if (inputField != null)
            inputField.onSubmit.RemoveListener(OnSubmit);
    }

    private void OnSubmit(string value)
    {
        // 按回车后抓取到的内容
        Debug.Log("回车提交内容：" + value);

        // 也可以直接读：string content = inputField.text;
        string content = value;
        
        inputField.text = "";
        inputField.ActivateInputField();
        string ObjectID = ScenePropertyRegistry.FindIdByName(content);
        HighlightAnimate.HighlightCall(ObjectID);
    }
}