using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BlazorPortal.PaymentMethods;

public class MastercardService
{
	private readonly HttpClient _http;
	private readonly IConfiguration configuration;




	public MastercardService(HttpClient http, IConfiguration configuration)
	{
		_http = http;
		this.configuration = configuration;
	}




	public async Task<string?> CreateSession(string orderId, string amount, string desc, string returnUrl)
	{
		var setting = configuration.GetSection("MastercardSetting").Get<MastercardSetting>();

		if (setting is null)
		{
			return null;
		}

		var auth = Convert.ToBase64String(
			Encoding.ASCII.GetBytes($"merchant.{setting.MerchantId}:{setting.Password}")
		);

		_http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);

		var body = new
		{
			apiOperation = "INITIATE_CHECKOUT",
			interaction = new
			{
				operation = "PURCHASE",
				returnUrl = returnUrl,
				merchant = new
				{
					name = setting.MerchantName
				}
			},
			order = new
			{
				currency = setting.Currency,
				amount = amount,
				id = orderId,
				description = desc
			}
		};

		var jsonBody = JsonSerializer.Serialize(body);

		var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

		var response = await _http.PostAsync(setting.ApiEndPoint, content);
		var json = await response.Content.ReadAsStringAsync();

		using var doc = JsonDocument.Parse(json);
		string? sessionId = null;

		try
		{
			sessionId = doc.RootElement.GetProperty("session").GetProperty("id").GetString();
		}
		catch (Exception)
		{
			sessionId = null;
		}

		return sessionId;
	}




	public async Task<string?> VerifyPayment(string orderId)
	{
		var setting = configuration.GetSection("MastercardSetting").Get<MastercardSetting>();

		if (setting is null)
		{
			return "snf";
		}

		var auth = Convert.ToBase64String(
			Encoding.ASCII.GetBytes($"merchant.{setting.MerchantId}:{setting.Password}")
		);

		_http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);

		var response = await _http.GetAsync($"{setting.VerifyApiEndPoint}/{orderId}");
		var json = await response.Content.ReadAsStringAsync();
		using var doc = JsonDocument.Parse(json);

		try
		{
			var status = doc.RootElement.GetProperty("result").GetString();
			return status;
		}
		catch (Exception)
		{
			return "error";
		}
	}

}




public class MastercardSetting
{
	public string MerchantId { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;
	public string ApiEndPoint { get; set; } = string.Empty;
	public string MerchantName { get; set; } = string.Empty;
	public string Currency { get; set; } = string.Empty;
	public string VerifyApiEndPoint { get; set; } = string.Empty;

}