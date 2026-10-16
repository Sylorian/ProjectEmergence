using System;
using System.Collections.Generic;

namespace Hyperplasia.Core.Flow
{
    public interface ITextInputSource
    {
        bool DeviceReady { get; }
        string ConsumeCommittedText();
        bool ConsumeKey(InputCommand key);
    }

    public sealed class TextInputRouter
    {
        private readonly ITextInputSource _source;

        public bool TypingEnabled = true;

        public float InputCooldown;

        private float _lastInputTime;

        public TextInputRouter(ITextInputSource source, float inputCooldown = 0.1f)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            InputCooldown = inputCooldown;
        }

        public event Action<string> TextEntered;
        public event Action<InputCommand> CommandPressed;

        public ITextInputSource Source => _source;
        public bool LastFrameDeliveredText { get; private set; }

        public void Poll(float now)
        {
            string text = TextNormalizer.NormalizeInput(_source.ConsumeCommittedText() ?? string.Empty);

            bool deliveredText = TypingEnabled && !string.IsNullOrEmpty(text);
            if (deliveredText)
            {
                _lastInputTime = now;
                TextEntered?.Invoke(text);
            }
            
            // 时钟倒退时清除冷却，防止冷却时间异常
            if (now < _lastInputTime) _lastInputTime = float.NegativeInfinity;

            bool cooldown = now - _lastInputTime < InputCooldown;

            foreach (InputCommand cmd in Enum.GetValues(typeof(InputCommand)))
            {
                // 移动指令不用冷却
                cooldown = cmd != InputCommand.MoveLeft && cmd != InputCommand.MoveRight && cooldown;
                bool commandAllowed = !deliveredText && !cooldown;
                if (commandAllowed && _source.ConsumeKey(cmd))
                {
                    _lastInputTime = now;
                    CommandPressed?.Invoke(cmd);
                }
            }

            LastFrameDeliveredText = deliveredText;
        }
    }
}