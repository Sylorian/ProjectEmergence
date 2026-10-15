using System.Text;
using Hyperplasia.Core.Flow;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hyperplasia.Presentation.Ui
{
    [DisallowMultipleComponent]
    public class DraftLineView : MonoBehaviour
    {
        public TextMeshProUGUI DraftLineText;
        public InputController Controller;
        public string Prefix = "> ";

        private readonly StringBuilder _sb = new StringBuilder();

        private void Update()
        {
            if (DraftLineText == null || Controller == null) return;

            var state = Controller.State;
            var draft = Controller.DraftText;
            _sb.Clear();
            _sb.Append(Prefix);
            _sb.Append(draft);
            DraftLineText.SetText(_sb);
        }
    }
}