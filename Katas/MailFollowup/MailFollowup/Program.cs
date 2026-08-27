
using System.Text.RegularExpressions;
// ReSharper disable InconsistentNaming

namespace MailFollowup;

/// <summary>
///     Function Kata: Mail Followup
///     Aufgabenbeschreibung: siehe README.md in diesem Projekt.
///     Quelle: https://ccd-school.de/coding-dojo/function-katas/mail-followup/
/// </summary>
public class Program
{
    private enum Months
    {
        jan = 1,
        feb = 2,
        mar = 3,
        apr = 4,
        may = 5,
        jun = 6,
        jul = 7,
        aug = 8,
        sep = 9,
        oct = 10,
        nov = 11,
        dec = 12,
    }

    public static DateTime FollowupZeitpunkt(DateTime now, string emailadresse)
    {
        DateTime followupZeitpunkt = now;
        string timeStuff = emailadresse.Split('@')[0];
        string generalTimeW = "([0-9]+)week|weeks";
        string generalTimeD = "([0-9]+)day|days";
        string generalTimeH = "([0-9]+)hour|hours";
        string generalTimeM = "([0-9]+)minute|minutes";
        string specificTimeM = $"({string.Join('|', Enum.GetNames<Months>())})([0-9]+)";
        string specificTimeAmPm = "([0-9]+)(am|pm)";
        
        Match matchResult =  Regex.Match(timeStuff, generalTimeW);
        if (matchResult.Success)
        {
            int.TryParse(matchResult.Groups[1].Value, out int weeks);
            {
                followupZeitpunkt = followupZeitpunkt.AddDays(weeks*7);
            }
        }
        
        matchResult =  Regex.Match(timeStuff, generalTimeD);
        if (matchResult.Success)
        {
            int.TryParse(matchResult.Groups[1].Value, out int days);
            {
                followupZeitpunkt = followupZeitpunkt.AddDays(days);
            }
        }
        
        matchResult =  Regex.Match(timeStuff, generalTimeH);
        if (matchResult.Success)
        {
            int.TryParse(matchResult.Groups[1].Value, out int hours);
            {
                followupZeitpunkt = followupZeitpunkt.AddHours(hours);
            }
        }
        
        matchResult =  Regex.Match(timeStuff, generalTimeM);
        if (matchResult.Success)
        {
            int.TryParse(matchResult.Groups[1].Value, out int minutes);
            {
                followupZeitpunkt = followupZeitpunkt.AddMinutes(minutes);
            }
        }
        
        matchResult =  Regex.Match(timeStuff, specificTimeM);
        if (matchResult.Success)
        {
            int day = followupZeitpunkt.Day;
            
            if(int.TryParse(matchResult.Groups[2].Value, out int days))
            {
                day = days;
            }
            
            if (Enum.TryParse(matchResult.Groups[1].Value, out Months months))
            {
                followupZeitpunkt = new  DateTime(followupZeitpunkt.Year, (int) months, day, followupZeitpunkt.Hour, followupZeitpunkt.Minute, followupZeitpunkt.Second);
            }
        }
        
        matchResult =  Regex.Match(timeStuff, specificTimeAmPm);
        if (matchResult.Success)
        {
            string amPm = matchResult.Groups[2].Value;
            int.TryParse(matchResult.Groups[1].Value, out int oClock);
            {
                if (string.Equals("AM", amPm, StringComparison.CurrentCultureIgnoreCase))
                {
                    followupZeitpunkt = new  DateTime(followupZeitpunkt.Year, followupZeitpunkt.Month, followupZeitpunkt.Day, oClock, 0, 00);
                }
                else
                {
                    followupZeitpunkt = new  DateTime(followupZeitpunkt.Year, followupZeitpunkt.Month, followupZeitpunkt.Day, 12 + oClock, 0, 00);
                }
            }
        }
        
        return followupZeitpunkt;
    }
}
