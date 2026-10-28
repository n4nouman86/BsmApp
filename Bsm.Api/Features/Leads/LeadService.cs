using Bsm.Api.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bsm.Api.Features.Leads;

public interface ILeadService
{
    Task<IReadOnlyList<LeadResponse>> GetAllAsync(string userId, bool isAdmin, CancellationToken cancellationToken);
    Task<LeadResponse?> GetByIdAsync(int id, string userId, bool isAdmin, CancellationToken cancellationToken);
    Task<LeadResponse> CreateAsync(SaveLeadRequest request, string userId, CancellationToken cancellationToken);
    Task<LeadResponse?> UpdateAsync(int id, SaveLeadRequest request, string userId, bool isAdmin, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, string userId, bool isAdmin, CancellationToken cancellationToken);
}

public sealed class LeadService(ApplicationDbContext db) : ILeadService
{
    private IQueryable<Lead> VisibleLeads(string userId, bool isAdmin) =>
        db.Leads.Where(lead => isAdmin || lead.AppUserId == userId);

    public async Task<IReadOnlyList<LeadResponse>> GetAllAsync(string userId, bool isAdmin, CancellationToken cancellationToken)
    {
        var leads = await VisibleLeads(userId, isAdmin).AsNoTracking()
            .OrderByDescending(lead => lead.CreatedOn).ThenByDescending(lead => lead.Id)
            .ToListAsync(cancellationToken);
        return leads.Select(ToResponse).ToArray();
    }

    public async Task<LeadResponse?> GetByIdAsync(int id, string userId, bool isAdmin, CancellationToken cancellationToken)
    {
        var lead = await VisibleLeads(userId, isAdmin).AsNoTracking()
            .SingleOrDefaultAsync(lead => lead.Id == id, cancellationToken);
        return lead is null ? null : ToResponse(lead);
    }

    public async Task<LeadResponse> CreateAsync(SaveLeadRequest request, string userId, CancellationToken cancellationToken)
    {
        var lead = Apply(request);
        lead.AppUserId = userId;
        var now = DateTime.UtcNow;
        lead.CreatedOn = new DateTime(now.Ticks - now.Ticks % TimeSpan.TicksPerMillisecond, DateTimeKind.Utc);
        db.Leads.Add(lead);
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(lead);
    }

    public async Task<LeadResponse?> UpdateAsync(int id, SaveLeadRequest request, string userId, bool isAdmin, CancellationToken cancellationToken)
    {
        var lead = await VisibleLeads(userId, isAdmin).SingleOrDefaultAsync(lead => lead.Id == id, cancellationToken);
        if (lead is null) return null;

        var values = Apply(request);
        values.Id = lead.Id;
        values.AppUserId = lead.AppUserId;
        values.CreatedOn = lead.CreatedOn;
        db.Entry(lead).CurrentValues.SetValues(values);
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(lead);
    }

    public async Task<bool> DeleteAsync(int id, string userId, bool isAdmin, CancellationToken cancellationToken)
    {
        var lead = await VisibleLeads(userId, isAdmin).SingleOrDefaultAsync(lead => lead.Id == id, cancellationToken);
        if (lead is null) return false;
        db.Leads.Remove(lead);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static Lead Apply(SaveLeadRequest request) => new()
    {
        PhoneNumber = request.PhoneNumber,
        FirstName = request.FirstName,
        LastName = request.LastName,
        Email = request.Email,
        DateOfBirth = request.DateOfBirth!.Value,
        StreetAddress = request.StreetAddress,
        City = request.City,
        State = request.State,
        PostalCode = request.PostalCode,
        HaveSpouse = request.HaveSpouse,
        SpouseName = request.SpouseName,
        SpouseDateOfBirth = request.SpouseDateOfBirth,
        CurrentAutoCarrier = request.CurrentAutoCarrier,
        AnyLaspse = request.AnyLaspse,
        MakeAndModel = request.MakeAndModel,
        Accidents = request.Accidents,
        Tikets = request.Tikets,
        HomeOwner = request.HomeOwner,
        HopeType = request.HopeType,
        HaveHomeClaims = request.HaveHomeClaims,
        CurrentHomeCarrier = request.CurrentHomeCarrier,
        AnyClaimInPast3Years = request.AnyClaimInPast3Years,
    };

    private static LeadResponse ToResponse(Lead lead) => new(
        lead.Id, lead.AppUserId, lead.PhoneNumber, lead.FirstName, lead.LastName, lead.Email,
        lead.DateOfBirth, lead.StreetAddress, lead.City, lead.State, lead.PostalCode, lead.HaveSpouse,
        lead.SpouseName, lead.SpouseDateOfBirth, lead.CurrentAutoCarrier, lead.AnyLaspse, lead.MakeAndModel,
        lead.Accidents, lead.Tikets, lead.HomeOwner, lead.HopeType, lead.HaveHomeClaims,
        lead.CurrentHomeCarrier, lead.AnyClaimInPast3Years,
        new DateTimeOffset(DateTime.SpecifyKind(lead.CreatedOn, DateTimeKind.Utc)));
}
