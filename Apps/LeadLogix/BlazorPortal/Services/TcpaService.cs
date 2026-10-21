using BlazorPortal.Models;
using System.Text;
using System.Text.Json;

namespace BlazorPortal.Services;

public class TcpaService(IConfiguration configuration)
{
    public async Task<string> GetDncStatus(string phoneNumber)
    {
        phoneNumber = phoneNumber.Trim();

        string apiEndpoint = "https://api.tcpalitigatorlist.com/scrub/phone/";
        string apiUsername = configuration["Tcpa:Username"] ?? throw new InvalidOperationException("Tcpa:Username is not configured.");
        string apiPassword = configuration["Tcpa:Password"] ?? throw new InvalidOperationException("Tcpa:Password is not configured.");

        var postData = new Dictionary<string, string>
        {
            { "type", JsonSerializer.Serialize(new[] { "tcpa", "dnc_complainers" }) },
            { "phone_number", phoneNumber }
        };

        using (var client = new HttpClient())
        {
            var byteArray = Encoding.ASCII.GetBytes($"{apiUsername}:{apiPassword}");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            var content = new FormUrlEncodedContent(postData);

            try
            {
                HttpResponseMessage response = await client.PostAsync(apiEndpoint, content);

				string responseBody = await response.Content.ReadAsStringAsync();       

                var dncModel = JsonSerializer.Deserialize<TcpaModel>(responseBody);

                return string.IsNullOrWhiteSpace(dncModel?.results.status) ? "clean" : dncModel.results.status;
            }
            catch (HttpRequestException e)
            {
                return $"Request error: {e.Message}";
            }
        }
    }
}
