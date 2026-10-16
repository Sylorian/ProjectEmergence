namespace Hyperplasia.Core.Flow.States
{
    public sealed class FirstWordState : IInputState
    {
        private readonly InputController _c;

        public FirstWordState(InputController c)
        {
            _c = c;
        }

        public InputState StateId => InputState.FirstWord;

        public void Enter()
        {
            _c.Router.TypingEnabled = true;
        }

        public void OnText(string text)
        {
            _c.Buffer.Append(text);
        }

        public void OnCommand(InputCommand command)
        {
            switch (command)
            {
                case InputCommand.Confirm:
                    break;
                case InputCommand.Submit:
                    Submit();
                    break;
                case InputCommand.Cancel:
                    _c.Buffer.Clear();
                    _c.TransitionTo(InputState.Explore);
                    break;
                case InputCommand.Backspace:
                    _c.Buffer.Backspace();
                    break;
                default:
                    break;
            }
        }

        public void Exit()
        {
        }

        private void Submit()
        {
            if (_c.Buffer.IsEmpty)
            {
                _c.TransitionTo(InputState.Explore);
            }
        }
    }
}