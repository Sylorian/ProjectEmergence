namespace Hyperplasia.Core.Flow.States
{
    /// <summary>
    /// 起始状态。
    /// </summary>
    public sealed class StartState : IInputState
    {

        private readonly InputController _c;

        public StartState(InputController c)
        {
            _c = c;
        }

        public InputState StateId { get { return InputState.Start; } }

        public void Enter()
        {
            _c.Router.TypingEnabled = false;
        }

        public void OnText(string text)
        {
        }

        public void OnCommand(InputCommand command)
        {
            switch (command)
            {
                default:
                    _c.TransitionTo(InputState.Explore);
                    break;
            }
        }

        public void Exit()
        {
        }
    }
}