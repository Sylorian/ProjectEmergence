using System;
using System.Collections.Generic;
using Hyperplasia.Core.Data;
using Hyperplasia.Core.Flow.States;

namespace Hyperplasia.Core.Flow
{
    /// <summary>
    /// 输入状态机调度：把采集层事件分发给当前状态，并持有词库 / 世界状态 / 规则引擎。
    /// 初始为状态 0（Explore），按 Enter 进入状态 1（FirstWord）。
    /// </summary>
    public sealed class InputController
    {
        public readonly TextInputRouter Router;

        public readonly DraftBuffer Buffer = new DraftBuffer();

        public readonly GameDatabase Database;
        public readonly GameState World;
        public readonly WordLibrary Library;
        public readonly ResolveEngine Resolver;

        private readonly IInputState[] _states;
        private readonly List<WordCandidate> _candidates = new List<WordCandidate>();

        private IInputState _current;

        public event Action<InputState, InputState> StateChanged;
        public event Action<string> FeedbackRaised;
        public event Action<IReadOnlyList<WordCandidate>> CandidatesChanged;
        public event Action<IReadOnlyList<int>> HighlightChanged;
        public event Action<WordCandidate> FirstWordCommitted;
        public event Action<WordCandidate> TargetCommitted;
        public event Action<string> InspectionRaised;

        public InputState State { get { return _current.StateId; } }
        public string DraftText { get { return Buffer.Text; } }
        public IReadOnlyList<WordCandidate> Candidates { get { return _candidates; } }

        /// <summary>当前帧的水平移动意图：-1 向左，1 向右，0 表示本帧没有移动输入。</summary>
        public float MoveAxis { get; private set; }

        public string Feedback { get; private set; }
        public string Inspection { get; private set; }
        public WordCandidate FirstWord { get; private set; }
        public WordCandidate TargetWord { get; private set; }
        public ResolveResult LastResolve { get; private set; }

        public int FirstWordId => FirstWord?.Id ?? -1;
        public int TargetId => TargetWord?.Id ?? -1;

        public InputController(ITextInputSource source = null)
            : this(source, null, null, null, null)
        {
        }

        public InputController(ITextInputSource source, GameDatabase db, GameState world,
            WordLibrary library, ResolveEngine resolver)
        {
            Database = db ?? new GameDatabase();
            World = world ?? new GameState(Database);
            Library = library ?? new WordLibrary(Database);
            Resolver = resolver ?? new ResolveEngine(Database);

            Router = new TextInputRouter(source ?? new NullTextInputSource());
            Router.TextEntered += HandleText;
            Router.CommandPressed += HandleCommand;
            Router.DigitPressed += HandleDigit;

            _states = new IInputState[Enum.GetValues(typeof(InputState)).Length];
            _states[(int)InputState.Explore] = new ExploreState(this);
            _states[(int)InputState.FirstWord] = new FirstWordState(this);
            _states[(int)InputState.SecondWord] = new SecondWordState(this);
            _states[(int)InputState.InspectItem] = new InspectItemState(this);
            _states[(int)InputState.Resolve] = new ResolveState(this);

            _current = _states[(int)InputState.Explore];
            _current.Enter();
        }

        public void Update(float now)
        {
            MoveAxis = 0f;
            Router.Poll(now);
        }

        /// <summary>由状态调用，声明本帧的移动意图（只取方向，不累加速度）。</summary>
        public void RequestMove(float axis)
        {
            if (axis > 0f) MoveAxis = 1f;
            else if (axis < 0f) MoveAxis = -1f;
        }

        private void HandleText(string text) { _current.OnText(text); }
        private void HandleCommand(InputCommand cmd) { _current.OnCommand(cmd); }
        private void HandleDigit(int digit) { _current.OnDigit(digit); }

        public void TransitionTo(InputState target)
        {
            if (_current.StateId == target) return;
            var from = _current.StateId;
            _current.Exit();
            _current = _states[(int)target];
            StateChanged?.Invoke(from, target);
            _current.Enter();
        }

        /// <summary>UI 点击候选按钮时调用；复用数字键通道（index 从 0 开始）。</summary>
        public void SelectCandidate(int index)
        {
            if (index >= 0) _current.OnDigit(index + 1);
        }

        public void RaiseFeedback(string text)
        {
            Feedback = text;
            FeedbackRaised?.Invoke(text);
        }

        public void SetCandidates(IReadOnlyList<WordCandidate> list)
        {
            _candidates.Clear();
            if (list != null)
                for (int i = 0; i < list.Count; i++) _candidates.Add(list[i]);
            CandidatesChanged?.Invoke(_candidates);
        }

        public void SetHighlight(IReadOnlyList<int> ids)
        {
            HighlightChanged?.Invoke(ids ?? Array.Empty<int>());
        }

        public void CommitFirstWord(WordCandidate word)
        {
            FirstWord = word;
            Inspection = null;
            FirstWordCommitted?.Invoke(word);
        }

        public void CommitTarget(WordCandidate word)
        {
            TargetWord = word;
            TargetCommitted?.Invoke(word);
        }

        /// <summary>清除已确认的第一词，并通知表现层移除对应芯片。</summary>
        public void ClearFirstWord()
        {
            FirstWord = null;
            Inspection = null;
            FirstWordCommitted?.Invoke(null);
        }

        public void ResetFirstWord() { FirstWord = null; }

        /// <summary>清除已确认的第二个词，并通知表现层移除对应芯片。</summary>
        public void ResetTarget()
        {
            TargetWord = null;
            TargetCommitted?.Invoke(null);
        }

        public void RaiseInspection(string text)
        {
            Inspection = text;
            InspectionRaised?.Invoke(text);
        }

        public void SetLastResolve(ResolveResult result) { LastResolve = result; }
    }
}