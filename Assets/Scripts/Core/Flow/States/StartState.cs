using System;

namespace Hyperplasia.Core.Flow.States
{
    /// <summary>
    /// 起始状态：输入 START 并按 Enter 后进入 Explore。
    /// </summary>
    public sealed class StartState : IInputState
    {
        private const string StartWord = "start";
        private const string StartHint = "输入START以开始";

        private readonly InputController _c;

        public StartState(InputController c)
        {
            _c = c;
        }

        public InputState StateId { get { return InputState.Start; } }

        public void Enter()
        {
            _c.Router.TypingEnabled = true;
            _c.TyperEngine.Buffer.Clear();
            _c.TyperEngine.ClearCandidates();
            _c.TyperEngine.ClearFirstWord();
            _c.TyperEngine.ResetSecondWord();
        }

        public void OnText(string text)
        {
            _c.TyperEngine.Buffer.Append(text);
        }

        public void OnCommand(InputCommand command)
        {
            switch (command)
            {
                case InputCommand.Submit:
                    if (IsStartWord(_c.TyperEngine.Buffer.Text))
                    {
                        _c.TransitionTo(InputState.Explore);
                    }
                    else
                    {
                        // 输入不匹配：清空输入，让提示重新显示
                        _c.TyperEngine.Buffer.Clear();
                        _c.TyperEngine.RaiseFeedback(StartHint);
                    }
                    break;
                case InputCommand.Backspace:
                    _c.TyperEngine.Buffer.Backspace();
                    break;
                case InputCommand.Cancel:
                    _c.TyperEngine.Buffer.Clear();
                    break;
            }
        }

        public void Exit()
        {
        }

        private static bool IsStartWord(string text)
        {
            return string.Equals(text?.Trim(), StartWord, StringComparison.OrdinalIgnoreCase);
        }

    }
}