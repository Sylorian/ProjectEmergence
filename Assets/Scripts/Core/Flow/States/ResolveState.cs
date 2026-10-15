namespace Hyperplasia.Core.Flow.States
{
    public sealed class ResolveState : IInputState
    {
        private readonly InputController _c;

        public ResolveState(InputController c)
        {
            _c = c;
        }

        public InputState StateId => InputState.Resolve;

        public void Enter()
        {
        }

        public void OnText(string text)
        {
        }

        public void OnCommand(InputCommand command)
        {
        }

        public void Exit()
        {
        }
    }
}