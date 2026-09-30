// ---- Tests ----
Console.WriteLine("Binary Search:");
Console.WriteLine(BinarySearch(new[] { -1, 0, 3, 5, 9, 12 }, 9));   // 4
Console.WriteLine(BinarySearch(new[] { -1, 0, 3, 5, 9, 12 }, 2));   // -1

Console.WriteLine("Search Insert Position:");
Console.WriteLine(SearchInsert(new[] { 1, 3, 5, 6 }, 5));   // 2
Console.WriteLine(SearchInsert(new[] { 1, 3, 5, 6 }, 2));   // 1
Console.WriteLine(SearchInsert(new[] { 1, 3, 5, 6 }, 7));   // 4

Console.WriteLine("Search in Rotated Sorted Array:");
Console.WriteLine(SearchRotated(new[] { 4, 5, 6, 7, 0, 1, 2 }, 0));   // 4
Console.WriteLine(SearchRotated(new[] { 4, 5, 6, 7, 0, 1, 2 }, 3));   // -1

// ---- Your code ----
static int BinarySearch(int[] nums, int target)
{
    int left = 0;
    int right = nums.Length - 1;
    while (left <= right)
    {
        int mid = left + (right - left) / 2;
        if (nums[mid] == target)
        {
            return mid;
        }
        if (nums[mid] < target)
        {
            left = mid + 1;
        }
        else
        {
            right = mid - 1;
        }
    }
    return -1;
}

static int SearchInsert(int[] nums, int target)
{
    int left = 0;
    int right = nums.Length - 1;
    while (left <= right)
    {
        int mid = left + (right - left) / 2;
        if (nums[mid] == target)
        {
            return mid;
        }
        if (nums[mid] < target)
        {
            left = mid + 1;
        }
        else
        {
            right = mid - 1;
        }
    }
    return left;
}

static int SearchRotated(int[] nums, int target)
{
    int left = 0;
    int right = nums.Length - 1;
    while (left <= right)
    {
        int mid = left + (right - left) / 2;
        if (nums[mid] == target)
        {
            return mid;
        }
        if (nums[left] <= nums[mid])
        {
            if (nums[left] <= target && target < nums[mid])
            {
                right = mid - 1;
            }
            else
            {
                left = mid + 1;
            }
        }
        else
        {
            if (nums[mid] < target && target <= nums[right])
            {
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }
    }
    return -1;
}