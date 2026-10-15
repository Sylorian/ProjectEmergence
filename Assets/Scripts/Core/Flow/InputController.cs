using System;
using Hyperplasia.Core.Flow.States;

namespace Hyperplasia.Core.Flow
{
    public sealed class InputController
    {
        public readonly TextInputRouter Router;

        public readonly DraftBuffer Buffer = new DraftBuffer();

        private readonly IInputState[] _states;

        private IInputState _current;

        public event Action<InputState, InputState> StateChanged;

        public InputState State { get { return _current.StateId; } }
        public string DraftText { get { return Buffer.Text; } }

        public InputController(ITextInputSource source = null)
        {
            Router = new TextInputRouter(source ?? new NullTextInputSource());
            Router.TextEntered += HandleText;
            Router.CommandPressed += HandleCommand;

            _states = new IInputState[System.Enum.GetValues(typeof(InputState)).Length];
            _states[(int)InputState.Explore] = new ExploreState(this);
            _states[(int)InputState.FirstWord] = new FirstWordState(this);
            _states[(int)InputState.SecondWord] = new SecondWordState(this);
            _states[(int)InputState.InspectItem] = new InspectItemState(this);
            _states[(int)InputState.Resolve] = new ResolveState(this);

            _current = _states[(int)InputState.FirstWord];
        }

        public void Update(float now) => Router.Poll(now);
        private void HandleText(string text) => _current.OnText(text);
        private void HandleCommand(InputCommand cmd) => _current.OnCommand(cmd);

        public void TransitionTo(InputState target)
        {
            if (_current.StateId == target) return;
            var from = _current.StateId;
            _current.Exit();
            _current = _states[(int)target];
            StateChanged?.Invoke(from, target);
            _current.Enter();
        }
    }
}