using System.Text;
using Hyperplasia.Core.Flow;
using TMPro;
using UnityEngine;

namespace Hyperplasia.View
{
    /// <summary>
    /// 输入框草稿行：空输入时显示当前状态提示，输入后显示 `&gt; 草稿 + 光标`。
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
            bool editing = state == InputState.Start
                        || state == InputState.FirstWord
                        || state == InputState.SecondWord;
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
                _sb.Append("检视中（Esc返回，Backspace回退）");
            }
            else if (editing)
            {
                string draft = Controller.TyperEngine.Buffer.Text;
                if (string.IsNullOrEmpty(draft))
                {
                    // 空输入时显示当前状态的输入提示
                    _sb.Append(GetInputHint(state));
                }
                else
                {
                    _sb.Append(Prefix);
                    _sb.Append(draft);
                    if (_cursorOn) _sb.Append(Cursor);
                }
            }

            DraftLineText.SetText(_sb);
        }

        private static string GetInputHint(InputState state)
        {
            switch (state)
            {
                case InputState.Start:
                    return "输入START以开始";
                case InputState.FirstWord:
                    return "输入第一个词（Enter提交，输入EXIT退出）";
                case InputState.SecondWord:
                    return "输入第二个词（Enter提交）";
                default:
                    return string.Empty;
            }
        }

    }
}