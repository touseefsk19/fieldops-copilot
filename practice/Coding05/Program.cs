// ---- Tests ----
using System.Runtime.InteropServices;

Console.WriteLine("Max Sum of k in a row:");
Console.WriteLine(MaxSumOfK(new[] { 2, 1, 5, 1, 3, 2 }, 3));   // 9  (5+1+3)
Console.WriteLine(MaxSumOfK(new[] { 4, 2, 1, 7 }, 2));         // 8  (1+7)

Console.WriteLine("Contains Nearby Duplicate:");
Console.WriteLine(ContainsNearbyDuplicate(new[] { 1, 2, 3, 1 }, 3));        // True
Console.WriteLine(ContainsNearbyDuplicate(new[] { 1, 2, 3, 1, 2, 3 }, 2));  // False

Console.WriteLine("Longest Substring Without Repeating:");
Console.WriteLine(LengthOfLongestSubstring("abcabcbb"));   // 3 ("abc")
Console.WriteLine(LengthOfLongestSubstring("bbbbb"));      // 1 ("b")
Console.WriteLine(LengthOfLongestSubstring("pwwkew"));     // 3 ("wke")

// ---- Your code ----
static int MaxSumOfK(int[] nums, int k)
{
    int windowSum =0;
    for (int i =0;i<k; i++)
    {
        windowSum += nums[i];
    }
    int best = windowSum;
    for(int right =k; right<nums.Length; right++)
    {
        windowSum += nums[right];
        windowSum -= nums[right -k];
        best = Math.Max(best, windowSum);
    }
    return best;
}

static bool ContainsNearbyDuplicate(int[] nums, int k)
{
    var window = new HashSet<int>();
    for (int i=0; i<nums.Length; i++)
    {
        if (!window.Add(nums[i]))
        {
            return true;
        }
        if(window.Count > k)
        {
            window.Remove(nums[i-k]);
        }
    }
    return false;
}

static int LengthOfLongestSubstring(string s)
{
    var window = new HashSet<char>();
    int left =0;
    int best =0;
    for(int right=0; right<s.Length; right++)
    {
        while (window.Contains(s[right]))
        {
            window.Remove(s[left]);
            left++;
        }
        window.Add(s[right]);
        best = Math.Max(best, right - left + 1);
    }
    return best;
}   