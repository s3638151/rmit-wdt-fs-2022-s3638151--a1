using System.Data;
using Microsoft.Data.SqlClient;

namespace s3638151_a1.Utilities;

public static class MiscellaneousExtensionUtilities
{
    public static bool IsInRange(this int value, int min, int max) => value >= min && value <= max;

    public static DataTable GetDataTable(this SqlCommand command)
    {
        using var adapter = new SqlDataAdapter(command);

        var table = new DataTable();
        adapter.Fill(table);

        return table;
    }

    public static object GetObjectOrDbNull(this object value) => value ?? DBNull.Value;

    public static string TrimStartOnAllLines(this string item)
    {
        var lines = item.Split('\n');

        for(var i = 0; i < lines.Length; i++)
        {
            lines[i] = lines[i].TrimStart();
        }

        return string.Join('\n', lines);
    }
}
