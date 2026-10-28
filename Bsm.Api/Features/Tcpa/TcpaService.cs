using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bsm.Api.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Bsm.Api.Features.Tcpa;

/// <summary>Settings for the TCPA litigator list scrub service.</summary>
public sealed class TcpaOptions
{
    public bool Enabled { get; set; } = true;
    public string Endpoint { get; set; } = "https://api.tcpalitigatorlist.com/scrub/phone/";
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>Outcome of a TCPA scrub. Status is "clean", "unavailable" or the flag returned by the provider.</summary>
public sealed record TcpaResult(bool IsClean, string Status);

public interface ITcpaService
{
    /// <summary>Normalizes a US phone number to 10 digits, or returns null when it is not valid.</summary>
    string? Normalize(string? phoneNumber);

    /// <summary>True when the number is on the internal DNC numbers list.</summary>
    Task<bool> IsOnDncListAsync(string normalizedPhone, CancellationToken cancellationToken);

    /// <summary>Scrubs against the external TCPA provider only.</summary>
    Task<TcpaResult> ScrubAsync(string normalizedPhone, CancellationToken cancellationToken);

    /// <summary>Internal DNC list first, then the TCPA provider.</summary>
    Task<TcpaResult> CheckAsync(string normalizedPhone, CancellationToken cancellationToken);
}

public sealed class TcpaService(HttpClient client, ApplicationDbContext db, IOptions<TcpaOptions> options, ILogger<TcpaService> logger) : ITcpaService
{
    private const string Clean = "clean";
    private const string DncList = "dnc list";
    private const string Unavailable = "unavailable";

    public string? Normalize(string? phoneNumber)
    {
        var digits = new string((phoneNumber ?? string.Empty).Where(char.IsAsciiDigit).ToArray());
        if (digits.Length == 11 && digits[0] == '1') digits = digits[1..];
        return digits.Length == 10 ? digits : null;
    }

    public async Task<bool> IsOnDncListAsync(string normalizedPhone, CancellationToken cancellationToken)
    {
        var withCountryCode = "1" + normalizedPhone;
        return await db.DncNumbers.AsNoTracking().AnyAsync(dnc => dnc.Phone == normalizedPhone || dnc.Phone == withCountryCode, cancellationToken);
    }

    public async Task<TcpaResult> CheckAsync(string normalizedPhone, CancellationToken cancellationToken)
    {
        // The internal DNC list is local and free, so it is checked before the paid TCPA lookup.
        if (await IsOnDncListAsync(normalizedPhone, cancellationToken)) return new TcpaResult(false, DncList);
        return await ScrubAsync(normalizedPhone, cancellationToken);
    }

    public async Task<TcpaResult> ScrubAsync(string normalizedPhone, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (!settings.Enabled) return new TcpaResult(true, Clean);

        if (string.IsNullOrWhiteSpace(settings.Username) || string.IsNullOrWhiteSpace(settings.Password))
        {
            logger.LogError("TCPA credentials are not configured.");
            return new TcpaResult(false, Unavailable);
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, settings.Endpoint)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["type"] = JsonSerializer.Serialize(new[] { "tcpa", "dnc_complainers" }),
                    ["phone_number"] = normalizedPhone,
                }),
            };
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{settings.Username}:{settings.Password}")));

            using var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("TCPA scrub returned HTTP {StatusCode}.", (int)response.StatusCode);
                return new TcpaResult(false, Unavailable);
            }

            var body = await response.Content.ReadFromJsonAsync<TcpaResponse>(cancellationToken);
            var status = body?.Results?.Status;
            return string.IsNullOrWhiteSpace(status) ? new TcpaResult(true, Clean) : new TcpaResult(false, status);
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException or TaskCanceledException
            && !cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "TCPA scrub failed.");
            return new TcpaResult(false, Unavailable);
        }
    }

    private sealed record TcpaResponse([property: JsonPropertyName("results")] TcpaResults? Results);

    private sealed record TcpaResults([property: JsonPropertyName("status")] string? Status);
}
