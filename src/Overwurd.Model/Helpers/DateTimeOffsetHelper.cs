using System;

namespace Overwurd.Model.Helpers;

public static class DateTimeOffsetHelper
{
    public static DateTimeOffset TrimSeconds(this DateTimeOffset dateTimeOffset) =>
        new(year:   dateTimeOffset.Year,
            month:  dateTimeOffset.Month,
            day:    dateTimeOffset.Day,
            hour:   dateTimeOffset.Hour,
            minute: dateTimeOffset.Minute,
            second: dateTimeOffset.Second,
            offset: dateTimeOffset.Offset);

    public static DateTimeOffset NowUtcSeconds() =>
        DateTimeOffset.UtcNow.TrimSeconds();
}