# Nox.Cron
Convert an English phrase to a CRON expression for .NET

```csharp
var cron = "11:34 on 13 June".ToCronExpression().ToString(); // 34 11 13 6 *
```

### To install from nuget.org
```powershell
dotnet add package Nox.Cron
```

### Examples:
English Phrase |Output
-|-
at 5:15am on 13 June|15 5 13 6 *
at 5:15pm on christmas|15 17 25 12 *
Daily at 02:00 CET|0 1 * * *
Daily at 02:00 UTC|0 2 * * *
Daily at    02:00 |0 2 * * *
On Mondays and Fridays at 14:00|0 14 * * 1,5
Every Hour Mondays to Fridays|0 * * * 1-5
Every 30 minutes|*/30 * * * *
Every Hour Mondays to Fridays and Sundays|0 * * * 1-5,0
Every Hour Mondays to Fridays and Sundays in October and December|0 * * 10,12 1-5,0
Every 5 minutes|*/5 * * * *
Every 5 minutes from January to June|*/5 * * 1-6 *
Every 3 months on the 2nd at 8am|0 8 2 */3 *
Every 3 months on the 2nd and 4th at 8am|0 8 2,4 */3 *
At 5pm on even days|0 17 2-30/2 * *
At 5pm on odd days|0 17 1-31/2 * *
every 3rd day at 2:55 am from January to August|55 2 */3 1-8 *
Every Tuesday at 15:00|0 15 * * 2
every day at 9am|0 9 * * *
every 6 hours|0 */6 * * *
each day|0 0 * * *
5:15am every Tuesday|15 5 * * 2
new year|* * 1 1 *
at 5 pm|0 17 * * *
5 pm|0 17 * * *
13 June at 11:34|34 11 13 6 *
11:34 on June 13|34 11 13 6 *
11:34 on Mondays and Fridays|34 11 * * 1,5
11:34 on 13 June|34 11 13 6 *
11:34 on 13 and 16 June|34 11 13,16 6 *
never|0 0 31 2 0
(blank)|0 0 31 2 0
