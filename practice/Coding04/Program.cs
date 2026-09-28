// ---- Tests ----
using System.Text.Json.Serialization;

Console.WriteLine("Valid Palindrome:");
Console.WriteLine(IsPalindrome("Was it a car or a cat I saw?"));
Console.WriteLine(IsPalindrome("A man, a plan, a canal: Panama"));   // True
Console.WriteLine(IsPalindrome("race a car"));                       // False

Console.WriteLine("Reverse String:");
var letters = new[] { 'h', 'e', 'l', 'l', 'o' };
ReverseString(letters);
Console.WriteLine(new string(letters));                              // olleh

Console.WriteLine("Move Zeroes:");
var nums = new[] { 0, 1, 0, 3, 12 };
MoveZeroes(nums);
Console.WriteLine(string.Join(", ", nums));                          // 1, 3, 12, 0, 0

// ---- Your code ----
static bool IsPalindrome(string s)
{
    int left = 0;
    int right = s.Length-1;

    while(left < right)
    {
        if (!char.IsLetterOrDigit(s[left]))
        {
            left++;
            continue;
        }
        if (!char.IsLetterOrDigit(s[right]))
        {
            right--;
            continue;
        }
        if(char.ToLower(s[left]) != char.ToLower(s[right]))
        {
            return false;
        }
        left++;
        right--;
    }
    return true;
}

static void ReverseString(char[] s)
{
    int left = 0;
    int right = s.Length-1;

    while(left < right)
    {
        (s[left], s[right]) = (s[right], s[left]);
        left++;
        right--;
    }
}

static void MoveZeroes(int[] nums)
{
    int write = 0;

    for (int read = 0; read < nums.Length; read++)
    {
        if(nums[read] != 0)
        {
            (nums[write], nums[read]) = (nums[read], nums[write]);
                write++;
        }
    
    }
    
}