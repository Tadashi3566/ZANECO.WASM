namespace ZANECO.WASM.Client.Components.Common;

public class CadFunctions
{
    public static IEnumerable<string> BillMonths()
    {
        List<string> list = new();

        var dtStart = DateTime.Today.AddYears(-10);
        var dtEnd = DateTime.Today.AddYears(1);
        var dt = dtStart;
        while (dt < dtEnd)
        {
            list.Add(dt.ToString("MMyy"));
            dt = dt.AddMonths(1);
        }

        return list.ToArray();
    }
}
