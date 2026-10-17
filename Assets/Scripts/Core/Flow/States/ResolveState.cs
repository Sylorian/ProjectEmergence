namespace Hyperplasia.Core.Flow.States
{
    /// <summary>
    /// 状态 4 · 解析反馈（Resolve）。
    /// 进入即查搭配、执行 effects、抛出 feedback；结束后立即回状态 0，让玩家看到世界变化。
    /// </summary>
    public sealed class ResolveState : IInputState
    {
        private readonly InputController _c;

        public ResolveState(InputController c)
        {
            _c = c;
        }

        public InputState StateId { get { return InputState.Resolve; } }

        public void Enter()
        {
            _c.Router.TypingEnabled = false;
            _c.Buffer.Clear();

            ResolveResult result = _c.Resolver.Resolve(_c.FirstWordId, _c.TargetId, _c.World);
            _c.SetLastResolve(result);
            _c.World.CountWord();
            _c.World.Ink -= _c.Database.Config.InkCostPerWord;
            _c.RaiseFeedback(result.Feedback);

            // 结束 → 回状态 0
            _c.TransitionTo(InputState.Explore);
        }

        public void OnText(string text)
        {
        }

        public void OnDigit(int digit)
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