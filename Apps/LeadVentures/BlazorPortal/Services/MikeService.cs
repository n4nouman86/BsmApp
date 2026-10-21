using BlazorPortal.Entities;
using BlazorPortal.Models;

namespace BlazorPortal.Services;

public class MikeService
{
    public async Task<bool> CreateLeadAsync(Lead model, string? state)
    {
        string apiEndpoint = "https://allstate-mt2-leadsapp.ricochet.me/api/v1/lead/create/Lead-Ventures-Live-Call-Transfer/?token=36d8bb2776fe025b64dd20d1af0d59e1&cid=Auto&imported=1";

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
