using BlazorPortal.Entities;
using BlazorPortal.Models;

namespace BlazorPortal.Services;

public class JayDepoloService
{
	public async Task<bool> CreateLeadAsync(Lead model, string? state)
	{
		string apiEndpoint = "https://allstate-leadsapp.ricochet.me/api/v1/lead/create/Lead-Ventures-Live-Transfers/?token=b6a5313727055d31aca7c9a54dc95b7d&cid=Home&imported=1";

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
