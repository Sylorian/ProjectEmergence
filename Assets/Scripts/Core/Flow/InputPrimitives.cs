using System;
using System.Text;

namespace Hyperplasia.Core.Flow
{
    /// <summary>
    /// 状态机
    /// </summary>
    public enum InputState
    {
        Explore,
        FirstWord,
        SecondWord,
        InspectItem,
        Resolve
    }

    /// <summary>
    /// 采集层产出的字符以外的命令
    /// </summary>
    public enum InputCommand
    {
        Confirm,
        Submit,
        Cancel,
        Backspace,

    }

    public static class TextNormalizer
    {
        public static char Normalize(char c)
        {
            if (c == '\u3000') return ' '; // 全角空格转半角空格
            if (c >= '\uFF01' && c <= '\uFF5E') return (char)(c - 0xFEE0); // 全角字符转半角字符
            return c;
        }

        /// <summary>
        /// 将输入字符串中的全角字符转换为半角字符，并移除控制字符和空格。
        /// </summary>
        /// <param name="s">要规范化的输入字符串</param>
        /// <returns>规范化后的字符串</returns>
        public static string NormalizeInput(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            var sb = new StringBuilder(s.Length);
            foreach (var c in s)
            {
                char nc = Normalize(c);
                if (char.IsControl(nc) || char.IsWhiteSpace(nc)) continue;
                sb.Append(nc);
            }
            return sb.ToString();
        }
    }

    public sealed class DraftBuffer
    {
        private StringBuilder _text = new StringBuilder();
        private Func<char, bool> _filter;

        public event Action Changed;

        public string Text { get { return _text.ToString(); } }
        public int Length { get { return _text.Length; } }
        public bool IsEmpty { get { return _text.Length == 0; } }
        public bool HasFilter { get { return _filter != null; } }
        
        public void SetFilter(Func<char, bool> filter)
        {
            _filter = filter;
        }

        public bool Append(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            var pending = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (char.IsControl(c)) continue;
                if (_filter != null && !_filter(c)) return false;
                pending.Append(c);
            }
            if (pending.Length == 0) return false;
            _text.Append(pending);
            Raise();
            return true;
        }

        public bool Backspace()
        {
            if (_text.Length == 0) return false;
            _text.Length--;
            Raise();
            return true;
        }

        public void Set(string s)
        {
            _text.Clear();
            if (!string.IsNullOrEmpty(s))
            {
                Append(s);
            }
            Raise();
        }

        public void Clear()
        {
            if (_text.Length == 0) return;
            _text.Clear();
            Raise();
        }

        private void Raise()
        {
            Changed?.Invoke();
        }
    }

    public interface IInputState
    {
        InputState StateId { get; }
        void Enter();
        void OnText(string txt);
        void OnCommand(InputCommand cmd);
        void Exit();
    }

    public sealed class NullTextInputSource : ITextInputSource
    {
        public bool DeviceReady => false;
        public string ConsumeCommittedText() { return string.Empty; }
        public bool ConsumeKey(InputCommand key) { return false; }
    }
}