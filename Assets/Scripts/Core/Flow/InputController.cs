using System;
using System.Collections.Generic;
using Hyperplasia.Core.Data;
using Hyperplasia.Core.Flow.States;

namespace Hyperplasia.Core.Flow
{
    /// <summary>
    /// 输入状态机调度：把采集层事件分发给当前状态，并持有词库 / 世界状态 / 规则引擎。
    /// 初始为 StartState；输入 START 并按 Enter 后进入 Explore。
    /// </summary>
    public sealed class InputController
    {
        public readonly TextInputRouter Router;
        public readonly TyperEngine TyperEngine;
        public readonly GameSession Session;

        private readonly IInputState[] _states;

        private IInputState _current;

        public event Action<InputState, InputState> StateChanged;

        /// <summary>请求退出游戏（由具体运行时层处理，Core 不直接依赖 UnityEditor）。</summary>
        public event Action ExitRequested;

        public InputState State { get { return _current.StateId; } }

        public float MoveAxis { get; private set; }

        public InputController(ITextInputSource source, GameSession session)
        {
            Router = new TextInputRouter(source ?? new NullTextInputSource());
            Router.TextEntered += HandleText;
            Router.CommandPressed += HandleCommand;

            Session = session;
            TyperEngine = Session.TyperEngine;

            _states = new IInputState[Enum.GetValues(typeof(InputState)).Length];
            _states[(int)InputState.Start] = new StartState(this);
            _states[(int)InputState.Explore] = new ExploreState(this);
            _states[(int)InputState.FirstWord] = new FirstWordState(this);
            _states[(int)InputState.SecondWord] = new SecondWordState(this);
            _states[(int)InputState.InspectItem] = new InspectItemState(this);
            _states[(int)InputState.Resolve] = new ResolveState(this);

            _current = _states[(int)GameConfig.START_INPUT_STATE];
            _current.Enter();
        }

        public void Update(float now)
        {
            MoveAxis = 0f;
            Router.Poll(now);
        }

        private void HandleText(string text) { _current.OnText(text); }

        private void HandleCommand(InputCommand cmd) { _current.OnCommand(cmd); }

        public void TransitionTo(InputState target)
        {
            if (_current.StateId == target) return;
            var from = _current.StateId;
            _current.Exit();
            _current = _states[(int)target];
            StateChanged?.Invoke(from, target);
            _current.Enter();
        }

        public bool TrySelectCandidate(WordCandidate word)
        {
            return _current is ICandidateSelector selector && selector.TrySelectCandidate(word);
        }

        #region Requests
        public void RequestExit()
        {
            ExitRequested?.Invoke();
        }

        public void RequestCharacterMove(float axis)
        {
            if (axis > 0f) MoveAxis = 1f;
            else if (axis < 0f) MoveAxis = -1f;
        }

        public void RequestToggleInventory()
        {
            TyperEngine.ToggleInventory();
        }

        public void RequestToggleInk()
        {
            TyperEngine.ToggleInk();
        }

        public void RequestToggleHistory()
        {
            TyperEngine.ToggleHistory();
        }

        #endregion
    }
}