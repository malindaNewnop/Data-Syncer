using System;

namespace syncer.ui
{
    /// Extension methods for string to provide .NET 4.0+ functionality in .NET 3.5
    public static class StringExtensions
    {
        public static bool IsNullOrWhiteSpace(string value)
        {
            if (value == null)
                return true;
            if (value.Length == 0)
                return true;
            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                    return false;
            }
            return true;
        }
    }
}
