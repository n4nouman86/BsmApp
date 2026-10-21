using BlazorPortal.Entities;
using BlazorPortal.Models;

namespace BlazorPortal.Services;

public class StefanSmallwoodService
{
	public async Task<bool> CreateLeadAsync(Lead model, string? state)
	{
		string apiEndpoint = "https://app.agencyzoom.com/v1/api/lead/webhook/6a1f2754037eeb9fc3c187ec617f523d64ae61ad436d5";

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
