// ---- Tests ----
Console.WriteLine("Two Sum:");
Console.WriteLine(string.Join(",", TwoSum(new[] { 2, 7, 11, 15 }, 9)));   // expected 0,1
Console.WriteLine(string.Join(",", TwoSum(new[] { 3, 3 }, 6)));           // expected 0,1

Console.WriteLine("Valid Parentheses:");
Console.WriteLine(IsValid("()[]{}"));   // True
Console.WriteLine(IsValid("([{}])"));   // True
Console.WriteLine(IsValid("(]"));       // False
Console.WriteLine(IsValid("(("));       // False
Console.WriteLine(IsValid(")"));        // False

Console.WriteLine("Best Time to Buy and Sell Stock:");
Console.WriteLine(MaxProfit(new[] { 7, 1, 5, 3, 6, 4 }));  // 5
Console.WriteLine(MaxProfit(new[] { 7, 6, 4, 3, 1 }));     // 0

// ---- Your code ----
static int[] TwoSum(int[] nums, int target)
{
    var seen = new Dictionary<int, int>();
    for (int i = 0; i < nums.Length; i++)
    {
        int need = target - nums[i];
        if(seen.TryGetValue(need, out int j))
        {
            return new[]{j,i};
        }
        seen[nums[i]] = i;
    }
    return Array.Empty<int>();
}

static bool IsValid(string s)
{
    var stack = new Stack<char>();
    foreach (var c in s)
    {
        if (c == '(' || c == '[' || c == '{')
        {
            stack.Push(c);
        }
        else
        {
            if (stack.Count == 0)
            {
                return false;
            }
            char open = stack.Pop();

            if (c ==')' && open != '(') return false;
            if (c ==']' && open != '[') return false;
            if (c =='}' && open != '{') return false;
        }
    }
    return stack.Count ==0;;
}

static int MaxProfit(int[] prices)
{
    int lowest = int.MaxValue;
    int best = 0;
    foreach(var price in prices)
    {
        lowest = Math.Min(lowest, price);
        best = Math.Max(best, price - lowest);
    }
    return best;
}