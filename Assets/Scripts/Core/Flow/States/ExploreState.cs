namespace Hyperplasia.Core.Flow.States
{
    public sealed class ExploreState : IInputState
    {
        private readonly InputController _c;

        public ExploreState(InputController c)
        {
            _c = c;
        }

        public InputState StateId => InputState.Explore;

        public void Enter()
        {
            _c.Router.TypingEnabled = false;
            _c.Buffer.Clear();
        }

        public void OnText(string text)
        {
        }

        public void OnCommand(InputCommand command)
        {
            switch (command)
            {
                case InputCommand.Confirm:
                    break;
                case InputCommand.Submit:
                    _c.TransitionTo(InputState.FirstWord);
                    break;
                case InputCommand.Cancel:
                    break;
                case InputCommand.Backspace:
                    break;
                case InputCommand.MoveLeft:
                    _c.RequestMove(-1f);
                    break;
                case InputCommand.MoveRight:
                    _c.RequestMove(1f);
                    break;
                default:
                    break;
            }
        }

        public void Exit()
        {
        }
    }
}