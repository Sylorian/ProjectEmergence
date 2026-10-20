
using System;
using System.Collections.Generic;
using Hyperplasia.Core.Data;
using Hyperplasia.Core.Flow;
using Hyperplasia.Input;
using Hyperplasia.View;
using Hyperplasia.World;
using Hyperplasia.Bridge;
using Hyperplasia.Presenter;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hyperplasia.Runtime
{
    /// <summary>
    /// 组装层
    /// </summary>
    [DisallowMultipleComponent]
    public class GameBootstrap : MonoBehaviour
    {
        public static GameBootstrap Instance { get; private set; }

        [Header("数据源")]
        [Tooltip("处理后的数据SO资产")]
        public GameData GameData;

        [Header("打字机界面")]
        public TyperPresenter Typer;

        [Header("世界表现")]
        public WorldPresenter World;

        public GameSession Session { get; private set; }
        public InputController Controller { get; private set; }
        public ITextInputSource Source { get; private set; }


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Build();
        }

        private void Update()
        {
            if (Controller == null) return;
            Controller.Update(Time.unscaledTime);
            Typer.UpdateInputVisibility(Controller.State);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            UnhookTyperEvents();
            TeardownInput();
        }

        private void Build()
        {
            EnsureInput();

            Session = new GameSession(LoadDatabase());
            Controller = new InputController(Source, Session);

            Typer.Engine = Session.Typer;

            if (Typer.Engine == null || Typer.DraftLine == null)
                Debug.LogError("[GameBootstrap] Typer/World 引用不完整，请检查 Inspector 绑定。");

            if (Typer.DraftLine != null) Typer.DraftLine.Controller = Controller;
            if (World.CharacterMover != null) World.CharacterMover.SetController(Controller);

            HookTyperEvents();
            Typer.PrepareTyper(InputState.Explore);
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

        private GameDatabase LoadDatabase()
        {
            var loader = new GameDataLoader();
            if (GameData != null)
                return loader.Load(GameData);

            Debug.LogWarning("[GameBootstrap] 未配置 GameData，使用空数据库。");
            return new GameDatabase();
        }

        private void HookTyperEvents()
        {
            Session.Typer.FeedbackRaised += Typer.OnFeedback;
            Session.Typer.FirstWordCommitted += Typer.OnFirstWord;
            Session.Typer.SecondWordCommitted += Typer.OnSecondWord;
            Session.Typer.CandidatesChanged += Typer.OnCandidatesChanged;
        }

        private void UnhookTyperEvents()
        {
            if (Session == null || Typer == null) return;
            Session.Typer.FeedbackRaised -= Typer.OnFeedback;
            Session.Typer.FirstWordCommitted -= Typer.OnFirstWord;
            Session.Typer.SecondWordCommitted -= Typer.OnSecondWord;
            Session.Typer.CandidatesChanged -= Typer.OnCandidatesChanged;
        }
    }
}