using BlazorPortal.Entities;
using BlazorPortal.Models;

namespace BlazorPortal.Services
{
    public class EstersonService
    {
        public async Task<bool> CreateLeadAsync(Lead model, string? state)
        {
            string apiEndpoint = "https://allstate-mt2-leadsapp.ricochet.me/api/v1/lead/create/Lead-Venture-Live-Call-Transfer/?token=b63ce5c682ec0b2e9cd5960c07a68c48&cid=Auto&imported=1";

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
}
