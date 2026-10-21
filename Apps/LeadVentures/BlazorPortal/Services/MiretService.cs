using BlazorPortal.Entities;
using BlazorPortal.Models;

namespace BlazorPortal.Services
{
    public class MiretService
    {
        public async Task<bool> CreateLeadAsync(Lead model, string? state)
        {
            string apiEndpoint = "https://allstate-leadsapp.ricochet.me/api/v1/lead/create/Lead-Ventures-Auto/?token=4730528c161e6df7404dddd540f0cd70&cid=Auto";

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
