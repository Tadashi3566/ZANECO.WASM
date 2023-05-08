namespace ZANECO.WASM.Client.Infrastructure.Common;

public static class DateTimeFunctions
{
    public static int Years(in DateTime startDate, in DateTime endDate)
    {
        int yearsPassed = endDate.Year - startDate.Year;

        // Are we before the birth date this year? If so subtract one year from the mix
        if (endDate.Month < startDate.Month || (endDate.Month == startDate.Month && endDate.Day < startDate.Day))
        {
            yearsPassed--;
        }

        return yearsPassed;
    }

    public static int Months(this in DateTime startDate, in DateTime endDate)
    {
        DateTime earlyDate = (startDate > endDate) ? endDate.Date : startDate.Date;
        DateTime lateDate = (startDate > endDate) ? startDate.Date : endDate.Date;

        // Start with 1 month's difference and keep incrementing
        // until we overshoot the late date
        int monthsDiff = 1;
        while (earlyDate.AddMonths(monthsDiff) <= lateDate)
        {
            monthsDiff++;
        }

        return monthsDiff - 1;
    }
}