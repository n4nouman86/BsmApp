using BlazorPortal.Entities;
using BlazorPortal.Models;
using System.Text;
using System.Text.Json;

namespace BlazorPortal.Services
{
    public class OscarService
    {
        public async Task<bool> CreateLeadAsync(Lead model, string? state)
        {
            string apiEndpoint = "https://allstate-leadsapp.ricochet.me/api/v1/lead/create/Lead-Ventures-Live-Call-Transfer/?token=60d955b58a20fa376f532b7214cec054&cid=Auto&imported=1";

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
