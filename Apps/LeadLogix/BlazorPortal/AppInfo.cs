namespace BlazorPortal
{
    public static class AppInfo
    {
        public static string AppName { get; } = "Lead Logix";
        public static string Website { get; } = "https://leadlogix.net";
        public static string Logo { get; } = "logo/logo.png";
        public static List<int> InvoiceBankIds { get; set; } = new List<int> { 1, 2 };
    }
}
