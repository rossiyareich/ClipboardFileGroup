using static ClipboardFileGroup.ClipboardFileGroup;

namespace ClipboardFileGroup.Examples;

internal class Program
{
    private static void Main(string[] args)
    {
        string[] paths = { @"C:\Users\blah\Foo.png", @"C:\Users\blah\Bar.png", @"C:\Users\blah\Baz.png" };

        SetClipboardPaths(paths, true);
        IEnumerable<string> retrievedMove = GetClipboardPaths();
        foreach (string path in retrievedMove)
        {
            Console.WriteLine(path);
        }

        ClearClipboard();

        SetClipboardPaths(paths, false);
        IEnumerable<string> retrievedCopy = GetClipboardPaths();
        foreach (string path in retrievedCopy)
        {
            Console.WriteLine(path);
        }

        ClearClipboard();
    }
}
