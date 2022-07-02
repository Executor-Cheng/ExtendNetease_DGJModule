using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ExtendNetease_DGJModule.Models
{
    public class LyricInfo
    {
        private static Regex TimeSpanRegex = new Regex(@"(\d+(?::))?(\d+)((?:\.)\d{1,6})?", RegexOptions.Compiled);

        public string Title { get; private set; }

        public string Artist { get; private set; }

        public string Album { get; private set; }

        public string LrcBy { get; private set; }

        public int Offset { get; private set; }

        public IDictionary<double, string> LrcWord { get; }

        public LyricInfo()
        {
            LrcWord = new SortedDictionary<double, string>();
        }

        public LyricInfo(string lyricText) : this()
            => AppendLrc(lyricText);

        public int GetCurrentLyric(double seconds, out string current, out string upcoming)
        {
            if (LrcWord.Count < 1)
            {
                current = "无歌词";
                upcoming = string.Empty;
                return -1;
            }
            List<KeyValuePair<double, string>> list = LrcWord.ToList();
            int i;
            if (seconds < list[0].Key)
            {
                i = 0;
                current = string.Empty;
                upcoming = list[0].Value;
            }
            else
            {
                for (i = 1; i < LrcWord.Count && !(seconds < list[i].Key); i++)
                {
                }
                current = list[i - 1].Value;
                if (list.Count > i)
                {
                    upcoming = list[i].Value;
                }
                else
                {
                    upcoming = string.Empty;
                }
            }
            return i;
        }

        public string GetLyricText()
        {
            StringBuilder lyric = new StringBuilder();
            if (!string.IsNullOrEmpty(Title))
            {
                lyric.AppendLine($"[ti:{Title}]");
            }
            if (!string.IsNullOrEmpty(Artist))
            {
                lyric.AppendLine($"[ar:{Artist}]");
            }
            if (!string.IsNullOrEmpty(Album))
            {
                lyric.AppendLine($"[al:{Album}]");
            }
            if (!string.IsNullOrEmpty(LrcBy))
            {
                lyric.AppendLine($"[by:{LrcBy}]");
            }
            if (Offset != 0)
            {
                lyric.AppendLine($"[offset:{Offset}]");
            }
            lyric.Append(string.Join(Environment.NewLine, LrcWord.GroupBy(p => p.Value).Select(p => $"{string.Join("", p.Select(q => $"[{TimeSpan.FromSeconds(q.Key).ToString(@"mm\:ss\.ff")}]"))}{p.Key}")));
            return lyric.ToString();
        }

        public void AppendLrc(string lrcText)
        {
            string[] array = lrcText.Split(new string[2]
            {
                "\r\n",
                "\n"
            }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string text in array)
            {
                if (text.StartsWith("[ti:"))
                {
                    Title = SplitInfo(text);
                }
                else if (text.StartsWith("[ar:"))
                {
                    Artist = SplitInfo(text);
                }
                else if (text.StartsWith("[al:"))
                {
                    Album = SplitInfo(text);
                }
                else if (text.StartsWith("[by:"))
                {
                    LrcBy = SplitInfo(text);
                }
                else if (text.StartsWith("[offset:"))
                {
                    Offset = int.Parse(SplitInfo(text));
                }
                else
                {
                    try
                    {
                        string value = new Regex(".*\\](.*)").Match(text).Groups[1].Value;
                        if (!(value.Replace(" ", "") == ""))
                        {
                            foreach (Match item in new Regex("\\[([0-9.:]*?)\\]", RegexOptions.Compiled).Matches(text))
                            {
                                Match m = TimeSpanRegex.Match(item.Groups[1].Value);
                                if (m.Success)
                                {
                                    TimeSpan ts = TimeSpan.FromSeconds(int.Parse(m.Groups[2].Value));
                                    if (m.Groups[1].Success)
                                    {
                                        string minute = m.Groups[1].Value;
                                        ts += TimeSpan.FromMinutes(int.Parse(minute.Substring(0, minute.Length - 1)));
                                    }
                                    if (m.Groups[3].Success)
                                    {
                                        string ms = m.Groups[3].Value;
                                        ts += TimeSpan.FromMilliseconds(int.Parse(ms.Substring(1)));
                                    }
                                    double totalSeconds = ts.TotalSeconds;
                                    if (LrcWord.ContainsKey(totalSeconds))
                                    {
                                        LrcWord[totalSeconds] += $"({value})";
                                    }
                                    else
                                    {
                                        LrcWord[totalSeconds] = value;
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }
        
        private static string SplitInfo(string line)
        {
            return line.Substring(line.IndexOf(":") + 1).TrimEnd(']');
        }
    }
}
