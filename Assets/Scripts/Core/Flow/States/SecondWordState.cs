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
        private readonly List<WordCandidate> _candidates = new List<WordCandidate>();
        private int _selected;

        public SecondWordState(InputController c)
        {
            _c = c;
        }

        public InputState StateId { get { return InputState.SecondWord; } }

        public void Enter()
        {
            _c.Router.TypingEnabled = true;
            _c.Buffer.Clear();
            _c.ResetTarget();   // 清掉目标芯片（第一词芯片保留）
            _selected = 0;
            Refresh();
            RefreshHighlight();
        }

        public void OnText(string text)
        {
            _c.Buffer.Append(text);
            _c.ResetTarget();   // 重新输入 → 取消已选目标
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
                case InputCommand.Confirm:   // 空格：选中名词 → 生成芯片
                    Select(_candidates.Count > 0 ? _selected : 0);
                    break;
                case InputCommand.Submit:    // 回车：提交已选目标
                    Submit();
                    break;
                case InputCommand.Cancel:    // Esc → 回状态 0
                    _c.Buffer.Clear();
                    _c.SetCandidates(NoCandidates);
                    _c.SetHighlight(NoIds);
                    _c.ClearFirstWord();
                    _c.ResetTarget();
                    _c.TransitionTo(InputState.Explore);
                    break;
                case InputCommand.Backspace:
                    if (_c.Buffer.Backspace())
                    {
                        _c.ResetTarget();
                        Refresh();
                    }
                    else if (_c.TargetWord != null)
                    {
                        // 先撤销第二词（移除芯片），仍留在 SecondWord
                        _c.ResetTarget();
                        Refresh();
                    }
                    else
                    {
                        // Draft 为空且没有第二词 → 回退到 FirstWord，清除第一词芯片
                        _c.SetCandidates(NoCandidates);
                        _c.SetHighlight(NoIds);
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
            _candidates.Clear();
            List<WordCandidate> found = _c.Library.GetCandidates(_c.Buffer.Text);
            foreach (var candidate in found)
                if (candidate.Kind == WordKind.Noun) _candidates.Add(candidate);

            if (_selected >= _candidates.Count) _selected = 0;
            _c.SetCandidates(_candidates);
        }

        private void RefreshHighlight()
        {
            var ids = new List<int>();
            List<Entity> targets = _c.Resolver.GetCandidateTargets(_c.FirstWordId, _c.World);
            for (int i = 0; i < targets.Count; i++)
                if (IsReachable(targets[i].Id)) ids.Add(targets[i].Id);
            _c.SetHighlight(ids);
        }

        /// <summary>选中名词：生成 Confirmed 芯片，清空草稿与候选，等待回车提交。</summary>
        private void Select(int index)
        {
            if (_candidates.Count == 0) return;
            if (index < 0 || index >= _candidates.Count) index = 0;
            _selected = index;

            _c.CommitTarget(_candidates[index]);
            _c.Buffer.Clear();
            _c.SetCandidates(NoCandidates);
        }

        private void Submit()
        {
            WordCandidate target = _c.TargetWord;
            if (target == null || target.Kind != WordKind.Noun || !IsReachable(target.Id))
            {
                _c.RaiseFeedback("似乎没看见这个东西");
                return;
            }

            _c.SetCandidates(NoCandidates);
            _c.SetHighlight(NoIds);
            _c.TransitionTo(InputState.Resolve);
        }

        /// <summary>当前可打：在背包里、已生成、或就在玩家所在房间。</summary>
        private bool IsReachable(int id)
        {
            if (id < 0) return false;
            if (!_c.Database.EntityById.TryGetValue(id, out Entity e)) return false;
            return e.Room == _c.World.CurrentRoom;
        }
    }
}