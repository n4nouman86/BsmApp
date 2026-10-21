namespace BlazorPortal.Models
{
    public class OscarLeadModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address1 { get; set; } = string.Empty;
        public string Address2 { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Zip { get; set; } = string.Empty;
        public DateTime? DOB { get; set; }
    }


    public class OscarResponseModel
    {
        public bool Status { get; set; }
        public int Lead_Id { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
