namespace Hyperplasia.Core.Flow.States
{
    public sealed class InspectItemState : IInputState
    {
        private readonly InputController _c;

        public InspectItemState(InputController c)
        {
            _c = c;
        }

        public InputState StateId => InputState.InspectItem;

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
    }
}