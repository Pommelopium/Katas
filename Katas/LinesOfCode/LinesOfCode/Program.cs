
using System;
using System.IO;

namespace LinesOfCode;

class Program
{
    static void Main(string[] args)
    {
        if (File.Exists("../../../../RomanNumerals/Program.cs"))
        {
            string sourceCode = File.ReadAllText("../../../../RomanNumerals/Program.cs");
            Console.WriteLine(LinesOfCode(sourceCode));
        }
    }

    private static (int, int, int) LinesOfCode(string code)
    {
        StringReader reader = new StringReader(code);
        string? currentLine = reader.ReadLine()?.Trim();
        bool isMultiLineComment = false;
        int linesOfComments = 0;
        int linesOfCode = 0;
        int linesOfWhiteSpace = 0;
        while (currentLine != null)
        {
            bool isComment = currentLine.StartsWith("//");
            if (currentLine.StartsWith("/*"))
            {
                isMultiLineComment = true;
            }

            if (string.IsNullOrWhiteSpace(currentLine))
            {
                linesOfWhiteSpace++;
            }
            
            bool isMultiLineCommentEnds = currentLine.EndsWith("*/");
            currentLine = reader.ReadLine()?.Trim();
            if (isComment || isMultiLineComment)
            {
                linesOfComments++;

                if (isMultiLineCommentEnds)
                    isMultiLineComment = false;
                continue;
            }

            linesOfCode++;
        }

        return (linesOfCode, linesOfComments, linesOfWhiteSpace);
    }
}
