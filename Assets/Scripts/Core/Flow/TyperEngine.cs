using System;
using System.Collections.Generic;
using Hyperplasia.Core.Data;

namespace Hyperplasia.Core.Flow
{
    public class TyperEngine
    {
        public readonly WordLibrary Library;
        public readonly ResolveEngine Resolver;

        public readonly DraftBuffer Buffer = new DraftBuffer();

        private readonly List<WordCandidate> _candidates = new List<WordCandidate>();
        private readonly List<string> _history = new List<string>();

        public WordCandidate FirstWord { get; set; }
        public WordCandidate SecondWord { get; set; }
        public string Inspection { get; set; }
        public string Feedback { get; set; }
        public ResolveResult ResolvedResult { get; set; }

        public event Action<string> FeedbackRaised;  // 当有反馈信息时触发
        public event Action<IReadOnlyList<WordCandidate>> CandidatesChanged;  // 当候选词列表变化时触发
        public event Action<WordCandidate> FirstWordCommitted;  // 当第一词提交时触发
        public event Action<WordCandidate> SecondWordCommitted;  // 当第二词提交时触发
        public event Action<string> InspectionRaised;  // 当检查信息触发时触发
        public event Action<ResolveResult> Resolved;  // 当解析结果触发时触发
        public IReadOnlyList<WordCandidate> Candidates { get { return _candidates; } }

        public event Action OrangeOutline;

        public event Action<bool> InventoryToggled;
        public event Action<bool> InkToggled;
        public event Action<bool> HistoryToggled;

        public TyperEngine(GameDatabase db)
        {
            Library = new WordLibrary(db);
            Resolver = new ResolveEngine(db);
        }

        public void AddFeedback(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                var timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                _history.Add($"[{timeStamp}] {text}");
            }
        }

        public IReadOnlyList<string> GetHistory()
        {
            return _history.AsReadOnly();
        }

        public void RaiseFeedback(string text)
        {
            Feedback = text;
            AddFeedback(text);
            FeedbackRaised?.Invoke(text);
        }

        public void ClearCandidates()
        {
            _candidates.Clear();
            CandidatesChanged?.Invoke(_candidates);
        }

        public void MatchAndStoreCandidates(Func<WordCandidate, bool> filter)
        {
            SetCandidates(Library.GetCandidates(Buffer.Text, filter: filter));
        }

        public List<WordCandidate> GetCandidates()
        {
            return _candidates;
        }

        private void SetCandidates(IReadOnlyList<WordCandidate> list)
        {
            _candidates.Clear();
            foreach (var w in list)
            {
                _candidates.Add(w);
            }
            CandidatesChanged?.Invoke(_candidates);
        }

        public void CommitFirstWord(WordCandidate word)
        {
            FirstWord = word;
            Inspection = null;
            FirstWordCommitted?.Invoke(word);
        }

        public void CommitSecondWord(WordCandidate word)
        {
            SecondWord = word;
            SecondWordCommitted?.Invoke(word);
        }

        public void ClearFirstWord()
        {
            FirstWord = null;
            Inspection = null;
            FirstWordCommitted?.Invoke(null);
        }

        public void ResetFirstWord()
        {
            FirstWord = null;
        }

        public void ResetSecondWord()
        {
            SecondWord = null;
            SecondWordCommitted?.Invoke(null);
        }

        public void RaiseInspection(string text)
        {
            Inspection = text;
            AddFeedback(text);
            InspectionRaised?.Invoke(text);
        }

        public void RaiseResolved(ResolveResult result)
        {
            ResolvedResult = result;
            Resolved?.Invoke(result);
        }

        public void SetOutline()
        {
            OrangeOutline?.Invoke();
        }

        public void ToggleInventory(bool open)
        {
            InventoryToggled?.Invoke(open);
        }

        public void ToggleInk(bool open)
        {
            InkToggled?.Invoke(open);
        }

        public void ToggleHistory(bool open)
        {
            HistoryToggled?.Invoke(open);
        }
    }
}