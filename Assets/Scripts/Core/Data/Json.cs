using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Hyperplasia.Core.Data
{
    /// <summary>JSON 解析错误（带位置，便于定位手写数据里的低级错误）。</summary>
    public sealed class JsonException : Exception
    {
        public JsonException(string message) : base(message) { }
    }

    public enum JsonKind
    {
        Null,
        Bool,
        Number,
        String,
        Array,
        Object
    }

    /// <summary>
    /// 极简 JSON 值。Hyperplasia.Core 不引用 UnityEngine 与任何三方库，因此自带解析器，
    /// 既能被 EditMode 单测直接驱动，也能在编辑器工具（校验器）里复用。
    /// </summary>
    public sealed class JsonValue
    {
        private static readonly List<JsonValue> NoItems = new List<JsonValue>();
        private static readonly Dictionary<string, JsonValue> NoMembers = new Dictionary<string, JsonValue>();

        private readonly bool _bool;
        private readonly double _number;
        private readonly string _str;
        private readonly List<JsonValue> _items;
        private readonly Dictionary<string, JsonValue> _members;

        public static readonly JsonValue Null = new JsonValue();

        public JsonKind Kind { get; private set; }

        private JsonValue()
        {
            Kind = JsonKind.Null;
            _items = NoItems;
            _members = NoMembers;
        }

        private JsonValue(bool v)
        {
            Kind = JsonKind.Bool;
            _bool = v;
            _items = NoItems;
            _members = NoMembers;
        }

        private JsonValue(double v)
        {
            Kind = JsonKind.Number;
            _number = v;
            _items = NoItems;
            _members = NoMembers;
        }

        private JsonValue(string v)
        {
            Kind = JsonKind.String;
            _str = v;
            _items = NoItems;
            _members = NoMembers;
        }

        private JsonValue(List<JsonValue> items)
        {
            Kind = JsonKind.Array;
            _items = items;
            _members = NoMembers;
        }

        private JsonValue(Dictionary<string, JsonValue> members)
        {
            Kind = JsonKind.Object;
            _items = NoItems;
            _members = members;
        }

        internal static JsonValue CreateBool(bool v) { return new JsonValue(v); }
        internal static JsonValue CreateNumber(double v) { return new JsonValue(v); }
        internal static JsonValue CreateString(string v) { return new JsonValue(v); }
        internal static JsonValue CreateArray(List<JsonValue> items) { return new JsonValue(items); }
        internal static JsonValue CreateObject(Dictionary<string, JsonValue> members) { return new JsonValue(members); }

        public bool IsNull { get { return Kind == JsonKind.Null; } }
        public bool IsArray { get { return Kind == JsonKind.Array; } }
        public bool IsObject { get { return Kind == JsonKind.Object; } }
        public bool IsString { get { return Kind == JsonKind.String; } }
        public bool IsNumber { get { return Kind == JsonKind.Number; } }

        /// <summary>数组元素数 / 对象成员数；其它类型为 0。</summary>
        public int Count
        {
            get
            {
                if (Kind == JsonKind.Array) return _items.Count;
                if (Kind == JsonKind.Object) return _members.Count;
                return 0;
            }
        }

        public IReadOnlyList<JsonValue> Items { get { return _items; } }
        public IEnumerable<KeyValuePair<string, JsonValue>> Members { get { return _members; } }

        public bool Has(string key)
        {
            return key != null && Kind == JsonKind.Object && _members.ContainsKey(key);
        }

        public JsonValue this[string key]
        {
            get
            {
                JsonValue v;
                if (key != null && Kind == JsonKind.Object && _members.TryGetValue(key, out v)) return v;
                return Null;
            }
        }

        public JsonValue this[int index]
        {
            get
            {
                if (Kind == JsonKind.Array && index >= 0 && index < _items.Count) return _items[index];
                return Null;
            }
        }

        public string AsString(string fallback = null)
        {
            if (Kind == JsonKind.String) return _str;
            if (Kind == JsonKind.Number) return _number.ToString(CultureInfo.InvariantCulture);
            if (Kind == JsonKind.Bool) return _bool ? "true" : "false";
            return fallback;
        }

        /// <summary>空字符串与缺失都返回 null（把「字段没写」和「写了空串」归一）。</summary>
        public string AsNonEmptyString()
        {
            if (Kind != JsonKind.String) return null;
            return string.IsNullOrEmpty(_str) ? null : _str;
        }

        public bool AsBool(bool fallback = false)
        {
            if (Kind == JsonKind.Bool) return _bool;
            if (Kind == JsonKind.Number) return Math.Abs(_number) > double.Epsilon;
            if (Kind == JsonKind.String) return string.Equals(_str, "true", StringComparison.OrdinalIgnoreCase);
            return fallback;
        }

        public double AsDouble(double fallback = 0d)
        {
            if (Kind == JsonKind.Number) return _number;
            double parsed;
            if (Kind == JsonKind.String &&
                double.TryParse(_str, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed)) return parsed;
            return fallback;
        }

        public float AsFloat(float fallback = 0f) { return (float)AsDouble(fallback); }

        public int AsInt(int fallback = 0) { return (int)Math.Round(AsDouble(fallback)); }

        public bool? AsBoolOrNull()
        {
            return Kind == JsonKind.Bool ? (bool?)_bool : null;
        }

        public int? AsIntOrNull()
        {
            return Kind == JsonKind.Number ? (int?)(int)Math.Round(_number) : null;
        }

        /// <summary>
        /// 数组 → 字符串数组。非数组返回 null：调用方据此区分「字段缺失（= 无约束）」与「空数组」。
        /// </summary>
        public string[] AsStringArray()
        {
            if (Kind != JsonKind.Array) return null;
            var list = new List<string>(_items.Count);
            for (int i = 0; i < _items.Count; i++)
            {
                string s = _items[i].AsNonEmptyString();
                if (s != null) list.Add(s);
            }
            return list.ToArray();
        }

        public List<JsonValue> AsList()
        {
            return Kind == JsonKind.Array ? _items : new List<JsonValue>();
        }

        public override string ToString()
        {
            return Kind switch
            {
                JsonKind.Null => "null",
                JsonKind.Bool => _bool ? "true" : "false",
                JsonKind.Number => _number.ToString(CultureInfo.InvariantCulture),
                JsonKind.String => _str,
                JsonKind.Array => "[array " + _items.Count + "]",
                _ => "{object " + _members.Count + "}",
            };
        }
    }

    public static class JsonParser
    {
        public static JsonValue Parse(string text)
        {
            string s = text ?? string.Empty;
            int i = 0;
            SkipWhitespace(s, ref i);
            JsonValue value = ParseValue(s, ref i);
            SkipWhitespace(s, ref i);
            if (i < s.Length) throw new JsonException("JSON 解析错误：位置 " + i + " 之后还有多余内容。");
            return value;
        }

        private static void SkipWhitespace(string s, ref int i)
        {
            while (i < s.Length)
            {
                char c = s[i];
                if (c == ' ' || c == '\t' || c == '\r' || c == '\n' || c == '\uFEFF')
                {
                    i++;
                    continue;
                }
                if (c == '/' && i + 1 < s.Length && s[i + 1] == '/')
                {
                    i += 2;
                    while (i < s.Length && s[i] != '\n') i++;
                    continue;
                }
                if (c == '/' && i + 1 < s.Length && s[i + 1] == '*')
                {
                    i += 2;
                    while (i + 1 < s.Length && !(s[i] == '*' && s[i + 1] == '/')) i++;
                    i = Math.Min(i + 2, s.Length);
                    continue;
                }
                break;
            }
        }

        private static JsonValue ParseValue(string s, ref int i)
        {
            if (i >= s.Length) throw new JsonException("JSON 解析错误：内容意外结束。");
            char c = s[i];
            switch (c)
            {
                case '{': return ParseObject(s, ref i);
                case '[': return ParseArray(s, ref i);
                case '"': return JsonValue.CreateString(ParseString(s, ref i));
                case 't': Expect(s, ref i, "true"); return JsonValue.CreateBool(true);
                case 'f': Expect(s, ref i, "false"); return JsonValue.CreateBool(false);
                case 'n': Expect(s, ref i, "null"); return JsonValue.Null;
                default:
                    if (c == '-' || (c >= '0' && c <= '9')) return JsonValue.CreateNumber(ParseNumber(s, ref i));
                    throw new JsonException("JSON 解析错误：位置 " + i + " 出现意外字符 '" + c + "'。");
            }
        }

        private static JsonValue ParseObject(string s, ref int i)
        {
            var map = new Dictionary<string, JsonValue>(StringComparer.Ordinal);
            i++; // '{'
            SkipWhitespace(s, ref i);
            if (i < s.Length && s[i] == '}')
            {
                i++;
                return JsonValue.CreateObject(map);
            }
            while (true)
            {
                SkipWhitespace(s, ref i);
                if (i >= s.Length || s[i] != '"')
                    throw new JsonException("JSON 解析错误：位置 " + i + " 处对象缺少键名。");
                string key = ParseString(s, ref i);
                SkipWhitespace(s, ref i);
                if (i >= s.Length || s[i] != ':')
                    throw new JsonException("JSON 解析错误：位置 " + i + " 处键 '" + key + "' 后缺少 ':'。");
                i++;
                SkipWhitespace(s, ref i);
                map[key] = ParseValue(s, ref i);
                SkipWhitespace(s, ref i);
                if (i >= s.Length) throw new JsonException("JSON 解析错误：对象未闭合。");
                if (s[i] == ',')
                {
                    i++;
                    continue;
                }
                if (s[i] == '}')
                {
                    i++;
                    return JsonValue.CreateObject(map);
                }
                throw new JsonException("JSON 解析错误：位置 " + i + " 处对象内缺少 ',' 或 '}'。");
            }
        }

        private static JsonValue ParseArray(string s, ref int i)
        {
            var list = new List<JsonValue>();
            i++; // '['
            SkipWhitespace(s, ref i);
            if (i < s.Length && s[i] == ']')
            {
                i++;
                return JsonValue.CreateArray(list);
            }
            while (true)
            {
                SkipWhitespace(s, ref i);
                list.Add(ParseValue(s, ref i));
                SkipWhitespace(s, ref i);
                if (i >= s.Length) throw new JsonException("JSON 解析错误：数组未闭合。");
                if (s[i] == ',')
                {
                    i++;
                    continue;
                }
                if (s[i] == ']')
                {
                    i++;
                    return JsonValue.CreateArray(list);
                }
                throw new JsonException("JSON 解析错误：位置 " + i + " 处数组内缺少 ',' 或 ']'。");
            }
        }

        private static string ParseString(string s, ref int i)
        {
            i++; // 开引号
            var sb = new StringBuilder();
            while (true)
            {
                if (i >= s.Length) throw new JsonException("JSON 解析错误：字符串未闭合。");
                char c = s[i++];
                if (c == '"') return sb.ToString();
                if (c != '\\')
                {
                    sb.Append(c);
                    continue;
                }
                if (i >= s.Length) throw new JsonException("JSON 解析错误：转义符后内容结束。");
                char e = s[i++];
                switch (e)
                {
                    case '"': sb.Append('"'); break;
                    case '\\': sb.Append('\\'); break;
                    case '/': sb.Append('/'); break;
                    case 'b': sb.Append('\b'); break;
                    case 'f': sb.Append('\f'); break;
                    case 'n': sb.Append('\n'); break;
                    case 'r': sb.Append('\r'); break;
                    case 't': sb.Append('\t'); break;
                    case 'u':
                        if (i + 4 > s.Length) throw new JsonException("JSON 解析错误：\\u 转义不完整。");
                        string hex = s.Substring(i, 4);
                        int code;
                        if (!int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out code))
                            throw new JsonException("JSON 解析错误：非法 \\u 转义 '" + hex + "'。");
                        sb.Append((char)code);
                        i += 4;
                        break;
                    default:
                        throw new JsonException("JSON 解析错误：未知转义 '\\" + e + "'。");
                }
            }
        }

        private static double ParseNumber(string s, ref int i)
        {
            int start = i;
            if (i < s.Length && (s[i] == '-' || s[i] == '+')) i++;
            while (i < s.Length && ((s[i] >= '0' && s[i] <= '9') || s[i] == '.' || s[i] == 'e' || s[i] == 'E' ||
                                   s[i] == '-' || s[i] == '+')) i++;
            string token = s.Substring(start, i - start);
            double v;
            if (!double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                throw new JsonException("JSON 解析错误：非法数字 '" + token + "'。");
            return v;
        }

        private static void Expect(string s, ref int i, string literal)
        {
            if (i + literal.Length > s.Length || string.CompareOrdinal(s, i, literal, 0, literal.Length) != 0)
                throw new JsonException("JSON 解析错误：位置 " + i + " 处期望 '" + literal + "'。");
            i += literal.Length;
        }
    }
}
