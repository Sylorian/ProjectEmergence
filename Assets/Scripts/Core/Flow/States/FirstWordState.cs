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
        private readonly InputController _c;
        private int _selected;

        public FirstWordState(InputController c)
        {
            _c = c;
        }

        public InputState StateId { get { return InputState.FirstWord; } }

        public void Enter()
        {
            _c.Router.TypingEnabled = true;
            _c.TyperEngine.Buffer.Clear();
            _c.TyperEngine.ClearFirstWord();   // 清除上一轮遗留的 Confirmed 芯片
            _c.TyperEngine.ResetSecondWord();
            _selected = 0;
            Refresh();
        }

        public void OnText(string text)
        {
            _c.TyperEngine.Buffer.Append(text);
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
                case InputCommand.Confirm:
                    Select(_c.TyperEngine.GetCandidates().Count > 0 ? _selected : 0);
                    break;
                case InputCommand.Submit:
                    if (_c.TyperEngine.GetCandidates().Count > 0) Select(_selected);
                    else if (_c.TyperEngine.Buffer.IsEmpty) _c.TransitionTo(InputState.Explore);
                    else _c.TyperEngine.RaiseFeedback("这行不通");
                    break;
                case InputCommand.Cancel:
                    _c.TyperEngine.Buffer.Clear();
                    _c.TyperEngine.ClearCandidates();
                    _c.TyperEngine.ClearFirstWord();
                    _c.TyperEngine.ResetSecondWord();
                    _c.TransitionTo(InputState.Explore);
                    break;
                case InputCommand.Backspace:
                    if (_c.TyperEngine.Buffer.Backspace()) Refresh();
                    break;
            }
        }

        public void Exit()
        {
        }

        private void Refresh()
        {
            _c.TyperEngine.MatchAndStoreCandidates();
        }

        private void Select(int index)
        {
            if (_c.TyperEngine.GetCandidates().Count == 0)
            {
                _c.TyperEngine.RaiseFeedback("这行不通");
                return;
            }

            if (index < 0 || index >= _c.TyperEngine.GetCandidates().Count) index = 0;
            _selected = index;
            WordCandidate word = _c.TyperEngine.GetCandidates()[index];

            if (word.Kind == WordKind.Verb)
            {
                _c.TyperEngine.CommitFirstWord(word);
                _c.TyperEngine.Buffer.Clear();
                _c.TyperEngine.ClearCandidates();
                _c.TransitionTo(InputState.SecondWord);
            }
            else if (word.Kind == WordKind.Noun)
            {
                _c.TyperEngine.CommitFirstWord(word);
                _c.TyperEngine.Buffer.Clear();
                _c.TyperEngine.ClearCandidates();
                _c.TransitionTo(InputState.InspectItem);
            }
            else
            {
                _c.TyperEngine.RaiseFeedback("这行不通");
            }
        }
    }
}