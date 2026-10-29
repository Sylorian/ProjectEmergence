using System;
using System.Collections.Generic;
using Hyperplasia.Core.Data;

namespace Hyperplasia.Core.Flow.States
{
    /// <summary>
    /// 输入第一词（FirstWord）。
    /// </summary>
    public sealed class FirstWordState : IInputState, ICandidateSelector
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

            // 在 FirstWord 输入 EXIT 直接退出游戏
            if (IsExitWord(_c.TyperEngine.Buffer.Text))
            {
                _c.RequestExit();
                return;
            }

            Refresh();
        }

        public void OnCommand(InputCommand command)
        {
            switch (command)
            {
                case InputCommand.Confirm:
                    Select(_c.TyperEngine.GetCandidates().Count > 0 ? _selected : 0);
                    break;
                case InputCommand.Submit:
                    if (IsExitWord(_c.TyperEngine.Buffer.Text))
                    {
                        _c.RequestExit();
                        break;
                    }
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

        public bool TrySelectCandidate(WordCandidate word)
        {
            if (word == null) return false;
            int index = _c.TyperEngine.GetCandidates().IndexOf(word);
            if (index < 0) return false;   // 点击的是过期 chip：什么都不做
            Select(index);
            return true;
        }

        private static bool IsExitWord(string text)
        {
            return string.Equals(text?.Trim(), "exit", StringComparison.OrdinalIgnoreCase);
        }

        private void Refresh()
        {
            _c.TyperEngine.MatchAndStoreCandidates(CandidateFilter);
        }

        private bool CandidateFilter(WordCandidate candidate)
        {
            return candidate.Kind == WordKind.Verb || IsReachable(candidate.Id);
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
        private bool IsReachable(int id)
        {
            if (id < 0) return false;
            if (!_c.Session.Database.EntityById.TryGetValue(id, out Entity e)) return false;
            return e.Room == _c.Session.CurrentRoom;
        }
    }
}