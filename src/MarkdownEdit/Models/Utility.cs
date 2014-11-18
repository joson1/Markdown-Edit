﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommonMark;

namespace MarkdownEdit
{
    internal static class Utility
    {
        public static Func<TKey, TResult> Memoize<TKey, TResult>(this Func<TKey, TResult> func)
        {
            var cache = new ConcurrentDictionary<TKey, TResult>();
            return key => cache.GetOrAdd(key, func);
        }

        public static Action<T> Debounce<T>(this Action<T> func, int milliseconds = 300)
        {
            int last = 0;
            return arg =>
            {
                var current = Interlocked.Increment(ref last);
                Task.Delay(milliseconds).ContinueWith(t =>
                {
                    if (current == last) func(arg);
                    t.Dispose();
                });
            };
        }

        public static void Beep()
        {
            SystemSounds.Beep.Play();
        }

        public static void EditFile(string file)
        {
            Process.Start("Notepad.exe", file);
        }

        public static string AssemblyFolder()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase.Substring(8).Replace('/', '\\'));
        }

        public static void ExportHtmlToClipboard(string markdown)
        {
            var text = RemoveYamlFrontMatter(markdown);
            var html = CommonMarkConverter.Convert(text);
            Clipboard.SetText(html);
        }

        public static string RemoveYamlFrontMatter(string markdown)
        {
            if (App.UserSettings.IgnoreYaml == false) return markdown;
            if (markdown.StartsWith("---\n", StringComparison.Ordinal) ||
                markdown.StartsWith("---\r\n", StringComparison.Ordinal))
            {
                var index = Regex.Match(markdown.Substring(3), @"^(---)|(\.\.\.)", RegexOptions.Multiline).Index;
                if (index > 0) return markdown.Substring(index + 6);
            }
            return markdown;
        }
    }
}