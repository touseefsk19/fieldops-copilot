// ---- Tests ----
using System.ComponentModel.DataAnnotations;

Console.WriteLine("Contains Duplicate:");
Console.WriteLine(ContainsDuplicate(new[] { 1, 2, 3, 1 }));   // True
Console.WriteLine(ContainsDuplicate(new[] { 1, 2, 3, 4 }));   // False

Console.WriteLine("Valid Anagram:");
Console.WriteLine(IsAnagram("listen", "silent"));   // True
Console.WriteLine(IsAnagram("rat", "car"));         // False

Console.WriteLine("First Unique Character:");
Console.WriteLine(FirstUniqChar("leetcode"));       // 0  ('l')
Console.WriteLine(FirstUniqChar("loveleetcode"));   // 2  ('v')
Console.WriteLine(FirstUniqChar("aabb"));           // -1 (none)

// ---- Your code ----
static bool ContainsDuplicate(int[] nums)
{
    var seen = new HashSet<int>();
    foreach(var n in nums)
    {
        if (!seen.Add(n))
        {
            return true;
        }
    }
    return false;
}

static bool IsAnagram(string s, string t)
{
    if(s.Length != t.Length) return false;
    var counts = new Dictionary<char, int>();
    foreach (var c in s)
    {
        counts[c] = counts.GetValueOrDefault(c) + 1;
    }
    foreach(var c in t)
    {
        counts[c] = counts.GetValueOrDefault(c) - 1;
    }
    foreach(var value in counts.Values)
    {
        if(value != 0) return false;
    }
    return true;
}

static int FirstUniqChar(string s)
{
    var counts = new Dictionary<char, int>();
    foreach(var c in s)
    {
        counts[c] = counts.GetValueOrDefault(c) + 1;
    }
    for(int i =0; i<s.Length; i++)
    {
        if(counts[s[i]] == 1)
        {
            return i;
        }
    }
    return -1;
}