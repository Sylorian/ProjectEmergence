using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace Hyperplasia.Presentation.Ui
{
    public class WordCandidateView : MonoBehaviour
    {
        public Button Button;
        public TextMeshProUGUI Label;
        
        public void Subscribe(UnityAction action)
        {
            if (Button != null) Button.onClick.AddListener(action);
        }
        public void SetLabel(string text)
        {
            if (Label != null) Label.SetText(text);
        }
    }
}