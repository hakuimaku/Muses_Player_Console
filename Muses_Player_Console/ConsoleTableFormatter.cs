namespace Muses_Player_Console;

public static class ConsoleTableFormatter
{
    public static string PadRightDisplay(string? text, int totalWidth)
    {
        text ??= string.Empty;
        int width = GetDisplayWidth(text);
        int padding = Math.Max(0, totalWidth - width);
        return text + new string(' ', padding) + ' ';
    }

    public static string PadLeftDisplay(string? text, int totalWidth)
    {
        text ??= string.Empty;
        int width = GetDisplayWidth(text);
        int padding = Math.Max(0, totalWidth - width);
        return new string(' ', padding) + text + ' ';
    }

    private static int GetDisplayWidth(string text)
    {
        int width = 0;
        foreach (char c in text)
            width += IsWideChar(c) ? 2 : 1;
        return width;
    }

    private static bool IsWideChar(char c)
    {
        return c >= '\u1100' &&
               (c <= '\u115F' ||
                c == '\u2329' || c == '\u232A' ||
                (c >= '\u2E80' && c <= '\uA4CF' && c != '\u303F') ||
                (c >= '\uAC00' && c <= '\uD7A3') ||
                (c >= '\uF900' && c <= '\uFAFF') ||
                (c >= '\uFE10' && c <= '\uFE19') ||
                (c >= '\uFE30' && c <= '\uFE6F') ||
                (c >= '\uFF00' && c <= '\uFF60') ||
                (c >= '\uFFE0' && c <= '\uFFE6'));
    }
}