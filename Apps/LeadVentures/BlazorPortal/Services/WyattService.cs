using BlazorPortal.Entities;
using BlazorPortal.Models;

namespace BlazorPortal.Services;

public class WyattService
{
    public async Task<bool> CreateLeadAsync(Lead model, string? state)
    {
        string apiEndpoint = "https://allstate-leadsapp.ricochet.me/api/v1/lead/create/LeadVentures?token=1f5e027461a0ab7e95fe2ca43ac7d78c";

        var postModel = new OscarLeadModel
        {
            FirstName = model.FullName,
            LastName = "",
            Phone = model.PhoneNumber,
            Address1 = model.Address,
            Address2 = $"DOB: {model.DateOfBirth}",
            City = "",
            State = state ?? "",
            Zip = "",
            DOB = null
        };

        using (var client = new HttpClient())
        {
            try
            {
                var clientResponse = await client.PostAsJsonAsync(apiEndpoint, postModel);
                var res = await clientResponse.Content.ReadFromJsonAsync<OscarResponseModel>();

                if (res is null)
                {
                    return false;
                }
                else
                {
                    return res.Status;
                }
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }
    }
}
