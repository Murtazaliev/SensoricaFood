using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;

namespace AVBDelivery.Models
{
    public class TimeOnlyConverter : ValueConverter<TimeOnly, DateTime>
    {
        public TimeOnlyConverter() : base(
           timeOnly => new DateTime(DateOnly.MinValue.Year, DateOnly.MinValue.Month, DateOnly.MinValue.Day,
                timeOnly.Hour, timeOnly.Minute, timeOnly.Second),
           dateTime => TimeOnly.FromDateTime(dateTime))
        { }
    }
}
