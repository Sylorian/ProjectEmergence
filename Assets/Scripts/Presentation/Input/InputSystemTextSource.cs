using System;
using System.Text;
using Hyperplasia.Core.Flow;
using UnityEngine.InputSystem;

namespace Hyperplasia.Presentation.Input
{
    public sealed class InputSystemTextSource : ITextInputSource, IDisposable
    {
        private readonly StringBuilder _pending = new StringBuilder();
        private Keyboard _keyboard;
        private bool _disposed;

        public bool DeviceReady
        {
            get
            {
                EnsureSubscribed();
                return _keyboard != null;
            }
        }

        public string ConsumeCommittedText()
        {
            EnsureSubscribed();
            if (_pending.Length == 0) return string.Empty;
            string s = _pending.ToString();
            _pending.Clear();
            return s;
        }

        public bool ConsumeKey(InputCommand key)
        {
            EnsureSubscribed();
            var kb = _keyboard;
            if (kb == null) return false;

            return key switch
            {
                InputCommand.Confirm => kb.spaceKey.wasPressedThisFrame,
                InputCommand.Submit => kb.enterKey.wasPressedThisFrame,
                InputCommand.Cancel => kb.escapeKey.wasPressedThisFrame,
                InputCommand.Backspace => kb.backspaceKey.isPressed,
                _ => false,
            };
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Unsubscribe();
            _pending.Clear();
        }

        private void EnsureSubscribed()
        {
            if (_disposed) return;
            var c = Keyboard.current;
            if (ReferenceEquals(_keyboard, c)) return;

            Unsubscribe();
            _keyboard = c;
            if (_keyboard != null)
            {
                _keyboard.onTextInput += OnTextInput;
                _keyboard.SetIMEEnabled(false);
            }
        }

        private void Unsubscribe()
        {
            if (_keyboard == null) return;
            _keyboard.onTextInput -= OnTextInput;
        }

        private void OnTextInput(char c)
        {
            _pending.Append(c);
        }
    }
}