using System;
using System.Text;
using Hyperplasia.Core.Data;

namespace Hyperplasia.Core.Flow.States
{
    /// <summary>
    /// 状态 3 · 查看物品（InspectItem）。
    /// 直接显示该物品的标签（如「挂钟：电子、信息」）。Esc 回状态 0；Backspace 回状态 1。
    /// </summary>
    public sealed class InspectItemState : IInputState
    {
        private static readonly WordCandidate[] NoCandidates = Array.Empty<WordCandidate>();
        private static readonly string[] NoIds = Array.Empty<string>();

        private readonly InputController _c;

        public InspectItemState(InputController c)
        {
            _c = c;
        }

        public InputState StateId { get { return InputState.InspectItem; } }

        public void Enter()
        {
            _c.Router.TypingEnabled = false;
            _c.Buffer.Clear();
            _c.SetCandidates(NoCandidates);
            _c.SetHighlight(NoIds);
            _c.RaiseInspection(BuildLabel());
        }

        public void OnText(string text)
        {
        }

        public void OnDigit(int digit)
        {
        }

        public void OnCommand(InputCommand command)
        {
            switch (command)
            {
                case InputCommand.Cancel:      // Esc → 回状态 0
                    _c.TransitionTo(InputState.Explore);
                    break;
                case InputCommand.Backspace:   // Backspace → 回状态 1
                    _c.TransitionTo(InputState.FirstWord);
                    break;
            }
        }

        public void Exit()
        {
        }

        private string BuildLabel()
        {
            WordCandidate w = _c.FirstWord;
            Entity e;
            if (w == null || !_c.Database.EntityById.TryGetValue(w.Id, out e)) return null;

            var sb = new StringBuilder();
            sb.Append(e.Name);
            sb.Append('：');
            if (e.Tags == null || e.Tags.Length == 0)
            {
                sb.Append("（无标签）");
            }
            else
            {
                for (int i = 0; i < e.Tags.Length; i++)
                {
                    if (i > 0) sb.Append('、');
                    sb.Append(e.Tags[i]);
                }
            }
            return sb.ToString();
        }
    }
}