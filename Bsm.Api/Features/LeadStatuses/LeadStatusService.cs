using Bsm.Api.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bsm.Api.Features.LeadStatuses;

public interface ILeadStatusService
{
    Task<IReadOnlyList<LeadStatusResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<LeadStatusResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<LeadStatusResponse?> CreateAsync(SaveLeadStatusRequest request, CancellationToken cancellationToken);
    Task<UpdateStatusResult> UpdateAsync(int id, SaveLeadStatusRequest request, CancellationToken cancellationToken);
    Task<DeleteStatusResult> DeleteAsync(int id, CancellationToken cancellationToken);
}

public enum UpdateStatusResult { Updated, NotFound, Duplicate }
public enum DeleteStatusResult { Deleted, NotFound, InUse }

public sealed class LeadStatusService(ApplicationDbContext db) : ILeadStatusService
{
    public async Task<IReadOnlyList<LeadStatusResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var statuses = await db.LeadStatuses.AsNoTracking()
            .OrderBy(status => status.Name).ThenBy(status => status.Id)
            .ToListAsync(cancellationToken);
        return statuses.Select(ToResponse).ToArray();
    }

    public async Task<LeadStatusResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var status = await db.LeadStatuses.AsNoTracking().SingleOrDefaultAsync(status => status.Id == id, cancellationToken);
        return status is null ? null : ToResponse(status);
    }

    public async Task<LeadStatusResponse?> CreateAsync(SaveLeadStatusRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        if (await db.LeadStatuses.AnyAsync(status => status.Name == name, cancellationToken)) return null;

        var now = DateTime.UtcNow;
        var status = new LeadStatus
        {
            Name = name,
            CreatedOn = new DateTime(now.Ticks - now.Ticks % TimeSpan.TicksPerMillisecond, DateTimeKind.Utc),
        };
        db.LeadStatuses.Add(status);
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(status);
    }

    public async Task<UpdateStatusResult> UpdateAsync(int id, SaveLeadStatusRequest request, CancellationToken cancellationToken)
    {
        var status = await db.LeadStatuses.SingleOrDefaultAsync(status => status.Id == id, cancellationToken);
        if (status is null) return UpdateStatusResult.NotFound;

        var name = request.Name.Trim();
        if (await db.LeadStatuses.AnyAsync(other => other.Id != id && other.Name == name, cancellationToken))
            return UpdateStatusResult.Duplicate;

        status.Name = name;
        await db.SaveChangesAsync(cancellationToken);
        return UpdateStatusResult.Updated;
    }

    public async Task<DeleteStatusResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var status = await db.LeadStatuses.SingleOrDefaultAsync(status => status.Id == id, cancellationToken);
        if (status is null) return DeleteStatusResult.NotFound;
        if (await db.Leads.AnyAsync(lead => lead.StatusId == id, cancellationToken)) return DeleteStatusResult.InUse;
        db.LeadStatuses.Remove(status);
        await db.SaveChangesAsync(cancellationToken);
        return DeleteStatusResult.Deleted;
    }

    private static LeadStatusResponse ToResponse(LeadStatus status) =>
        new(status.Id, status.Name, new DateTimeOffset(DateTime.SpecifyKind(status.CreatedOn, DateTimeKind.Utc)));
}
