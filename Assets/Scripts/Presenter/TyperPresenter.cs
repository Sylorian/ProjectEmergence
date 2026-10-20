using System;
using System.Collections.Generic;
using Hyperplasia.Core.Data;
using Hyperplasia.Core.Flow;
using Hyperplasia.View;
using UnityEngine;
using TMPro;
using Hyperplasia.Utils;

namespace Hyperplasia.Presenter
{
    [Serializable]
    public class TyperPresenter
    {
        public DraftLineView DraftLine;
        public TextMeshProUGUI HistoryText;
        public RectTransform CandidatesRoot;
        public WordCandidateView CandidateTemplate;
        public ConfirmedWordView ConfirmedTemplate;
        public RectTransform ConfirmedRoot;
        public GameObject InputRoot;

        public TyperEngine Engine { get; set; }

        private readonly List<WordCandidateView> _candidateChips = new List<WordCandidateView>();
        private readonly List<ConfirmedWordView> _confirmedChips = new List<ConfirmedWordView>();

        public TyperPresenter()
        {
        }

        public void UpdateInputVisibility(InputState inputState)
        {
            UIBase.SetActive(InputRoot, inputState != InputState.Explore);
        }

        public void PrepareTyper(InputState inputState)
        {
            PrepareCandidates();
            PrepareConfirmedWords();
            RefreshConfirmedWords();
            UpdateInputVisibility(inputState);
        }

        #region event_handlers
        public void OnFeedback(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            Engine?.AddFeedback(text);
            var history = Engine?.GetHistory() ?? Array.Empty<string>();
            UIBase.SetText(HistoryText, string.Join("\n", history));
        }

        public void OnFirstWord(WordCandidate word)
        {
            Engine.FirstWord = word;
            RefreshConfirmedWords();
        }

        public void OnSecondWord(WordCandidate word)
        {
            Engine.SecondWord = word;
            RefreshConfirmedWords();
        }

        public void OnCandidatesChanged(IReadOnlyList<WordCandidate> candidates)
        {
            RefreshCandidates(candidates);
        }
        #endregion

        private void PrepareConfirmedWords()
        {
            if (ConfirmedTemplate == null) return;
            if (ConfirmedRoot == null) return;

            UIBase.SetActive(ConfirmedTemplate, false);
        }

        private void RefreshConfirmedWords()
        {
            if (ConfirmedTemplate == null) return;

            foreach (var chip in _confirmedChips)
            {
                UIBase.SetActive(chip, false);
                GameObject.Destroy(chip.gameObject);
            }
            _confirmedChips.Clear();

            if (Engine?.FirstWord != null)
            {
                ConfirmedWordView chip = GameObject.Instantiate(ConfirmedTemplate, ConfirmedRoot);
                chip.SetLabel(Engine.FirstWord.Text);
                UIBase.SetActive(chip, true);
                _confirmedChips.Add(chip);
            }

            if (Engine?.SecondWord != null)
            {
                ConfirmedWordView chip = GameObject.Instantiate(ConfirmedTemplate, ConfirmedRoot);
                chip.SetLabel(Engine.SecondWord.Text);
                UIBase.SetActive(chip, true);
                _confirmedChips.Add(chip);
            }
        }

        private void PrepareCandidates()
        {
            if (CandidateTemplate == null) return;
            if (CandidatesRoot == null) return;

            UIBase.SetActive(CandidateTemplate, false);
        }

        private void RefreshCandidates(IReadOnlyList<WordCandidate> candidates)
        {
            if (CandidateTemplate == null) return;

            foreach (var chip in _candidateChips)
            {
                UIBase.SetActive(chip, false);
                GameObject.Destroy(chip.gameObject);
            }
            _candidateChips.Clear();

            var count = 0;
            foreach (var candidate in candidates)
            {
                WordCandidateView chip = GameObject.Instantiate(CandidateTemplate, CandidatesRoot);
                chip.SetLabel(++count + ". " + candidate.Text);
                UIBase.SetActive(chip, true);
                _candidateChips.Add(chip);
            }
        }
    }
}