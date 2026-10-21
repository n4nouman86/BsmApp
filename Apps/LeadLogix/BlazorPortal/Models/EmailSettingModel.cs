namespace BlazorPortal.Models
{
    public class EmailSettingModel
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string EmailFromName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;

    }
}
