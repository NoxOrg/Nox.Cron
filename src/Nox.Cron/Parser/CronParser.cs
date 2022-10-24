using System.Text;

namespace Nox.Cron
{

    /// <summary>
    /// Extension method to convert English phrase to a CRON expression
    /// </summary>
    public static class CronParserExtension
    {
        private struct CronSheduleBuilder
        {
            internal string Minutes = string.Empty;
            internal string Hours = string.Empty;
            internal string DayOfMonth = string.Empty;
            internal string Months = string.Empty;
            internal string DayOfWeek = string.Empty;
            internal string Unparsed = string.Empty;

            public CronSheduleBuilder()
            {
            }
        }

        /// <summary>
        /// Converts an English phrase to a CRON expresion
        /// </summary>
        /// <param name="englishPhrase">the english phrase to convert to a CRON expression.</param>
        public static CronSchedule ToCronExpresssion(this string englishPhrase)
        {
            var schedule = new CronSheduleBuilder();

            var sbPhrase = new StringBuilder(englishPhrase.Trim().ToLower());

            // interpret blank as "never"

            if (sbPhrase.Length == 0)
            {
                sbPhrase.Append("never");
            }

            // '-' in input implies "to"

            sbPhrase.Replace("-", " to ");

            // keep lowercase alpha, numbers, colon and plus - ignore rest
            for (var i = 0; i < sbPhrase.Length; i++)
            {
                if (!" +:0123456789abcdefghijklmnopqrstuvwxyz".Contains(sbPhrase[i])) 
                    sbPhrase[i] = '_';
            }
            sbPhrase.Replace("_", "");

            // expand frequency words
            sbPhrase.Replace("hourly", "every hour");
            sbPhrase.Replace("daily", "every day");
            sbPhrase.Replace("weekly", "every week");
            sbPhrase.Replace("monthly", "every month");
            sbPhrase.Replace("yearly", "every year");
            sbPhrase.Replace("annually", "every year");

            // expand well known holidays
            sbPhrase.Replace("christmas day", "25 dec");
            sbPhrase.Replace("christmas", "25 dec");
            sbPhrase.Replace("new years day", "1 jan");
            sbPhrase.Replace("new years", "1 jan");
            sbPhrase.Replace("new year", "1 jan");
            sbPhrase.Replace("valentines day", "14 feb");
            sbPhrase.Replace("valentines", "14 feb");
            sbPhrase.Replace("valentine", "14 feb");
            sbPhrase.Replace("st patricks day", "17 mar");
            sbPhrase.Replace("halloween", "31 oct");
            sbPhrase.Replace("independance day", "4 jul");

            // Replace synonymns 
            var words = sbPhrase.ToString()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Synonymn(x))
                .ToList();

            // tokenize "from" as in "from January to August" or "from 10am to 11am"
            var phraseFrom = words.IndexOf("from");
            if (phraseFrom >= 0)
            {
                words[phraseFrom] = words.Contains("[T]") ? "[D]" : "[T]";
            }
            
            // if there is a date implied, assume first phrase is a time and vice versa
            if (!words[0].StartsWith('['))
            {
                if (words.Contains("[T]") && !words.Contains("[D]"))
                    words.Insert(0, "[D]");
                
                else if (!words.Contains("[T]") && words.Contains("[D]"))
                    words.Insert(0, "[T]");
            }

            // handle "every xxx day|hour|<minutes|<hours>|<days>|...

            var everyStartPos = words.IndexOf("every");
            if (everyStartPos > -1 && words.Count > everyStartPos+1)
            {
                var sbEvery = new StringBuilder();
                var everyWordCount = 0;
                for (var i = everyStartPos; i < words.Count; i++)
                {
                    if (words[i].StartsWith('[')) break;
                    everyWordCount++;
                    sbEvery.Append(words[i]);
                    sbEvery.Append(' ');
                }

                var every = sbEvery.ToString().TrimEnd();
                var isEveryHandled = true;

                switch ($"{words[everyStartPos]} {words[everyStartPos+1]}")
                {
                    case "every minute":
                        everyWordCount = 2;
                        schedule.Minutes = "*";
                        break;

                    case "every hour":
                        everyWordCount = 2;
                        schedule.Hours = "*";
                        schedule.Minutes = "0";
                        break;

                    case "every day":
                        everyWordCount = 2;
                        schedule.DayOfMonth = "*";
                        schedule.DayOfWeek = "*";
                        schedule.Hours = "0";
                        schedule.Minutes = "0";
                        break;

                    case "every month":
                        schedule.Minutes = "1";
                        schedule.Hours = "1";
                        schedule.DayOfMonth = "1";
                        schedule.Months = "*";
                        break;

                    case "every year":
                        everyWordCount = 2;
                        schedule.Minutes = "1";
                        schedule.Hours = "1";
                        schedule.Months = "1";
                        schedule.DayOfMonth = "1";
                        break;

                    default:
                        isEveryHandled = false;
                        break;

                }

                if (!isEveryHandled)
                {
                    var everyParts = every.Split(' ').Reverse().ToArray();
                    
                    var everyPartString = "*/"+
                        string.Join(',', 
                            everyParts.Skip(1).Reverse().Skip(1).ToArray()
                        );

                    isEveryHandled = true;
                    
                    switch (everyParts[0])
                    {
                        case "minute":
                            schedule.Minutes = everyPartString;
                            break;

                        case "hour":
                            schedule.Minutes = "0";
                            schedule.Hours = everyPartString;
                            break;

                        case "day":
                            schedule.DayOfMonth = everyPartString;
                            break;

                        case "month":
                            schedule.Months = everyPartString;
                            break;

                        default:
                            isEveryHandled=false;
                            break;

                    }
                }

                if (isEveryHandled)
                {
                    for (var i = everyStartPos + everyWordCount - 1; i >= everyStartPos; i--)
                    {
                        words.RemoveAt(i);
                    }
                }
                else
                {
                    words.RemoveAt(everyStartPos);
                }
            }

            // convert month and day names

            var days = new List<string>()
            {
                "sun", "mon", "tue", "wed", "thu", "fri", "sat"
            };

            var months = new List<string>()
            {
                "_", "jan", "feb", "mar", "apr", "may", "jun", "jul", "aug", "sep", "oct", "nov", "dec"
            };

            var sbMonths = new StringBuilder(schedule.Months);

            var sbDayOfWeek = new StringBuilder(schedule.DayOfWeek);

            var isDaySequence = false;

            for(var i= 0; i < words.Count; i++ )
            {
                if (words[i].StartsWith("["))
                {
                    // ignore all tokens
                }

                else if (days.Contains(words[i]))
                {
                    isDaySequence = true;
                    sbDayOfWeek.Append(days.IndexOf(words[i]));
                    sbDayOfWeek.Append('_');
                    words[i] = "";
                }

                else if (months.Contains(words[i]))
                {
                    isDaySequence = false;
                    sbMonths.Append(months.IndexOf(words[i]));
                    sbMonths.Append('_');
                    words[i] = "";
                }

                else if (words[i].Equals("-"))
                {
                    if (isDaySequence)
                    {
                        sbDayOfWeek.Append("-_");
                    }
                    else
                    {
                        sbMonths.Append("-_");
                    }
                    words[i] = "";
                }
            }

            sbMonths.Replace("_-_", "-");
            sbMonths.Replace("_", ",");
            schedule.Months = sbMonths.ToString().TrimEnd(',');

            sbDayOfWeek.Replace("_-_", "-");
            sbDayOfWeek.Replace("_", ",");
            schedule.DayOfWeek = sbDayOfWeek.ToString().TrimEnd(',');

            // convert "odd" and "even" phrases

            for (var i = 0; i < words.Count-1; i++)
            {
                var thisAndNextWord = $"{words[i]} {words[i+1]}";

                if (thisAndNextWord.Equals("odd minute"))
                {
                    schedule.Minutes = "1-59/2";
                    words[i] = "";
                    words[i+1] = "";
                }

                else if (thisAndNextWord.Equals("even minute"))
                {
                    schedule.Minutes = "2-60/2";
                    words[i] = "";
                    words[i+1] = "";
                }

                else if (thisAndNextWord.Equals("odd day"))
                {
                    schedule.DayOfMonth = "1-31/2";
                    words[i] = "";
                    words[i+1] = "";
                }

                else if (thisAndNextWord.Equals("even day"))
                {
                    schedule.DayOfMonth = "2-30/2";
                    words[i] = "";
                    words[i+1] = "";
                }

                else if (thisAndNextWord.Equals("odd month"))
                {
                    schedule.Months = "1-11/2";
                    words[i] = "";
                    words[i+1] = "";
                }

                else if (thisAndNextWord.Equals("even month"))
                {
                    schedule.Months = "2-12/2";
                    words[i] = "";
                    words[i+1] = "";
                }

            }

            // handle "never"

            if (words.IndexOf("never") > -1)
            {
                schedule.Minutes = "0";
                schedule.Hours = "0";
                schedule.DayOfMonth = "31";
                schedule.Months = "2";
                schedule.DayOfWeek = "0";

                words.Remove("never");
            }


            // handle anything unparsed that may be a time or day-of-month

            words = string.Join(' ', words.ToArray())
                .Trim()
                .Replace("[D]","_")
                .Replace("[T]","_")
                .Split('_', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .ToList();

            for (var i = words.Count - 1; i > -1; i--)
            {
                var timeString = $"{DateTime.MinValue:yyyy-MM-dd} {words[i]}";

                if (!timeString.Contains('+'))
                {
                    timeString += "+00:00";
                }

                if (DateTimeOffset.TryParse(timeString, out var parsedTime))
                {
                    schedule.Hours = parsedTime.UtcDateTime.Hour.ToString();
                    schedule.Minutes = parsedTime.UtcDateTime.Minute.ToString();
                    words.RemoveAt(i);
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(words[i]) && words[i].All(c => Char.IsWhiteSpace(c) || Char.IsDigit(c)))
                {
                    var elements = words[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    schedule.DayOfMonth = string.Join(',',elements);
                    words.RemoveAt(i);
                    continue;
                }

            }

            // default all elements of schedule to "*"

            if (string.IsNullOrWhiteSpace(schedule.Minutes))
                schedule.Minutes = "*";

            if (string.IsNullOrWhiteSpace(schedule.Hours))
                schedule.Hours = "*";

            if (string.IsNullOrWhiteSpace(schedule.DayOfMonth))
                schedule.DayOfMonth = "*";

            if (string.IsNullOrWhiteSpace(schedule.Months))
                schedule.Months = "*";

            if (string.IsNullOrWhiteSpace(schedule.DayOfWeek))
                schedule.DayOfWeek = "*";

            schedule.Unparsed = string.Join(' ',words.ToArray()).Trim();

            // done

            return new CronSchedule
            {
                Minutes = schedule.Minutes,
                Hours = schedule.Hours,
                DayOfMonth = schedule.DayOfMonth,
                Months = schedule.Months,
                DayOfWeek = schedule.DayOfWeek,
                Unparsed = schedule.Unparsed,
            };
        }

        private static string Synonymn(string word)
        {
            return word switch
            {
                // Synonymns - plurals

                "minutes" => "minute",
                "days" => "day",
                "hours" => "hour",
                "weeks" => "week",
                "months" => "month",
                "years" => "year",

                // Synonymns - numbers

                "first" => "1",
                "second" => "2",
                "third" => "3",
                "fourth" => "4",
                "fifth" => "5",
                "sixth" => "6",
                "seventh" => "7",
                "eighth" => "8",
                "ninth" => "9",
                "tenth" => "10",
                "eleventh" => "11",
                "twelfth" => "12",
                "thirteenth" => "13",

                "1st" => "1",
                "2nd" => "2",
                "3rd" => "3",
                "4th" => "4",
                "5th" => "5",
                "6th" => "6",
                "7th" => "7",
                "8th" => "8",
                "9th" => "9",
                "10th" => "10",
                "11th" => "11",
                "12th" => "12",
                "13th" => "13",
                "14th" => "14",
                "15th" => "15",
                "16th" => "16",
                "17th" => "17",
                "18th" => "18",
                "19th" => "19",
                "20th" => "20",
                "21st" => "21",
                "22nd" => "22",
                "23rd" => "23",
                "24th" => "24",
                "25th" => "25",
                "26th" => "26",
                "27th" => "27",
                "28th" => "28",
                "29th" => "29",
                "30th" => "30",
                "31st" => "31",
                "32nd" => "32",
                "33rd" => "33",
                "34th" => "34",
                "35th" => "35",
                "36th" => "36",
                "37th" => "37",
                "38th" => "38",
                "39th" => "39",
                "40th" => "40",
                "41st" => "41",
                "42nd" => "42",
                "43rd" => "43",
                "44th" => "44",
                "45th" => "45",
                "46th" => "46",
                "47th" => "47",
                "48th" => "48",
                "49th" => "49",
                "50th" => "50",
                "51st" => "51",
                "52nd" => "52",
                "53rd" => "53",
                "54th" => "54",
                "55th" => "55",
                "56th" => "56",
                "57th" => "57",
                "58th" => "58",
                "59th" => "59",

                // Synonymns - time

                "noon" => "12:00",
                "midnight" => "00:00",
                "morning" => "10:00",
                "afternoon" => "15:00",
                "evening" => "20:00",

                // Synonymns - days

                "sunday" => "sun",
                "monday" => "mon",
                "tuesday" => "tue",
                "wednesday" => "wed",
                "thursday" => "thu",
                "friday" => "fri",
                "saturday" => "sat",
                "sundays" => "sun",
                "mondays" => "mon",
                "tuesdays" => "tue",
                "wednesdays" => "wed",
                "thursdays" => "thu",
                "fridays" => "fri",
                "saturdays" => "sat",

                // Synonymns - months

                "january" => "jan",
                "february" => "feb",
                "march" => "mar",
                "april" => "apr",
                "may" => "may",
                "june" => "jun",
                "july" => "jul",
                "august" => "aug",
                "september" => "sep",
                "october" => "oct",
                "november" => "nov",
                "december" => "dec",


                // time zones
                "acdt" => "+10:30",
                "acst" => "+09:30",
                "act" => "+08:00",
                "acwst" => "+08:45",
                "adt" => "−03:00",
                "aedt" => "+11:00",
                "aest" => "+10:00",
                "aet" => "+10:00",
                "aft" => "+04:30",
                "akdt" => "−08:00",
                "akst" => "−09:00",
                "almt" => "+06:00",
                "amst" => "−03:00",
                "amt" => "+04:00",
                "anat" => "+12:00",
                "aqtt" => "+05:00",
                "art" => "−03:00",
                "ast" => "+03:00",
                "awst" => "+08:00",
                "azost" => "+00:00",
                "azot" => "−01:00",
                "azt" => "+04:00",
                "bnt" => "+08:00",
                "biot" => "+06:00",
                "bit" => "−12:00",
                "bot" => "−04:00",
                "brst" => "−02:00",
                "brt" => "−03:00",
                "bst" => "+01:00",
                "btt" => "+06:00",
                "cat" => "+02:00",
                "cct" => "+06:30",
                "cdt" => "−05:00",
                "cest" => "+02:00",
                "cet" => "+01:00",
                "chadt" => "+13:45",
                "chast" => "+12:45",
                "chot" => "+08:00",
                "chost" => "+09:00",
                "chst" => "+10:00",
                "chut" => "+10:00",
                "cist" => "−08:00",
                "ckt" => "−10:00",
                "clst" => "−03:00",
                "clt" => "−04:00",
                "cost" => "−04:00",
                "cot" => "−05:00",
                "cst" => "+08:00",
                "ct" => "−06:00",
                "cvt" => "−01:00",
                "cwst" => "+08:45",
                "cxt" => "+07:00",
                "davt" => "+07:00",
                "ddut" => "+10:00",
                "dft" => "+01:00",
                "easst" => "−05:00",
                "east" => "−06:00",
                "eat" => "+03:00",
                "ect" => "−04:00",
                "edt" => "−04:00",
                "eest" => "+03:00",
                "eet" => "+02:00",
                "egst" => "±00:00",
                "egt" => "−01:00",
                "est" => "−05:00",
                "et" => "−05:00",
                "fet" => "+03:00",
                "fjt" => "+12:00",
                "fkst" => "−03:00",
                "fkt" => "−04:00",
                "fnt" => "−02:00",
                "galt" => "−06:00",
                "gamt" => "−09:00",
                "get" => "+04:00",
                "gft" => "−03:00",
                "gilt" => "+12:00",
                "git" => "−09:00",
                "gmt" => "+00:00",
                "gst" => "+04:00",
                "gyt" => "−04:00",
                "hdt" => "−09:00",
                "haec" => "+02:00",
                "hst" => "−10:00",
                "hkt" => "+08:00",
                "hmt" => "+05:00",
                "hovst" => "+08:00",
                "hovt" => "+07:00",
                "ict" => "+07:00",
                "idlw" => "−12:00",
                "idt" => "+03:00",
                "iot" => "+03:00",
                "irdt" => "+04:30",
                "irkt" => "+08:00",
                "irst" => "+03:30",
                "ist" => "+05:30",
                "jst" => "+09:00",
                "kalt" => "+02:00",
                "kgt" => "+06:00",
                "kost" => "+11:00",
                "krat" => "+07:00",
                "kst" => "+09:00",
                "lhst" => "+10:30",
                "lint" => "+14:00",
                "magt" => "+12:00",
                "mart" => "−09:30",
                "mawt" => "+05:00",
                "mdt" => "−06:00",
                "met" => "+01:00",
                "mest" => "+02:00",
                "mht" => "+12:00",
                "mist" => "+11:00",
                "mit" => "−09:30",
                "mmt" => "+06:30",
                "msk" => "+03:00",
                "mst" => "+08:00",
                "mut" => "+04:00",
                "mvt" => "+05:00",
                "nct" => "+11:00",
                "ndt" => "−02:30",
                "nft" => "+11:00",
                "novt" => "+07:00",
                "npt" => "+05:45",
                "nst" => "−03:30",
                "nt" => "−03:30",
                "nut" => "−11:00",
                "nzdt" => "+13:00",
                "nzst" => "+12:00",
                "omst" => "+06:00",
                "orat" => "+05:00",
                "pdt" => "−07:00",
                "pet" => "−05:00",
                "pett" => "+12:00",
                "pgt" => "+10:00",
                "phot" => "+13:00",
                "pht" => "+08:00",
                "phst" => "+08:00",
                "pkt" => "+05:00",
                "pmdt" => "−02:00",
                "pmst" => "−03:00",
                "pont" => "+11:00",
                "pst" => "−08:00",
                "pwt" => "+09:00",
                "pyst" => "−03:00",
                "pyt" => "−04:00",
                "ret" => "+04:00",
                "rott" => "−03:00",
                "sakt" => "+11:00",
                "samt" => "+04:00",
                "sast" => "+02:00",
                "sbt" => "+11:00",
                "sct" => "+04:00",
                "sdt" => "−10:00",
                "sgt" => "+08:00",
                "slst" => "+05:30",
                "sret" => "+11:00",
                "srt" => "−03:00",
                "sst" => "−11:00",
                "syot" => "+03:00",
                "taht" => "−10:00",
                "tha" => "+07:00",
                "tft" => "+05:00",
                "tjt" => "+05:00",
                "tkt" => "+13:00",
                "tlt" => "+09:00",
                "tmt" => "+05:00",
                "trt" => "+03:00",
                "tot" => "+13:00",
                "tvt" => "+12:00",
                "ulast" => "+09:00",
                "ulat" => "+08:00",
                "utc" => "+00:00",
                "uyst" => "−02:00",
                "uyt" => "−03:00",
                "uzt" => "+05:00",
                "vet" => "−04:00",
                "vlat" => "+10:00",
                "volt" => "+03:00",
                "vost" => "+06:00",
                "vut" => "+11:00",
                "wakt" => "+12:00",
                "wast" => "+02:00",
                "wat" => "+01:00",
                "west" => "+01:00",
                "wet" => "+00:00",
                "wib" => "+07:00",
                "wit" => "+09:00",
                "wita" => "+08:00",
                "wgst" => "−02:00",
                "wgt" => "−03:00",
                "wst" => "+08:00",
                "yakt" => "+09:00",
                "yekt" => "+05:00",

                // range words

                "and" => "",
                "the" => "",
                "through" => "-",
                "to" => "-",

                // special token words

                "at" => "[T]",
                "on" => "[D]",
                "in" => "[D]",

                // other

                "each" => "every",


                // no synonymn

                _ => word,
            };
        }

    }
}
