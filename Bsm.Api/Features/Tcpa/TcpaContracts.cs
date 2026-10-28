namespace Bsm.Api.Features.Tcpa;

/// <summary>Result of scrubbing a phone number against the TCPA litigator list.</summary>
/// <param name="PhoneNumber">The normalized 10-digit phone number that was checked.</param>
/// <param name="IsClean">True when the number may be used.</param>
/// <param name="Status">"clean" or the flag returned by the provider.</param>
public sealed record TcpaCheckResponse(string PhoneNumber, bool IsClean, string Status);

/// <summary>Result of checking a phone number against the internal DNC numbers list.</summary>
public sealed record DncCheckResponse(string PhoneNumber, bool IsListed);
