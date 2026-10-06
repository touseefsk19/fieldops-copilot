// ---- Tests ----
Console.WriteLine("Quest 1 · Two parts for the budget:");
Console.WriteLine(string.Join(", ", FindTwoParts(new[] { 120, 450, 300, 80 }, 380)));   // 2, 3
Console.WriteLine(string.Join(", ", FindTwoParts(new[] { 200, 150, 250 }, 400)));       // 1, 2

Console.WriteLine("Quest 2 · Scanner-safe tag:");
Console.WriteLine(IsSymmetricTag("CP-200-PC"));    // False
Console.WriteLine(IsSymmetricTag("X7-Y-7x"));      // True

Console.WriteLine("Quest 3 · Vibration alarm:");
Console.WriteLine(MaxVibration(new[] { 2, 1, 5, 1, 3, 2 }, 3));   // 9

// ---- Fill the blanks ----

// A supervisor has exactly `budget` to spend on 2 spare parts. Which two?
static int[] FindTwoParts(int[] prices, int budget)
{
    var seen = new Dictionary<int, int>();   // price → position in the list
    for (int i = 0; i < prices.Length; i++)
    {
        int need = budget - prices[i];                                   // blank A
        if (seen.TryGetValue(need, out int j))             // blank B
        {
            return new[] { j, i };
        }
        seen[prices[i]] = i;                                    // blank C
    }
    return Array.Empty<int>();
}

// A Zebra scanner can read a tag in either direction only if it's symmetric
// (ignoring dashes and upper/lower case).
static bool IsSymmetricTag(string tag)
{
    int left = 0;
    int right = tag.Length - 1;
    while (left < right)
    {
        if (!char.IsLetterOrDigit(tag[left])) { left++; continue; }
        if (!char.IsLetterOrDigit(tag[right])) { right--; continue; }
        if (char.ToLower(tag[left]) != char.ToUpper(tag[right])) return false;  // blank D
        left++;                                                // blank E
        right--;                                                // blank F
    }
    return true;
}

// A pump sends one vibration reading per minute. Alarm on the worst k-minute total.
static int MaxVibration(int[] readings, int k)
{
    int windowSum = 0;
    for (int i = 0; i < k; i++) windowSum += readings[i];
    int best = windowSum;
    for (int right = k; right < readings.Length; right++)
    {
        windowSum += readings[right];                                 // blank G
        windowSum -= readings[right-k];                                 // blank H
        best = Math.Max(best, windowSum);
    }
    return best;
}