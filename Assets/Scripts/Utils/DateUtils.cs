using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DateUtils
{
    public static string formatFromSeconds(string format, int seconds)
    {
        DateTime date = new DateTime();
        date = date.AddSeconds(seconds);
        return date.ToString(format);
    }

    public static bool isTimeBetween(DateTime datetime, TimeSpan start, TimeSpan end)
    {
        // convert datetime to a TimeSpan
        TimeSpan now = datetime.TimeOfDay;
        // see if start comes before end
        if (start < end)
            return start <= now && now <= end;
        // start is after end, so do the inverse comparison
        return !(end < now && now < start);
    }

    public static double currentTimeMillis()
    {
        DateTime Jan1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        TimeSpan javaSpan = DateTime.UtcNow - Jan1970;
        return javaSpan.TotalMilliseconds;
    }
}
