using System;

namespace Hyperplasia.Core.Flow.States
{
    /// <summary>
    /// 移动模式（Explore）。
    /// </summary>
    public sealed class ExploreState : IInputState
    {

        private readonly InputController _c;

        public ExploreState(InputController c)
        {
            _c = c;
        }

        public InputState StateId { get { return InputState.Explore; } }

        public void Enter()
        {
            _c.Router.TypingEnabled = false;
            _c.TyperEngine.Buffer.Clear();
            _c.TyperEngine.ClearCandidates();
            _c.TyperEngine.ClearFirstWord();
            _c.TyperEngine.ResetSecondWord();
        }

        public void OnText(string text)
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
                    _c.RequestCharacterMove(-1f);
                    break;
                case InputCommand.MoveRight:
                    _c.RequestCharacterMove(1f);
                    break;
            }
        }

        public void Exit()
        {
        }
    }
}