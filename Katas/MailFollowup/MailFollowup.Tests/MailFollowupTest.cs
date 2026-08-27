using Xunit;

namespace MailFollowup.Tests;

public class MailFollowupTest
{
    // 02.04.2026 10:30
    private static DateTime _test = new (2026,2,4,10,30,0);

    public static TheoryData<string, DateTime> MailFollupTests = new()
    {
        { "7days@followup.cc", _test.AddDays(7) },
        { "12hours@followup.cc", _test.AddHours(12) },
        { "aug15-9am@followup.cc", new(2026, 8, 15, 9, 0, 0) },
        { "1week3days5hours@followup.cc", _test.AddDays(10).AddHours(5) },
    };
        
    [Theory, MemberData(nameof(MailFollupTests))]
    public void TestFollowupZeitpunkt(string followupMail, DateTime expected)
    {
        DateTime result = Program.FollowupZeitpunkt(_test, followupMail);
        Assert.Equal(expected, result);
    }
}