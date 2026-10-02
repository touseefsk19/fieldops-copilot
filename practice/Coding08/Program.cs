// ---- Tests ----
Console.WriteLine("PriorityQueue basics:");
PrintSmallestFirst(new[] { 5, 1, 4 });                         // 1 4 5

Console.WriteLine("Kth Largest:");
Console.WriteLine(KthLargest(new[] { 3, 2, 1, 5, 6, 4 }, 2));  // 5
Console.WriteLine(KthLargest(new[] { 7, 7, 1 }, 1));           // 7

Console.WriteLine("Top K Frequent:");
Console.WriteLine(string.Join(", ", TopKFrequent(new[] { 1, 1, 1, 2, 2, 3 }, 2)));   // 2, 1

// ---- Your code ----
static void PrintSmallestFirst(int[] nums)
{
    var heap = new PriorityQueue<int, int>();
    foreach (var n in nums)
    {
        heap.Enqueue(n, n);
    }
    while (heap.Count > 0)
    {
        Console.Write(heap.Dequeue() + " ");
    }
    Console.WriteLine();
}

static int KthLargest(int[] nums, int k)
{
        var heap = new PriorityQueue<int, int>();
    foreach (var n in nums)
    {
        heap.Enqueue(n, n);
        if (heap.Count > k)
        {
            heap.Dequeue();
        }
    }
    return heap.Peek();
}

static List<int> TopKFrequent(int[] nums, int k)
{
    var counts = new Dictionary<int, int>();
    foreach (var n in nums)
    {
        counts[n] = counts.GetValueOrDefault(n) + 1;
    }
    var heap = new PriorityQueue<int, int>();
    foreach (var pair in counts)
    {
        heap.Enqueue(pair.Key, pair.Value);
        if (heap.Count > k)
        {
            heap.Dequeue();
        }
    }
    var result = new List<int>();
    while (heap.Count > 0)
    {
        result.Add(heap.Dequeue());
    }
    return result;
}