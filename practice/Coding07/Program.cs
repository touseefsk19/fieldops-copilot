// ---- Build the test tree ----
//        3
//       / \
//      9   20
//         /  \
//        15   7
var root = new TreeNode(3,
    new TreeNode(9),
    new TreeNode(20, new TreeNode(15), new TreeNode(7)));

// ---- Tests ----
Console.WriteLine("Max Depth:");
Console.WriteLine(MaxDepth(root));                  // 3
Console.WriteLine(MaxDepth(null));                  // 0

Console.WriteLine("Level Order:");
foreach (var level in LevelOrder(root))
{
    Console.WriteLine(string.Join(", ", level));    // 3 | 9, 20 | 15, 7
}

Console.WriteLine("Invert Tree:");
var inverted = InvertTree(root);
foreach (var level in LevelOrder(inverted))
{
    Console.WriteLine(string.Join(", ", level));    // 3 | 20, 9 | 7, 15
}

// ---- Your code ----
static int MaxDepth(TreeNode? node)
{
    if (node == null)
    {
        return 0;
    }
       int leftDepth = MaxDepth(node.Left);
    int rightDepth = MaxDepth(node.Right);
    return 1 + Math.Max(leftDepth, rightDepth);
}

static List<List<int>> LevelOrder(TreeNode? root)
{
        var result = new List<List<int>>();
    if (root == null)
    {
        return result;
    }
        var queue = new Queue<TreeNode>();
    queue.Enqueue(root);
    while (queue.Count > 0)
    {
        int levelSize = queue.Count;
        var level = new List<int>();
        for (int i = 0; i < levelSize; i++)
        {
            var node = queue.Dequeue();
            level.Add(node.Value);
            if (node.Left != null) queue.Enqueue(node.Left);
            if (node.Right != null) queue.Enqueue(node.Right);
        }
        result.Add(level);
    }
    return result;
}

static TreeNode? InvertTree(TreeNode? node)
{
        if (node == null)
    {
        return null;
    }
        (node.Left, node.Right) = (node.Right, node.Left);
    InvertTree(node.Left);
    InvertTree(node.Right);
    return node;
}

// ---- The tree node type ----
class TreeNode
{
    public int Value;
    public TreeNode? Left;
    public TreeNode? Right;

    public TreeNode(int value, TreeNode? left = null, TreeNode? right = null)
    {
        Value = value;
        Left = left;
        Right = right;
    }
}