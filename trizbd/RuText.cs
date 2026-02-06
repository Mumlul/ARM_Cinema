using System;
using System.Text.RegularExpressions;

namespace trizbd;

/// <summary>
/// Небольшие преобразования текста для UI (чтобы не светился английский в интерфейсе,
/// даже если он попал в БД).
/// </summary>
public static class RuText
{
    public static string HallName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "";

        var s = name.Trim();

        // Частые варианты названий залов
        s = Regex.Replace(s, "\\bHall\\b", "Зал", RegexOptions.IgnoreCase);
        s = Regex.Replace(s, "\\bStandard\\b", "Стандарт", RegexOptions.IgnoreCase);
        s = Regex.Replace(s, "\\bVIP\\b", "ВИП", RegexOptions.IgnoreCase);

        return s;
    }
}
