using System;
using System.Text.RegularExpressions;

namespace Palindrome
{
    public static class Palindrome
    {
        public static bool IsPalindrome(string s)
        {
            s = s.ToLower();
            s = Regex.Replace(s, @"[^a-z0-9]+", "");
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i].Equals(s[s.Length - i - 1]))
                    continue;
                return false;
            }
            return true;
        }
    }
}
