using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace trizbd;

public enum AppTheme
{
    Dark,
    Light
}

/// <summary>
/// Очень простой переключатель темы (курсовой проект):
/// меняет ResourceDictionary с Theme.Dark.xaml / Theme.Light.xaml.
/// </summary>
public static class ThemeManager
{
    private const string ThemeFileName = "theme.txt";

    public static AppTheme CurrentTheme { get; private set; } = AppTheme.Dark;

    public static void LoadAndApplySavedTheme()
    {
        var theme = ReadSavedTheme();
        Apply(theme, persist: false);
    }

    public static void Toggle()
    {
        Apply(CurrentTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark, persist: true);
    }

    public static void Apply(AppTheme theme, bool persist)
    {
        try
        {
            var app = Application.Current;
            if (app == null) return;

            var merged = app.Resources.MergedDictionaries;

            // Удаляем старую тему
            var old = merged.FirstOrDefault(d =>
                d.Source != null &&
                (d.Source.OriginalString.Contains("Theme.Dark.xaml", StringComparison.OrdinalIgnoreCase)
                 || d.Source.OriginalString.Contains("Theme.Light.xaml", StringComparison.OrdinalIgnoreCase)));

            if (old != null)
                merged.Remove(old);

            // Добавляем новую
            var src = theme == AppTheme.Dark
                ? new Uri("Resources/Theme.Dark.xaml", UriKind.Relative)
                : new Uri("Resources/Theme.Light.xaml", UriKind.Relative);

            merged.Add(new ResourceDictionary { Source = src });

            CurrentTheme = theme;

            if (persist)
                SaveTheme(theme);
        }
        catch
        {
            // не критично для UI
        }
    }

    private static AppTheme ReadSavedTheme()
    {
        try
        {
            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ThemeFileName);
            if (!File.Exists(file)) return AppTheme.Dark;

            var txt = File.ReadAllText(file).Trim();
            if (Enum.TryParse<AppTheme>(txt, ignoreCase: true, out var t))
                return t;
        }
        catch
        {
            // ignored
        }

        return AppTheme.Dark;
    }

    private static void SaveTheme(AppTheme theme)
    {
        try
        {
            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ThemeFileName);
            File.WriteAllText(file, theme.ToString());
        }
        catch
        {
            // ignored
        }
    }
}
