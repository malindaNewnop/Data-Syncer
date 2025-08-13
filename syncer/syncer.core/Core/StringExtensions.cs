using System;

namespace syncer.core
{
    internal static class StringExtensions
    {
        public static bool IsNullOrWhiteSpace(string value)
        {
            if (value == null) return true;
            
            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                    return false;
            }
            return true;
        }
        
        public static bool IsNullOrEmpty(string value)
        {
            return string.IsNullOrEmpty(value);
        }
    }
}
