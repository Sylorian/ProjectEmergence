using System;

namespace Hyperplasia.Core.Flow.States
{
    /// <summary>
    /// 状态 0 · 移动模式（Explore）。
    /// 左右箭头移动；按 Enter 进入状态 1。
    /// </summary>
    public sealed class ExploreState : IInputState
    {
        private static readonly WordCandidate[] NoCandidates = Array.Empty<WordCandidate>();
        private static readonly int[] NoIds = Array.Empty<int>();

        private readonly InputController _c;

        public ExploreState(InputController c)
        {
            _c = c;
        }

        public InputState StateId { get { return InputState.Explore; } }

        public void Enter()
        {
            _c.Router.TypingEnabled = false;
            _c.Buffer.Clear();
            _c.SetCandidates(NoCandidates);
            _c.SetHighlight(NoIds);
            _c.ClearFirstWord();
            _c.ResetTarget();
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
                case InputCommand.Submit:          // Enter → 显示输入框
                    _c.TransitionTo(InputState.FirstWord);
                    break;
                case InputCommand.MoveLeft:
                    _c.RequestMove(-1f);
                    break;
                case InputCommand.MoveRight:
                    _c.RequestMove(1f);
                    break;
            }
        }

        public void Exit()
        {
        }
    }
}