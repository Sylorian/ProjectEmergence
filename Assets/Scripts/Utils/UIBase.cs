using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Hyperplasia.Utils
{
    public class UIBase
    {
        public static void SetActive(GameObject go, bool active)
        {
            if (go != null && go.activeSelf != active)
                go.SetActive(active);
        }

        public static void SetActive(Component component, bool active)
        {
            if (component != null)
                SetActive(component.gameObject, active);
        }

        public static void SetText(TextMeshProUGUI textComponent, string text)
        {
            if (textComponent != null)
                textComponent.SetText(text);
        }

        public static void Subscribe(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
                button.onClick.AddListener(action);
        }
    }
}