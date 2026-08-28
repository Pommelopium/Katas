
namespace Textumbruch;

/// <summary>
///     Function Kata: Textumbruch
///     Aufgabenbeschreibung: siehe README.md in diesem Projekt.
///     Quelle: https://ccd-school.de/coding-dojo/function-katas/textumbruch/
/// </summary>
public class Program
{
    public static string[] Textumbruch(string input, int lineLength, bool blocksatz)
    {
        List<string> inputSplit = input.Split([' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries)
            .ToList();
        List<string> lines = [];
        string currentLastLine = string.Empty;
        foreach (string s in inputSplit)
        {
            if (lineLength >= s.Length + 1 + currentLastLine.Length)
            {
                if (!string.IsNullOrWhiteSpace(currentLastLine))
                {
                    currentLastLine += ' ';
                }
                currentLastLine += s;
            }
            else if (lineLength < s.Length + 1 + currentLastLine.Length)
            {
                if (!string.IsNullOrWhiteSpace(currentLastLine))
                {
                    lines.Add(currentLastLine);
                }
                if (s.Length >= lineLength)
                {
                    string temp = s;
                    while (temp.Length >= lineLength)
                    {
                        lines.Add(temp.Substring(0, lineLength));
                        temp = temp.Substring(lineLength, temp.Length-lineLength);
                    }
                    currentLastLine = temp;
                }
                else
                {
                    currentLastLine = s;
                }
            }
        }
        
        if (!string.IsNullOrWhiteSpace(currentLastLine))
        {
            lines.Add(currentLastLine);
        }

        if (blocksatz)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                int lineLengthDiff = lineLength - lines[i].Length;
                while (lineLengthDiff > 0)
                {
                    int indexSpace = 0;
                    for (int j = 0; j < lineLengthDiff; j++)
                    {
                        indexSpace = lines[i].IndexOf(' ', indexSpace);
                        if (indexSpace == -1)
                        {
                            indexSpace = lines[i].IndexOf(' ', 0);
                            if (indexSpace == -1)
                            {
                                lines[i] = lines[i] += " ";
                                break;
                            }
                        }
                        if (indexSpace >= 0)
                        {
                            lines[i] = lines[i].Insert(indexSpace, " ");
                            indexSpace += 2;
                        }
                    }
                    lineLengthDiff = lineLength - lines[i].Length;
                }
            }
        }

        return lines.ToArray();
    }
}
