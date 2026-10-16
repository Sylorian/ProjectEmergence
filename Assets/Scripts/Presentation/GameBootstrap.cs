
using System;
using System.Diagnostics;
using Hyperplasia.Core.Flow;
using Hyperplasia.Presentation.Input;
using Hyperplasia.Presentation.Ui;
using UnityEngine;

namespace Hyperplasia.Presentation
{
    [DisallowMultipleComponent]
    public class GameBootstrap : MonoBehaviour
    {
        public InputController Controller { get; private set; }
        public ITextInputSource Source { get; private set; }

        public DraftLineView DraftLine;
        public CharacterMover characterMover;

        private void Awake()
        {
            Build();
        }

        private void Update()
        {
            if (Controller == null) return;
            Controller.Update(Time.unscaledTime);
        }

        private void OnDestroy()
        {
            TeardownInput();
        }

        private void Build()
        {
            EnsureInput();
            Controller = new InputController(Source);

            if (DraftLine != null) DraftLine.Controller = Controller;
            if (characterMover != null) characterMover.SetController(Controller);
        }

        private void EnsureInput()
        {
            if (Source != null) return;
            Source = TextInputSources.Create();
        }

        private void TeardownInput()
        {
            var disposable = Source as IDisposable;
            disposable?.Dispose();
            Source = null;
        }
    }
}