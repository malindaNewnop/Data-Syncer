using System;

namespace FTPSyncer.core
{
    /// <summary>
    /// String helpers available to core components (kept minimal to avoid UI dependencies).
    /// </summary>
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





