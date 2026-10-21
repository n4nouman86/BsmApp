using BlazorPortal.Entities;
using BlazorPortal.Models;

namespace BlazorPortal.Services;

public class MarkMercerService
{
    public async Task<bool> CreateLeadAsync(Lead model, string? state)
    {
        string apiEndpoint = "https://allstate-mt3-leadsapp.ricochet.me/api/v1/lead/create/Lead-Venture?token=6eb0503b033b48573675981a4db5b4db";

        var postModel = new OscarLeadModel
        {
            FirstName = model.FullName,
            LastName = "",
            Phone = model.PhoneNumber,
            Address1 = model.Address,
            Address2 = $"DOB: {model.DateOfBirth}",
            City = "",
            State = state ?? "NA",
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
