// ---- Tests (don't change) ----
Console.WriteLine("Two Sum:");
Console.WriteLine(string.Join(",", TwoSum(new[] { 2, 7, 11, 15 }, 9)));  // expected 0,1
Console.WriteLine(string.Join(",", TwoSum(new[] { 3, 2, 4 }, 6)));       // expected 1,2

Console.WriteLine("Group Anagrams:");
foreach (var group in GroupAnagrams(new[] { "eat", "tea", "tan", "ate", "nat", "bat" }))
    Console.WriteLine(string.Join(" ", group));                           // expected 3 lines: eat tea ate / tan nat / bat

// ---- Your solutions ----
static int[] TwoSum(int[] nums, int target)
{
    var seen = new Dictionary<int, int>(); // value -> index
    for (int i = 0; i < nums.Length; i++)
    {
        int need = target - nums[i];
        if (seen.TryGetValue(need, out int j))
            return new[] { j, i };
        seen[nums[i]] = i;
    }
    return Array.Empty<int>();
}

static List<List<string>> GroupAnagrams(string[] words)
{
    var groups = new Dictionary<string, List<string>>();
    foreach (var w in words)
    {
        var chars = w.ToCharArray();
        Array.Sort(chars);
        var key = new string(chars);
        if (!groups.TryGetValue(key, out var list))
            groups[key] = list = new List<string>();
        list.Add(w);
    }
    return groups.Values.ToList();
}