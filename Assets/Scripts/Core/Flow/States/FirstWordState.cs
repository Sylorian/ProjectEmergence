using System;
using System.Collections.Generic;

namespace Hyperplasia.Core.Flow.States
{
    /// <summary>
    /// 状态 1 · 输入第一词（FirstWord）。
    /// 键入拼音 → 候选词；空格选第一个、数字键选第 N 个；选中即按词性跳转：
    /// 动词 → 状态 2（SecondWord）；名词 → 状态 3（InspectItem）；都不是 →「这行不通」。
    /// </summary>
    public sealed class FirstWordState : IInputState
    {
        private static readonly WordCandidate[] NoCandidates = Array.Empty<WordCandidate>();

        private readonly InputController _c;
        private readonly List<WordCandidate> _candidates = new List<WordCandidate>();
        private int _selected;

        public FirstWordState(InputController c)
        {
            _c = c;
        }

        public InputState StateId { get { return InputState.FirstWord; } }

        public void Enter()
        {
            _c.Router.TypingEnabled = true;
            _c.Buffer.Clear();
            _c.ClearFirstWord();   // 清除上一轮遗留的 Confirmed 芯片
            _c.ResetTarget();
            _selected = 0;
            Refresh();
        }

        public void OnText(string text)
        {
            _c.Buffer.Append(text);
            Refresh();
        }

        public void OnDigit(int digit)
        {
            Select(digit - 1);
        }

        public void OnCommand(InputCommand command)
        {
            switch (command)
            {
                case InputCommand.Confirm:   // 空格：选中（默认第一个）并跳转
                    Select(_candidates.Count > 0 ? _selected : 0);
                    break;
                case InputCommand.Submit:    // 回车：迁就「输入后直接回车」的习惯
                    if (_candidates.Count > 0) Select(_selected);
                    else if (_c.Buffer.IsEmpty) _c.TransitionTo(InputState.Explore);
                    else _c.RaiseFeedback("这行不通");
                    break;
                case InputCommand.Cancel:    // Esc → 回状态 0
                    _c.Buffer.Clear();
                    _c.SetCandidates(NoCandidates);
                    _c.ClearFirstWord();
                    _c.ResetTarget();
                    _c.TransitionTo(InputState.Explore);
                    break;
                case InputCommand.Backspace:
                    if (_c.Buffer.Backspace()) Refresh();
                    break;
            }
        }

        public void Exit()
        {
        }

        private void Refresh()
        {
            _candidates.Clear();
            List<WordCandidate> found = _c.Library.GetCandidates(_c.Buffer.Text);
            if (found != null)
                for (int i = 0; i < found.Count; i++) _candidates.Add(found[i]);

            if (_selected >= _candidates.Count) _selected = 0;
            _c.SetCandidates(_candidates);
        }

        private void Select(int index)
        {
            if (_candidates.Count == 0)
            {
                _c.RaiseFeedback("这行不通");
                return;
            }

            if (index < 0 || index >= _candidates.Count) index = 0;
            _selected = index;
            WordCandidate word = _candidates[index];

            if (word.Kind == WordKind.Verb)
            {
                _c.CommitFirstWord(word);
                _c.Buffer.Clear();
                _c.SetCandidates(NoCandidates);
                _c.TransitionTo(InputState.SecondWord);
            }
            else if (word.Kind == WordKind.Noun)
            {
                _c.CommitFirstWord(word);
                _c.Buffer.Clear();
                _c.SetCandidates(NoCandidates);
                _c.TransitionTo(InputState.InspectItem);
            }
            else
            {
                _c.RaiseFeedback("这行不通");
            }
        }
    }
}