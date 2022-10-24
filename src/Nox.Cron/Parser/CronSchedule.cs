namespace Nox.Cron
{
    public struct CronSchedule
    {
        public CronSchedule()
        {
        }

        public string Minutes { get; init; } = string.Empty;
        public string Hours { get; init; } = string.Empty;
        public string DayOfMonth { get; init; } = string.Empty;
        public string Months { get; init; } = string.Empty;
        public string DayOfWeek { get; init; } = string.Empty;
        public string Unparsed { get; init; } = string.Empty;

        public override string ToString() => $"{Minutes} {Hours} {DayOfMonth} {Months} {DayOfWeek}";
        public bool IsFullyParsed() => string.IsNullOrWhiteSpace(Unparsed);
    }
}
