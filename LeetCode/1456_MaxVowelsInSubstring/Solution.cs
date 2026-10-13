
public class Solution
{
    public int MaxVowels(string s, int k)
    {
        int currentVowels = 0;
        int maxVowels = 0;

        for (int i = 0; i < k; i++)
        {
            if (IsVowel(s[i]))
            {
                currentVowels++;
            }
        }

        maxVowels = currentVowels;

        for (int i = k; i < s.Length; i++)
        {
            if (IsVowel(s[i - k]))
            {
                currentVowels--;
            }

            if (IsVowel(s[i]))
            {
                currentVowels++;
            }

            maxVowels = Math.Max(maxVowels, currentVowels);
        }

        return maxVowels;
    }

    private bool IsVowel(char c)
    {
        return c == 'a' ||
               c == 'e' ||
               c == 'i' ||
               c == 'o' ||
               c == 'u';
    }
}
