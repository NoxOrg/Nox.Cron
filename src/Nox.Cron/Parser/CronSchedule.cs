namespace Nox.Cron
{
    public struct CronSchedule
    {
        public CronSchedule()
        {
        }

        public string Minutes { get; set; } = string.Empty;
        public string Hours { get; set; } = string.Empty;
        public string DayOfMonth { get; set; } = string.Empty;
        public string Months { get; set; } = string.Empty;
        public string DayOfWeek { get; set; } = string.Empty;
        public string Unparsed { get; set; } = string.Empty;

        public override string ToString() => $"{Minutes} {Hours} {DayOfMonth} {Months} {DayOfWeek}";
        public bool IsFullyParsed() => string.IsNullOrWhiteSpace(Unparsed);
    }
}
