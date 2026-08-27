
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
        // (?<name>..) makes a group and accessable by name > matchResult.Groups["name"]
        // \s*  makes up for any numbner of white spaces, not possible in mail adresses but is not absolutly wrong 
        string generalTimeW = "(?<weeks>[0-9]+)\\s*weeks?";
        string generalTimeD = "(?<days>[0-9]+)days?";
        string generalTimeH = "(?<hours>[0-9]+)hours?";
        string generalTimeM = "(?<minutes>[0-9]+)minutes?";
        string specificTimeM = $"(?<month>{string.Join('|', Enum.GetNames<Months>())})(?<dOfMonth>[0-9]+)";
        string specificTimeAmPm = "(?<oclock>[0-9]+)(?<timeOfDay>am|pm)";
        
        Match matchResult =  Regex.Match(timeStuff, generalTimeW);
        if (matchResult.Success)
        {
            int.TryParse(matchResult.Groups["weeks"].Value, out int weeks);
            {
                followupZeitpunkt = followupZeitpunkt.AddDays(weeks*7);
            }
        }
        
        matchResult =  Regex.Match(timeStuff, generalTimeD);
        if (matchResult.Success)
        {
            int.TryParse(matchResult.Groups["days"].Value, out int days);
            {
                followupZeitpunkt = followupZeitpunkt.AddDays(days);
            }
        }
        
        matchResult =  Regex.Match(timeStuff, generalTimeH);
        if (matchResult.Success)
        {
            int.TryParse(matchResult.Groups["hours"].Value, out int hours);
            {
                followupZeitpunkt = followupZeitpunkt.AddHours(hours);
            }
        }
        
        matchResult =  Regex.Match(timeStuff, generalTimeM);
        if (matchResult.Success)
        {
            int.TryParse(matchResult.Groups["minutes"].Value, out int minutes);
            {
                followupZeitpunkt = followupZeitpunkt.AddMinutes(minutes);
            }
        }
        
        matchResult =  Regex.Match(timeStuff, specificTimeM);
        if (matchResult.Success)
        {
            int day = followupZeitpunkt.Day;
            
            if(int.TryParse(matchResult.Groups["dOfMonth"].Value, out int days))
            {
                day = days;
            }
            
            if (Enum.TryParse(matchResult.Groups["month"].Value, out Months months))
            {
                followupZeitpunkt = new  DateTime(followupZeitpunkt.Year, (int) months, day, followupZeitpunkt.Hour, followupZeitpunkt.Minute, followupZeitpunkt.Second);
            }
        }
        
        matchResult =  Regex.Match(timeStuff, specificTimeAmPm);
        if (matchResult.Success)
        {
            string amPm = matchResult.Groups["timeOfDay"].Value;
            int.TryParse(matchResult.Groups["oclock"].Value, out int oClock);
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
