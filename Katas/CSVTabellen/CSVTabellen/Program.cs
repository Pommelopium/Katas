using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSVTabellen;

class Program
{
    static void Main(string[] _)
    {
        List<string> input = new List<string>
        {
            "Name;Strasse;Ort;Alter;Gurke",
            "Peter Pan;Am Hang 5;12345 Einsam;42",
            "Maria Schmitz;Kölner Straße 45;50123 Köln;43;nein",
            "Paul Meier;Münchener Weg 1;87654 München;65;manchmal",
        };
        
        foreach (string result in Tabellieren(input))
        {
            Console.WriteLine(result);
        }
    }

    private static IEnumerable<string> Tabellieren(IEnumerable<string> csvZeilen, bool hatUeberschrift = true)
    {
        // Um Enumeration fehler zu vermeiden, als Liste umwandeln
        List<string> inputValues = csvZeilen.ToList();
        if (inputValues.Any() == false)
        {
            // Leerer Input = Leerer Output
            return Array.Empty<string>();
        }

        string[][] cellsPerRow = inputValues.Select(zeile => zeile.Split(';')).ToArray();
        int rows = cellsPerRow.Length;
        // Die maximale Anzahl an Spalten kann Variabel sein
        int columns = cellsPerRow.Select(row => row.Length).Max();
        int[] charsMaxLengthPerColumn = new int[columns];

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                // Wenn eine Reihe nicht die gesamtanzahl an Columns hat, den Schritt überspringen
                if (column >= cellsPerRow[row].Length)
                {
                    continue;
                }

                //Ermittelt die gesamte länge pro Spalte um gleichartig zu Formatieren
                if(charsMaxLengthPerColumn[column] < cellsPerRow[row][column].Length + 1)
                {
                    charsMaxLengthPerColumn[column] = cellsPerRow[row][column].Length + 1;
                }
            }
        }

        List<string> result = new List<string>();
        for (int row = 0; row < rows; row++)
        {
            StringBuilder rowText = new StringBuilder();
            for (int column = 0; column < columns; column++)
            {
                if (column >= cellsPerRow[row].Length)
                {
                    rowText.Append(string.Empty.PadRight(charsMaxLengthPerColumn[column]));
                    rowText.Append('|');
                }
                else
                {
                    rowText.Append(cellsPerRow[row][column].PadRight(charsMaxLengthPerColumn[column]));
                    rowText.Append('|');
                }
            }
            result.Add(rowText.ToString());
        }

        if (hatUeberschrift)
        {
            //Ermittelt die Länge an - zeichen pro spalte für die Trennungszeile von Tabellenname zu Spalten
            StringBuilder tableNameSeparator = new StringBuilder();
            foreach (int length in charsMaxLengthPerColumn)
            {
                for (int j = 0; j < length; j++)
                {
                    tableNameSeparator.Append('-');
                }
                tableNameSeparator.Append('+');
            }
            result.Insert(1, tableNameSeparator.ToString());
        }

        return result;
    }
}
