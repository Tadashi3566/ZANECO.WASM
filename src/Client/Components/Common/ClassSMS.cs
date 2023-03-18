namespace ZANECO.WASM.Client.Components.Common;

internal class ClassSms
{
    public static string RemoveWhiteSpaces(string input)
    {
        return new string(input.ToCharArray()
            .Where(c => !char.IsWhiteSpace(c))
            .ToArray());
    }

    public static string FormatContactNumber(string contactNumber)
    {
        contactNumber = contactNumber.Trim();

        return $"+639{contactNumber[^9..]}";
    }

    public static string[] GetDistinctFromArray(string[] array)
    {
        return array.Distinct().ToArray();
    }

    public static int GetDistinctRecipients(string Recipients)
    {
        Recipients = ClassSms.RemoveWhiteSpaces(Recipients);
        string[] RecipientArray = Recipients.Split(',');

        return ClassSms.GetDistinctFromArray(RecipientArray).Length;
    }

    public static int RecipientCount(string Recipients)
    {
        return RemoveWhiteSpaces(Recipients).Split(',').Length;
    }
}