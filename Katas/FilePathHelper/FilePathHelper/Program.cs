
namespace FilePathHelper;

/// <summary>
///     Function Kata: File Path Helper
///     Aufgabenbeschreibung: siehe README.md in diesem Projekt.
///     Quelle: https://ccd-school.de/en/coding-dojo/function-katas/file-path-helper/
/// </summary>
public class Program
{
    public static string GetAbsolutFilePath(string input)
    {
        //Splits windows/unix paths if input wrong
        string[] partsOfPath = input.Split('/').SelectMany(x => x.Split('\\')).ToArray();
        
        string basePath = string.Empty;
        int startIndex = 0;
        
        //In Kata this is supposed to be the user-root
        if (string.Equals(partsOfPath[0], "~"))
        {
            basePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            startIndex = 1;
        }
        //In Kata this is supposed current working folder
        else if (string.Equals(partsOfPath[0], "."))
        {
            basePath = Environment.CurrentDirectory;
            startIndex = 1;
        }

        //In Kata this is supposed current working folder's parent folder
        else if (string.Equals(partsOfPath[0], ".."))
        {
            basePath = new DirectoryInfo(Environment.CurrentDirectory).Parent!.FullName;
            startIndex = 1;
        }
        
        /*
         * Die Handhabung mit .. "inmitten" der Pfadangabe entspringt der Aufgabenstellung des Katas
         * Eigentlich müsste man bei vorkommen von '..' immer prüfen, ob das aktuelle Verzeichnis existiert und dann den
         * Parent Pfad als Base PFad setzen, aber so sind die Beispiele aus dem kata nicht gestellt..
         */
        
        string path = basePath;
        
        //Das hier wäre die eigentliche Implementierung: 
        //for (int i = startIndex; i < partsOfPath.Length; i++)
        //{
        //    if (string.Equals(partsOfPath[i], ".."))
        //    {
        //        DirectoryInfo directoryInfo = new DirectoryInfo(path);
        //        path = directoryInfo.Parent!.Name;
        //        continue;
        //    }
        //    path = Path.Combine(path, partsOfPath[i]);
        //}
        
        for (int i = startIndex; i < partsOfPath.Length; i++)
        {
            if (string.Equals(partsOfPath[i], ".."))
            {
                partsOfPath[i] = string.Empty;
                partsOfPath[i-1] = string.Empty;
            }
        }
        for (int i = startIndex; i < partsOfPath.Length; i++)
        {
            if (string.IsNullOrEmpty(partsOfPath[i]))
            {
                continue;
            }
            path = Path.Combine(path, partsOfPath[i]);
        }

        return path;
    }
}
