// Minimal JSON parser for dictionary support (public domain style)
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Superglazka.Services
{
    public static class MiniJSON
    {
        public static class Json
        {
            public static object Deserialize(string json)
            {
                if (string.IsNullOrEmpty(json)) return null;
                return Parser.Parse(json);
            }

            public static string Serialize(object obj)
            {
                return Serializer.Serialize(obj);
            }

            private sealed class Parser : IDisposable
            {
                private readonly string json;
                private int index;

                private Parser(string json) { this.json = json; }
                public static object Parse(string json) { using var p = new Parser(json); return p.ParseValue(); }
                public void Dispose() { }

                private object ParseValue()
                {
                    SkipWhitespace();
                    char c = Peek;
                    return c switch
                    {
                        '"' => ParseString(),
                        '{' => ParseObject(),
                        '[' => ParseArray(),
                        't' or 'f' => ParseBool(),
                        'n' => ParseNull(),
                        _ => ParseNumber(),
                    };
                }

                private Dictionary<string, object> ParseObject()
                {
                    var dict = new Dictionary<string, object>();
                    index++; // {
                    while (true)
                    {
                        SkipWhitespace();
                        if (Peek == '}') { index++; break; }
                        var key = ParseString();
                        SkipWhitespace();
                        if (Peek == ':') index++;
                        dict[key] = ParseValue();
                        SkipWhitespace();
                        if (Peek == ',') index++;
                    }
                    return dict;
                }

                private List<object> ParseArray()
                {
                    var list = new List<object>();
                    index++; // [
                    while (true)
                    {
                        SkipWhitespace();
                        if (Peek == ']') { index++; break; }
                        list.Add(ParseValue());
                        SkipWhitespace();
                        if (Peek == ',') index++;
                    }
                    return list;
                }

                private string ParseString()
                {
                    var sb = new StringBuilder();
                    index++; // "
                    while (true)
                    {
                        char c = json[index++];
                        if (c == '"') break;
                        if (c == '\\')
                        {
                            c = json[index++];
                            sb.Append(c switch
                            {
                                '"' or '\\' or '/' => c,
                                'b' => '\b',
                                'f' => '\f',
                                'n' => '\n',
                                'r' => '\r',
                                't' => '\t',
                                _ => c,
                            });
                        }
                        else sb.Append(c);
                    }
                    return sb.ToString();
                }

                private bool ParseBool()
                {
                    if (json.Substring(index, 4) == "true") { index += 4; return true; }
                    if (json.Substring(index, 5) == "false") { index += 5; return false; }
                    return false;
                }

                private object ParseNull() { index += 4; return null; }

                private object ParseNumber()
                {
                    int start = index;
                    if (Peek == '-') index++;
                    while (char.IsDigit(Peek)) index++;
                    if (Peek == '.')
                    {
                        index++;
                        while (char.IsDigit(Peek)) index++;
                    }
                    if (Peek == 'e' || Peek == 'E')
                    {
                        index++;
                        if (Peek == '-' || Peek == '+') index++;
                        while (char.IsDigit(Peek)) index++;
                    }
                    var numStr = json.Substring(start, index - start);
                    if (numStr.Contains(".") || numStr.Contains("e") || numStr.Contains("E"))
                        return double.Parse(numStr, System.Globalization.CultureInfo.InvariantCulture);
                    return long.Parse(numStr);
                }

                private void SkipWhitespace() { while (index < json.Length && char.IsWhiteSpace(json[index])) index++; }
                private char Peek => index < json.Length ? json[index] : '\0';
            }

            private sealed class Serializer
            {
                private readonly StringBuilder sb = new();
                public static string Serialize(object obj) { var s = new Serializer(); s.SerializeValue(obj); return s.sb.ToString(); }

                private void SerializeValue(object value)
                {
                    switch (value)
                    {
                        case null: sb.Append("null"); break;
                        case string s: SerializeString(s); break;
                        case bool b: sb.Append(b ? "true" : "false"); break;
                        case IList list: SerializeArray(list); break;
                        case IDictionary dict: SerializeObject(dict); break;
                        case char c: SerializeString(c.ToString()); break;
                        case byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal:
                            sb.Append(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)); break;
                        default: SerializeString(value.ToString()); break;
                    }
                }

                private void SerializeObject(IDictionary dict)
                {
                    sb.Append('{');
                    bool first = true;
                    foreach (DictionaryEntry kv in dict)
                    {
                        if (!first) sb.Append(',');
                        first = false;
                        SerializeString(kv.Key.ToString());
                        sb.Append(':');
                        SerializeValue(kv.Value);
                    }
                    sb.Append('}');
                }

                private void SerializeArray(IList list)
                {
                    sb.Append('[');
                    bool first = true;
                    foreach (var item in list)
                    {
                        if (!first) sb.Append(',');
                        first = false;
                        SerializeValue(item);
                    }
                    sb.Append(']');
                }

                private void SerializeString(string str)
                {
                    sb.Append('"');
                    foreach (char c in str)
                    {
                        switch (c)
                        {
                            case '"': sb.Append("\\\""); break;
                            case '\\': sb.Append("\\\\"); break;
                            case '\b': sb.Append("\\b"); break;
                            case '\f': sb.Append("\\f"); break;
                            case '\n': sb.Append("\\n"); break;
                            case '\r': sb.Append("\\r"); break;
                            case '\t': sb.Append("\\t"); break;
                            default: sb.Append(c); break;
                        }
                    }
                    sb.Append('"');
                }
            }
        }
    }
}
