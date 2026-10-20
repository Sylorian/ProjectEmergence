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
        private readonly InputController _c;

        public InspectItemState(InputController c)
        {
            _c = c;
        }

        public InputState StateId { get { return InputState.InspectItem; } }

        public void Enter()
        {
            _c.Router.TypingEnabled = false;
            _c.TyperEngine.Buffer.Clear();
            _c.TyperEngine.ClearCandidates();
            _c.TyperEngine.RaiseInspection(BuildLabel());
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
                case InputCommand.Cancel:
                    _c.TransitionTo(InputState.Explore);
                    break;
                case InputCommand.Backspace:
                    _c.TransitionTo(InputState.FirstWord);
                    break;
            }
        }

        public void Exit()
        {
        }

        private string BuildLabel()
        {
            WordCandidate w = _c.TyperEngine.FirstWord;
            if (!_c.Session.Database.EntityById.TryGetValue(w.Id, out Entity e)) return null;

            var sb = new StringBuilder();
            sb.Append(e.Name);
            sb.Append('：');
            if (e.Tags == null || e.Tags.Count == 0)
            {
                sb.Append("（无标签）");
            }
            else
            {
                foreach (string tag in e.Tags)
                {
                    sb.Append("[");
                    sb.Append(tag);
                    sb.Append("]");
                }
            }

            sb.Append('\n');
            sb.Append(e.Clue);
            return sb.ToString();
        }
    }
}