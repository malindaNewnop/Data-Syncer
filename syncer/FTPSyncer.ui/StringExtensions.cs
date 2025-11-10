using System;
using FTPSyncer.core;

namespace FTPSyncer.ui
{
    /// <summary>
    /// UI-specific string helpers and shims to keep legacy calls working.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Extension wrapper to align with UI call sites using extension syntax.
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string value)
            => FTPSyncer.core.StringExtensions.IsNullOrWhiteSpace(value);

        /// <summary>
        /// Truncates a string to a specified maximum length for UI elements (tooltips, tray text, etc.).
        /// </summary>
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, Math.Max(0, maxLength - 3)) + "...";
        }
    }

    /// <summary>
    /// Backward-compatible alias for existing code that referenced UIStringExtensions.
    /// </summary>
    public static class UIStringExtensions
    {
        public static bool IsNullOrWhiteSpace(string value)
            => FTPSyncer.core.StringExtensions.IsNullOrWhiteSpace(value);
    }
}





