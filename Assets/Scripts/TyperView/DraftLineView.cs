using System.Text;
using Hyperplasia.Core.Flow;
using TMPro;
using UnityEngine;

namespace Hyperplasia.View
{
    /// <summary>
    /// 输入框草稿行：`&gt; 草稿 + 光标`。
    /// 状态 0（Explore）隐藏；状态 3（InspectItem）在输入框位置显示物品标签。
    /// </summary>
    [DisallowMultipleComponent]
    public class DraftLineView : MonoBehaviour
    {
        public TextMeshProUGUI DraftLineText;
        public InputController Controller;
        public string Prefix = "> ";
        public string Cursor = "|";

        private readonly StringBuilder _sb = new StringBuilder();
        private float _blinkTimer;
        private bool _cursorOn = true;

        private void Update()
        {
            if (DraftLineText == null || Controller == null) return;

            InputState state = Controller.State;
            bool editing = state == InputState.FirstWord || state == InputState.SecondWord;
            bool inspecting = state == InputState.InspectItem;

            _blinkTimer += Time.unscaledDeltaTime;
            if (_blinkTimer >= 0.5f)
            {
                _blinkTimer = 0f;
                _cursorOn = !_cursorOn;
            }

            _sb.Clear();
            if (inspecting)
            {
                // 输入框位置显示物品标签
                _sb.Append(Controller.Inspection);
            }
            else if (editing)
            {
                _sb.Append(Prefix);
                _sb.Append(Controller.DraftText);
                if (_cursorOn) _sb.Append(Cursor);
            }

            DraftLineText.SetText(_sb);
        }
    }
}