namespace BlazorPortal
{
    public static class AppInfo
    {
        public static string AppName { get; } = "Lead Ventures";
        public static string Website { get; } = "https://leadventures.net";
        public static string Logo { get; } = "logo/logo.png";
        public static List<int> InvoiceBankIds { get; set; } = new List<int> { 1 };



        public static int AppVersion { get; set; }
    }
}
