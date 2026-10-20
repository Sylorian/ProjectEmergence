
using System;
using System.Collections.Generic;
using Hyperplasia.Core.Data;
using Hyperplasia.Core.Diagnostics;
using Hyperplasia.Core.Flow;
using Hyperplasia.Input;
using Hyperplasia.View;
using Hyperplasia.World;
using Hyperplasia.Bridge;
using Hyperplasia.Presenter;
using Hyperplasia.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hyperplasia.Runtime
{
    /// <summary>
    /// 组装层
    /// </summary>
    [DisallowMultipleComponent]
    public partial class GameBootstrap : MonoBehaviour
    {
        public static GameBootstrap Instance { get; private set; }

        [Header("数据源")]
        [Tooltip("处理后的数据SO资产")]
        public GameData GameData;

        [Header("打字机界面")]
        [SerializeField] private TyperPresenter _typerPresenter;

        [Header("世界表现")]
        [SerializeField] private WorldPresenter _worldPresenter;

        private GameSession _session;
        private InputController _controller;
        private ITextInputSource _inputSource;
        private UnityCoreLogSink _coreLogSink;


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _coreLogSink = new UnityCoreLogSink();
            CoreLog.Enabled = Application.isEditor || Debug.isDebugBuild;
            CoreLog.Sink = _coreLogSink.Write;

            Build();
        }

        private void Update()
        {
            if (_controller == null) return;
            _controller.Update(Time.unscaledTime);
            _typerPresenter.UpdateInputVisibility(_controller.State);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                CoreLog.Sink = null;
                CoreLog.Enabled = false;
            }

            UnhookTyperEvents();
            TeardownInput();
        }

        private void Build()
        {
            EnsureInput();

            _session = new GameSession(LoadDatabase());
            _controller = new InputController(_inputSource, _session);

            _typerPresenter.Prepare(_session.TyperEngine, _controller, InputState.Explore);

            if (_typerPresenter.Engine == null || _typerPresenter.DraftLine == null)
                Debug.LogError("[GameBootstrap] Typer/World 引用不完整，请检查 Inspector 绑定。");

            if (_typerPresenter.DraftLine != null) _typerPresenter.DraftLine.Controller = _controller;
            if (_worldPresenter.CharacterMover != null) _worldPresenter.CharacterMover.SetController(_controller);

            HookTyperEvents();
        }

        private void EnsureInput()
        {
            if (_inputSource != null) return;
            _inputSource = TextInputSources.Create();
        }

        private void TeardownInput()
        {
            var disposable = _inputSource as IDisposable;
            disposable?.Dispose();
            _inputSource = null;
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
            Session.TyperEngine.FeedbackRaised += _typerPresenter.OnFeedback;
            Session.TyperEngine.FirstWordCommitted += _typerPresenter.OnFirstWord;
            Session.TyperEngine.SecondWordCommitted += _typerPresenter.OnSecondWord;
            Session.TyperEngine.CandidatesChanged += _typerPresenter.OnCandidatesChanged;
            Session.TyperEngine.InspectionRaised += _typerPresenter.OnInspection;
        }

        private void UnhookTyperEvents()
        {
            if (Session == null || _typerPresenter == null) return;
            Session.TyperEngine.FeedbackRaised -= _typerPresenter.OnFeedback;
            Session.TyperEngine.FirstWordCommitted -= _typerPresenter.OnFirstWord;
            Session.TyperEngine.SecondWordCommitted -= _typerPresenter.OnSecondWord;
            Session.TyperEngine.CandidatesChanged -= _typerPresenter.OnCandidatesChanged;
            Session.TyperEngine.InspectionRaised -= _typerPresenter.OnInspection;
        }
    }
}