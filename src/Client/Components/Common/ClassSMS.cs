namespace ZANECO.WASM.Client.Components.Common;

internal class ClassSMS
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
}