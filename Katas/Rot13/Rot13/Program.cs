
namespace Rot13;

/// <summary>
///     Function Kata: ROT-13
///     Aufgabenbeschreibung: siehe README.md in diesem Projekt.
///     Quelle: https://ccd-school.de/coding-dojo/function-katas/rot-13/
/// </summary>
public class Program
{
    public static string RotVerschluesseln(string content, int shift = 13)
    {
        string contentUpdated = content.ToUpper()
            .Replace("ß", "SS")
            .Replace("Ü", "UE")
            .Replace("Ö", "OE")
            .Replace("Ä", "AE");
        
        // Großbuchstaben (A–Z): 65 bis 90 ('A' ist 65, 'Z' ist 90)
        // Das Zeichen '0' hat den numerischen Wert 48. Das Zeichen '9' hat den numerischen Wert 57.
        char[] chars = new  char[contentUpdated.Length];
        for (int i = 0; i < contentUpdated.Length; i++)
        {
            char curr = contentUpdated[i];

            int currInt = curr;
            
            //Eine Zahl
            if (currInt is >= 48 and <= 57)
            {
                currInt = ToNewCharInRange(48, 57, currInt, shift);
            }
            //Ein Buchstabe
            else if (currInt is >= 65 and <= 90)
            {
                currInt = ToNewCharInRange(65, 90, currInt, shift);
            }
            
            chars[i] = (char)currInt;
        }
        
        return new string(chars);
    }

    private static int ToNewCharInRange(int start, int end, int charStart, int shift)
    {
        int charShift = charStart + shift;
        if (charShift > end)
        {
            charShift = charShift - end + start -1;
            if (charShift > end)
            {
                return ToNewCharInRange(start, end, start, charShift - end -1);
            }
        }
        return charShift;
    }
}
