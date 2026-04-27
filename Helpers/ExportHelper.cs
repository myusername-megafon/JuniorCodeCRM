    namespace JuniorCodeCRM.Helpers;

public static class ExportHelper
{
    public static string GetContentType(string format)
    {
        return format.ToLower() switch
        {
            "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }

    public static string GetFileName(string reportName, string format)
    {
        var date = DateTime.Now.ToString("yyyy-MM-dd");
        return $"{reportName}_{date}.{format.ToLower()}";
    }
}