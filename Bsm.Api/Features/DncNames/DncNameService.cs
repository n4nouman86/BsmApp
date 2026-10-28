using Bsm.Api.Features.Tcpa;
using Bsm.Api.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bsm.Api.Features.DncNames;

public interface IDncNameService
{
    Task<IReadOnlyList<DncNameResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<DncNameResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<SaveDncNameResult> CreateAsync(SaveDncNameRequest request, CancellationToken cancellationToken);
    Task<SaveDncNameResult> UpdateAsync(int id, SaveDncNameRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}

public enum SaveDncNameOutcome { Saved, NotFound, InvalidPhone }

public sealed record SaveDncNameResult(SaveDncNameOutcome Outcome, DncNameResponse? Entry = null);

public sealed class DncNameService(ApplicationDbContext db, ITcpaService tcpa) : IDncNameService
{
    public async Task<IReadOnlyList<DncNameResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var entries = await db.DncNames.AsNoTracking()
            .OrderBy(entry => entry.LastName).ThenBy(entry => entry.FirstName).ThenBy(entry => entry.Id)
            .ToListAsync(cancellationToken);
        return entries.Select(ToResponse).ToArray();
    }

    public async Task<DncNameResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var entry = await db.DncNames.AsNoTracking().SingleOrDefaultAsync(entry => entry.Id == id, cancellationToken);
        return entry is null ? null : ToResponse(entry);
    }

    public async Task<SaveDncNameResult> CreateAsync(SaveDncNameRequest request, CancellationToken cancellationToken)
    {
        var entry = new DncName();
        if (!Apply(entry, request)) return new SaveDncNameResult(SaveDncNameOutcome.InvalidPhone);
        db.DncNames.Add(entry);
        await db.SaveChangesAsync(cancellationToken);
        return new SaveDncNameResult(SaveDncNameOutcome.Saved, ToResponse(entry));
    }

    public async Task<SaveDncNameResult> UpdateAsync(int id, SaveDncNameRequest request, CancellationToken cancellationToken)
    {
        var entry = await db.DncNames.SingleOrDefaultAsync(entry => entry.Id == id, cancellationToken);
        if (entry is null) return new SaveDncNameResult(SaveDncNameOutcome.NotFound);
        if (!Apply(entry, request)) return new SaveDncNameResult(SaveDncNameOutcome.InvalidPhone);
        await db.SaveChangesAsync(cancellationToken);
        return new SaveDncNameResult(SaveDncNameOutcome.Saved, ToResponse(entry));
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var entry = await db.DncNames.SingleOrDefaultAsync(entry => entry.Id == id, cancellationToken);
        if (entry is null) return false;
        db.DncNames.Remove(entry);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private bool Apply(DncName entry, SaveDncNameRequest request)
    {
        string? phone = null;
        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            phone = tcpa.Normalize(request.Phone);
            if (phone is null) return false;
        }

        entry.FirstName = request.FirstName.Trim();
        entry.LastName = request.LastName.Trim();
        entry.Phone = phone;
        entry.State = string.IsNullOrWhiteSpace(request.State) ? null : request.State.Trim().ToUpperInvariant();
        return true;
    }

    private static DncNameResponse ToResponse(DncName entry) => new(entry.Id, entry.FirstName, entry.LastName, entry.Phone, entry.State);
}
