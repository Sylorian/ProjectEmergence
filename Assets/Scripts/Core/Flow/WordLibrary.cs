using System;
using System.Collections.Generic;
using Hyperplasia.Core.Data;

namespace Hyperplasia.Core.Flow
{
    /// <summary>候选词的词性。</summary>
    public enum WordKind
    {
        None,
        Verb,
        Noun
    }

    /// <summary>一个候选词：中文文本 + 拼音 + 词性 + 对应 id（动词 id 或实体 id）。</summary>
    public sealed class WordCandidate
    {
        public readonly string Text;
        public readonly string Pinyin;
        public readonly WordKind Kind;
        public readonly int Id;

        public WordCandidate(string text, string pinyin, WordKind kind, int id = 0)
        {
            Text = text;
            Pinyin = pinyin;
            Kind = kind;
            Id = id;
        }

        public override string ToString() { return Text; }
    }

    /// <summary>
    /// 词库：把 GameDatabase 里的 verb(name + aliases) 与 entity(name) 展开成候选词，
    /// 建立「拼音前缀表」与「中文文本表」，供状态机做候选与词性判定。
    /// 支持可选的 pinyin / alias_pinyin 字段；没写拼音的条目仍可按中文名前缀命中。
    /// </summary>
    public sealed class WordLibrary
    {
        private readonly List<WordCandidate> _all = new List<WordCandidate>();
        private readonly Dictionary<string, List<WordCandidate>> _byPinyin =
            new Dictionary<string, List<WordCandidate>>(StringComparer.Ordinal);
        private readonly Dictionary<string, WordCandidate> _byText =
            new Dictionary<string, WordCandidate>(StringComparer.Ordinal);

        public IReadOnlyList<WordCandidate> All { get { return _all; } }

        public WordLibrary(GameDatabase db)
        {
            if (db == null) return;

            foreach (Verb v in db.Verbs)
            {
                Add(new WordCandidate(v.Name, Norm(v.Pinyin), WordKind.Verb, v.Id));
            }

            foreach (Entity e in db.Entities)
                Add(new WordCandidate(e.Name, Norm(e.Pinyin), WordKind.Noun, e.Id));
        }

        /// <summary>规范化：去空格 + 转小写（拼音只比较小写字母）。</summary>
        public static string Norm(string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            return s.Trim().ToLowerInvariant();
        }

        private void Add(WordCandidate c)
        {
            if (c == null || string.IsNullOrEmpty(c.Text)) return;
            _all.Add(c);

            if (!_byText.ContainsKey(c.Text)) _byText[c.Text] = c;

            if (!string.IsNullOrEmpty(c.Pinyin))
            {
                if (!_byPinyin.TryGetValue(c.Pinyin, out List<WordCandidate> list))
                {
                    list = new List<WordCandidate>();
                    _byPinyin[c.Pinyin] = list;
                }
                list.Add(c);
            }
        }

        /// <summary>按输入串取候选：先按拼音前缀，命中则返回；否则退回中文名前缀。最多 max 个。</summary>
        public List<WordCandidate> GetCandidates(string typed, int max = 100)
        {
            var result = new List<WordCandidate>();
            string q = Norm(typed);
            if (string.IsNullOrEmpty(q) || max <= 0) return result;

            var pinyinHits = new List<WordCandidate>();
            foreach (KeyValuePair<string, List<WordCandidate>> kv in _byPinyin)
            {
                if (kv.Key.StartsWith(q, StringComparison.Ordinal)) pinyinHits.AddRange(kv.Value);
            }
            if (pinyinHits.Count > 0) return Trim(pinyinHits, max);

            var textHits = new List<WordCandidate>();
            foreach (KeyValuePair<string, WordCandidate> kv in _byText)
            {
                if (kv.Key.StartsWith(q, StringComparison.Ordinal)) textHits.Add(kv.Value);
            }
            return Trim(textHits, max);
        }

        private static List<WordCandidate> Trim(List<WordCandidate> hits, int max)
        {
            hits.Sort(Compare);
            if (hits.Count > max) hits.RemoveRange(max, hits.Count - max);
            return hits;
        }

        private static int Compare(WordCandidate a, WordCandidate b)
        {
            int c = string.CompareOrdinal(a.Pinyin ?? "", b.Pinyin ?? "");
            if (c != 0) return c;
            c = string.CompareOrdinal(a.Text, b.Text);
            if (c != 0) return c;
            return ((int)a.Kind).CompareTo((int)b.Kind);
        }

        /// <summary>精确按中文词查候选（用于拿词性）。</summary>
        public WordCandidate FindByText(string text)
        {
            return text != null && _byText.TryGetValue(text, out var c) ? c : null;
        }
    }
}