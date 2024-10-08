using Xunit.Abstractions;

namespace Nox.Cron.Tests;

public class UnitTests
{
    private readonly ITestOutputHelper output;

    public UnitTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void Test_AtFiveFifteenOnThirteenJune()
    {
        Assert.Equal("15 5 13 6 *", "at 5:15am on 13 June".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_AtFiveFifteenOnChristmas()
    {
        Assert.Equal("15 17 25 12 *", "at 5:15pm on christmas".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_DailyAtTwoAmCET()
    {
        Assert.Equal("0 1 * * *", "Daily at 02:00 CET".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_DailyAtTwoAmUTC()
    {
        Assert.Equal("0 2 * * *", "Daily at 02:00 UTC".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_DailyAtTwoAmWithSpaces()
    {
        Assert.Equal("0 2 * * *", "Daily at    02:00 ".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_MondaysAndFridaysAtFourteenHundred()
    {
        Assert.Equal("0 14 * * 1,5", "On Mondays and Fridays at 14:00".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_EveryHourMondaysToFridays()
    {
        Assert.Equal("0 * * * 1-5", "Every Hour Mondays to Fridays".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_EveryThirtyMinutes()
    {
        Assert.Equal("*/30 * * * *", "Every 30 minutes".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_EveryHourMondaystoFridaysandSundays()
    {
        Assert.Equal("0 * * * 1-5,0", "Every Hour Mondays to Fridays and Sundays".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_EveryHourMondaystoFridaysandSundaysinOctoberandDecember()
    {
        Assert.Equal("0 * * 10,12 1-5,0", "Every Hour Mondays to Fridays and Sundays in October and December".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_Every5minutes()
    {
        Assert.Equal("*/5 * * * *", "Every 5 minutes".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_Every5minutesfromJanuarytoJune()
    {
        Assert.Equal("*/5 * * 1-6 *", "Every 5 minutes from January to June".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_Every3monthsonthe2ndat8am()
    {
        Assert.Equal("0 8 2 */3 *", "Every 3 months on the 2nd at 8am".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_Every3monthsonthe2ndand4that8am()
    {
        Assert.Equal("0 8 2,4 */3 *", "Every 3 months on the 2nd and 4th at 8am".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_At5pmonevendays()
    {
        Assert.Equal("0 17 2-30/2 * *", "At 5pm on even days".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_At5pmonodddays()
    {
        Assert.Equal("0 17 1-31/2 * *", "At 5pm on odd days".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_every3rddayat2h55amfromJanuarytoAugust()
    {
        Assert.Equal("55 2 */3 1-8 *", "every 3rd day at 2:55 am from January to August".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_EveryTuesdayat15h00()
    {
        Assert.Equal("0 15 * * 2", "Every Tuesday at 15:00".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_never()
    {
        Assert.Equal("0 0 31 2 0", "never".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_Never()
    {
        Assert.Equal("0 0 31 2 0", "Never".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_blank_Isnever()
    {
        Assert.Equal("0 0 31 2 0", "".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_every_isUnparsed()
    {
        var result = "every".ToCronExpression();

        Assert.Equal("* * * * *", result.ToString());
        Assert.Equal("every", result.Unparsed);

        output.WriteLine("Assumes this runs always (every minute)");
    }

    [Fact]
    public void Test_everydayat9am()
    {
        Assert.Equal("0 9 * * *", "every day at 9am".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_every6hours()
    {
        Assert.Equal("0 */6 * * *", "every 6 hours".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_every2minutes_as_words()
    {
        Assert.Equal("0 */2 * * *", "every two hours".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_eachday()
    {
        Assert.Equal("0 0 * * *", "each day".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_5h15ameveryTuesday()
    {
        Assert.Equal("15 5 * * 2", "5:15am every Tuesday".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_newyear()
    {
        Assert.Equal("* * 1 1 *", "new year".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_at5pm()
    {
        Assert.Equal("0 17 * * *", "at 5 pm".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_5pm()
    {
        Assert.Equal("0 17 * * *", "5 pm".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_13JuneAt11h34()
    {
        Assert.Equal("34 11 13 6 *", "13 June at 11:34".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_11h34onJune13()
    {
        Assert.Equal("34 11 13 6 *", "11:34 on June 13".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_11h34onMondaysandFridays()
    {
        Assert.Equal("34 11 * * 1,5", "11:34 on Mondays and Fridays".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_11h34on13June()
    {
        Assert.Equal("34 11 13 6 *", "11:34 on 13 June".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_11h34on13and16June()
    {
        Assert.Equal("34 11 13,16 6 *", "11:34 on 13 and 16 June".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_DailyAt2amOnFridays()
    {
        Assert.Equal("0 2 * * 5", "Daily at 2am UTC on Fridays".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_EveryMorningat2_05()
    {
        Assert.Equal("5 2 * * *", "Every morning at 2:05".ToCronExpression().ToString());
    }

    [Fact]
    public void Test_DailyAt2_30am()
    {
        Assert.Equal("30 2 * * *", "Daily at 2:30am".ToCronExpression().ToString());
    }

    [Fact]
    public void _10MinutesPastMidnight()
    {
        Assert.Equal("10 0 * * *", "10 minutes past midnight".ToCronExpression().ToString());
    }

    [Fact]
    public void HalfPastMidnight()
    {
        Assert.Equal("30 0 * * *", "half past midnight".ToCronExpression().ToString());
    }

    [Fact]
    public void NineteenAfterMidnight()
    {
        Assert.Equal("19 0 * * *", "19 after midnight".ToCronExpression().ToString());
    }

    [Fact]
    public void DailyAt230Am()
    {
        Assert.Equal("30 2 * * *", "Daily at 2:30am".ToCronExpression().ToString());
    }
}