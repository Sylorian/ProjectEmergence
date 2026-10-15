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
        }

        public void Exit()
        {
        }
    }
}