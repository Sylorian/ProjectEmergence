
using System;
using System.Collections.Generic;
using Hyperplasia.Core.Data;
using Hyperplasia.Core.Flow;
using Hyperplasia.Presentation.Input;
using Hyperplasia.Presentation.Ui;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hyperplasia.Presentation
{
    /// <summary>
    /// 组装层：加载 JSON → GameDatabase / GameState / WordLibrary / ResolveEngine → InputController → 驱动 UI。
    /// 候选词用 CandidateTemplate 克隆；已确认词用 CommittedTemplate 克隆到 ConfirmedRoot（输入框左边）。
    /// </summary>
    [DisallowMultipleComponent]
    public class GameBootstrap : MonoBehaviour
    {
        [Header("数据")]
        [Tooltip("处理后的数据（含拼音），如 game_data_sample_processed.json")]
        public TextAsset GameData;

        [Header("界面")]
        public DraftLineView DraftLine;
        public TextMeshProUGUI HistoryText;
        public RectTransform CandidatesRoot;
        public WordCandidateView CandidateTemplate;
        [Tooltip("已确认词芯片模板（ConfirmedWord）")]
        public ConfirmedWordView CommittedTemplate;
        [Tooltip("已确认词芯片容器（Input/Confirmed）；留空自动取模板父节点")]
        public RectTransform ConfirmedRoot;
        [Tooltip("可选：整个 Input 容器；草稿行按状态显示/隐藏它")]
        public GameObject InputRoot;

        [Header("角色")]
        public CharacterMover characterMover;

        public InputController Controller { get; private set; }
        public ITextInputSource Source { get; private set; }

        /// <summary>
        /// 状态 2 高亮目标变化（实体 id）。现在与 ScenePropertyRegistry / HighlightAnimate
        /// 同属 Hyperplasia.Gameplay，订阅方可以直接驱动高亮。
        /// </summary>
        public event Action<IReadOnlyList<string>> HighlightChanged;

        public IReadOnlyList<string> CurrentHighlight { get { return _highlight; } }

        private readonly List<WordCandidateView> _candidateButtons = new List<WordCandidateView>();
        private readonly List<ConfirmedWordView> _confirmedChips = new List<ConfirmedWordView>();
        private readonly List<string> _history = new List<string>();
        private readonly List<string> _highlight = new List<string>();

        private void Awake()
        {
            Build();
        }

        private void Update()
        {
            if (Controller == null) return;
            Controller.Update(Time.unscaledTime);
            UpdateInputVisibility();
        }

        private void OnDestroy()
        {
            if (Controller != null)
            {
                Controller.FeedbackRaised -= OnFeedback;
                Controller.FirstWordCommitted -= OnFirstWord;
                Controller.CandidatesChanged -= OnCandidates;
                Controller.HighlightChanged -= OnHighlightChanged;
                Controller.TargetCommitted -= OnTarget;
            }
            TeardownInput();
        }

        private void Build()
        {
            EnsureInput();

            GameDatabase db = LoadDatabase();
            var world = new GameState(db);
            var library = new WordLibrary(db);
            var resolver = new ResolveEngine(db);

            Controller = new InputController(Source, db, world, library, resolver);

            if (DraftLine != null) DraftLine.Controller = Controller;
            if (characterMover != null) characterMover.SetController(Controller);

            HookController();
            PrepareCandidates();
            PrepareConfirmedWords();
            RefreshConfirmedWords();
            UpdateInputVisibility();
        }

        /// <summary>状态 0 隐藏输入条；状态 1/2/3 显示（状态 4 会立即回 0）。</summary>
        private void UpdateInputVisibility()
        {
            InputState state = Controller.State;
            bool showInput = state == InputState.FirstWord || state == InputState.SecondWord || state == InputState.InspectItem;
            if (InputRoot != null && InputRoot.activeSelf != showInput) InputRoot.SetActive(showInput);
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
            if (GameData != null && !string.IsNullOrEmpty(GameData.text))
                return loader.Load(GameData.text);

            Debug.LogWarning("[GameBootstrap] 未配置 GameData(TextAsset)，使用空数据库。");
            return new GameDatabase();
        }

        private void HookController()
        {
            Controller.FeedbackRaised += OnFeedback;
            Controller.FirstWordCommitted += OnFirstWord;
            Controller.TargetCommitted += OnTarget;
            Controller.CandidatesChanged += OnCandidates;
            Controller.HighlightChanged += OnHighlightChanged;
        }

        private void OnFeedback(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            _history.Add(text);
            if (HistoryText != null) HistoryText.SetText(string.Join("\n", _history.ToArray()));
        }

        private void OnFirstWord(WordCandidate word)
        {
            RefreshConfirmedWords();
        }

        private void OnTarget(WordCandidate word)
        {
            RefreshConfirmedWords();
        }

        private void OnHighlightChanged(IReadOnlyList<string> ids)
        {
            _highlight.Clear();
            if (ids != null)
                for (int i = 0; i < ids.Count; i++) _highlight.Add(ids[i]);
            HighlightChanged?.Invoke(_highlight);
        }

        private void OnCandidates(IReadOnlyList<WordCandidate> list)
        {
            RefreshCandidates(list);
        }

        private void PrepareCandidates()
        {
            if (CandidateTemplate == null) return;
            if (CandidatesRoot == null) CandidatesRoot = CandidateTemplate.transform.parent as RectTransform;
            CandidateTemplate.gameObject.SetActive(false);
        }

        private void RefreshCandidates(IReadOnlyList<WordCandidate> list)
        {
            if (CandidateTemplate == null) return;
            int count = list == null ? 0 : list.Count;

            while (_candidateButtons.Count < count)
            {
                var c = Instantiate(CandidateTemplate, CandidatesRoot);
                c.gameObject.SetActive(true);
                c.Subscribe(() => Controller.SelectCandidate(_candidateButtons.IndexOf(c)));
                _candidateButtons.Add(c);
            }

            for (int i = 0; i < _candidateButtons.Count; i++)
            {
                bool active = i < count;
                WordCandidateView c = _candidateButtons[i];
                if (c.gameObject.activeSelf != active) c.gameObject.SetActive(active);
                if (!active) continue;
                c.SetLabel((i + 1) + ". " + list[i].Text);
            }
        }

        /// <summary>ConfirmedWord 只作模板：运行时隐藏，按确认词克隆芯片。</summary>
        private void PrepareConfirmedWords()
        {
            if (CommittedTemplate == null) return;
            if (ConfirmedRoot == null) ConfirmedRoot = CommittedTemplate.transform.parent as RectTransform;
            CommittedTemplate.gameObject.SetActive(false);
        }

        /// <summary>
        /// 按 Controller 里已确认的词刷新芯片，顺序 [第一词][目标词]，
        /// 放在输入框左边的 Confirmed 横向容器里。
        /// </summary>
        private void RefreshConfirmedWords()
        {
            if (CommittedTemplate == null) return;

            int count = 0;
            if (Controller.FirstWord != null) count++;
            if (Controller.TargetWord != null) count++;

            while (_confirmedChips.Count < count)
            {
                ConfirmedWordView chip = Instantiate(CommittedTemplate, ConfirmedRoot);
                chip.gameObject.SetActive(true);
                _confirmedChips.Add(chip);
            }

            int index = 0;
            if (Controller.FirstWord != null)
            {
                ConfirmedWordView chip = _confirmedChips[index++];
                chip.gameObject.SetActive(true);
                chip.SetLabel(Controller.FirstWord.Text);
            }
            if (Controller.TargetWord != null)
            {
                ConfirmedWordView chip = _confirmedChips[index++];
                chip.gameObject.SetActive(true);
                chip.SetLabel(Controller.TargetWord.Text);
            }

            for (int i = index; i < _confirmedChips.Count; i++)
                _confirmedChips[i].gameObject.SetActive(false);
        }
    }
}
