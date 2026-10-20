using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using Hyperplasia.Utils;

namespace Hyperplasia.View
{
    public class ConfirmedWordView : MonoBehaviour
    {
        public Button Button;
        public TextMeshProUGUI Label;

        public void Subscribe(UnityAction action)
        {
            UIBase.Subscribe(Button, action);
        }
        public void SetLabel(string text)
        {
            UIBase.SetText(Label, text);
        }
    }
}