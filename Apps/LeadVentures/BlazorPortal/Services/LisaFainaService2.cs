using BlazorPortal.Entities;
using BlazorPortal.Models;

namespace BlazorPortal.Services;

public class LisaFainaService2
{
	public async Task<bool> CreateLeadAsync(Lead model, string? state)
	{
		string apiEndpoint = "https://leads.ricochet.me/api/v1/lead/create/Lead-Ventures-Multi?token=f0aa0378a932aab3cd13610306b7aa5d";

		var postModel = new OscarLeadModel
		{
			FirstName = model.FullName,
			LastName = "",
			Phone = model.PhoneNumber,
			Address1 = model.Address,
			Address2 = $"DOB: {model.DateOfBirth} Make and Model: {model.MakeAndModel}",
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
