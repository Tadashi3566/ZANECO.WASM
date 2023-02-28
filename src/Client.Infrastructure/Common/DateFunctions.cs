namespace ZANECO.WASM.Client.Infrastructure.Common;

public static class DateFunctions
{
    public static int GetWorkingDays(DateTime startDate, DateTime endDate)
    {
        int workingDays = 0;
        int totalDays = (int)(endDate - startDate).TotalDays;

        for (int i = 0; i <= totalDays; i++)
        {
            DateTime currentDay = startDate.AddDays(i);
            if (currentDay.DayOfWeek != DayOfWeek.Saturday && currentDay.DayOfWeek != DayOfWeek.Sunday)
            {
                workingDays++;
            }
        }

        return workingDays;
    }
}
