using System;
using System.Collections.Generic;
using Hyperplasia.Core.Data;

namespace Hyperplasia.Core.Flow.States
{
    /// <summary>
    /// 状态 2 · 输入第二词（SecondWord）。
    /// 进入时高亮可操作目标；键入拼音给候选；空格/数字键选中名词 → 生成 Confirmed 芯片；
    /// 回车提交 → 状态 4（Resolve）。Draft 为空时按 Backspace → 回退状态 1 并清除 Confirmed。
    /// </summary>
    public sealed class SecondWordState : IInputState
    {
        private static readonly WordCandidate[] NoCandidates = Array.Empty<WordCandidate>();
        private static readonly int[] NoIds = Array.Empty<int>();

        private readonly InputController _c;
        private int _selected;

        public SecondWordState(InputController c)
        {
            _c = c;
        }

        public InputState StateId { get { return InputState.SecondWord; } }

        public void Enter()
        {
            _c.Router.TypingEnabled = true;
            _c.TyperEngine.Buffer.Clear();
            _c.TyperEngine.ResetSecondWord();   // 清掉目标芯片（第一词芯片保留）
            _selected = 0;
            Refresh();
        }

        public void OnText(string text)
        {
            _c.TyperEngine.Buffer.Append(text);
            _c.TyperEngine.ResetSecondWord();
            Refresh();
        }

        public void OnCommand(InputCommand command)
        {
            switch (command)
            {
                case InputCommand.Confirm:   // 空格：选中名词 → 生成芯片
                    Select(_c.TyperEngine.Candidates.Count > 0 ? _selected : 0);
                    break;
                case InputCommand.Submit:    // 回车：提交已选目标
                    Submit();
                    break;
                case InputCommand.Cancel:    // Esc → 回状态 0
                    _c.TyperEngine.Buffer.Clear();
                    _c.TyperEngine.ClearCandidates();
                    _c.TyperEngine.ClearFirstWord();
                    _c.TyperEngine.ResetSecondWord();
                    _c.TransitionTo(InputState.Explore);
                    break;
                case InputCommand.Backspace:
                    if (_c.TyperEngine.Buffer.Backspace())
                    {
                        _c.TyperEngine.ResetSecondWord();
                        Refresh();
                    }
                    else if (_c.TyperEngine.SecondWord != null)
                    {
                        // 先撤销第二词（移除芯片），仍留在 SecondWord
                        _c.TyperEngine.ResetSecondWord();
                        Refresh();
                    }
                    else
                    {
                        // Draft 为空且没有第二词 → 回退到 FirstWord，清除第一词芯片
                        _c.TyperEngine.ClearCandidates();
                        _c.TransitionTo(InputState.FirstWord);
                    }
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
            var candidates = _c.TyperEngine.Candidates;
            if (candidates.Count == 0) return;
            if (index < 0 || index >= candidates.Count) index = 0;
            _selected = index;

            _c.TyperEngine.CommitSecondWord(candidates[index]);
            _c.TyperEngine.Buffer.Clear();
            _c.TyperEngine.ClearCandidates();
        }

        private void Submit()
        {
            WordCandidate secondWord = _c.TyperEngine.SecondWord;
            if (secondWord == null || secondWord.Kind != WordKind.Noun || !IsReachable(secondWord.Id))
            {
                _c.TyperEngine.RaiseFeedback("似乎没看见这个东西");
                return;
            }

            _c.TyperEngine.ClearCandidates();
            _c.TransitionTo(InputState.Resolve);
        }

        /// <summary>当前可打：在背包里、已生成、或就在玩家所在房间。</summary>
        private bool IsReachable(int id)
        {
            if (id < 0) return false;
            if (!_c.Session.Database.EntityById.TryGetValue(id, out Entity e)) return false;
            return e.Room == _c.Session.CurrentRoom;
        }
    }
}