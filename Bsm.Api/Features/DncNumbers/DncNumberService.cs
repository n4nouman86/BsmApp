using Bsm.Api.Features.Tcpa;
using Bsm.Api.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bsm.Api.Features.DncNumbers;

public interface IDncNumberService
{
    Task<IReadOnlyList<DncNumberResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<DncNumberResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<SaveDncResult> CreateAsync(SaveDncNumberRequest request, CancellationToken cancellationToken);
    Task<SaveDncResult> UpdateAsync(int id, SaveDncNumberRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}

public enum SaveDncOutcome { Saved, NotFound, InvalidPhone, Duplicate }

public sealed record SaveDncResult(SaveDncOutcome Outcome, DncNumberResponse? Entry = null);

public sealed class DncNumberService(ApplicationDbContext db, ITcpaService tcpa) : IDncNumberService
{
    public async Task<IReadOnlyList<DncNumberResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var entries = await db.DncNumbers.AsNoTracking()
            .OrderByDescending(entry => entry.CreatedOn).ThenByDescending(entry => entry.Id)
            .ToListAsync(cancellationToken);
        return entries.Select(ToResponse).ToArray();
    }

    public async Task<DncNumberResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var entry = await db.DncNumbers.AsNoTracking().SingleOrDefaultAsync(entry => entry.Id == id, cancellationToken);
        return entry is null ? null : ToResponse(entry);
    }

    public async Task<SaveDncResult> CreateAsync(SaveDncNumberRequest request, CancellationToken cancellationToken)
    {
        var phone = tcpa.Normalize(request.Phone);
        if (phone is null) return new SaveDncResult(SaveDncOutcome.InvalidPhone);
        if (await db.DncNumbers.AnyAsync(entry => entry.Phone == phone, cancellationToken)) return new SaveDncResult(SaveDncOutcome.Duplicate);

        var now = DateTime.UtcNow;
        var entry = new DncNumber
        {
            Phone = phone,
            CreatedOn = new DateTime(now.Ticks - now.Ticks % TimeSpan.TicksPerMillisecond, DateTimeKind.Utc),
        };
        Apply(entry, request);
        db.DncNumbers.Add(entry);
        await db.SaveChangesAsync(cancellationToken);
        return new SaveDncResult(SaveDncOutcome.Saved, ToResponse(entry));
    }

    public async Task<SaveDncResult> UpdateAsync(int id, SaveDncNumberRequest request, CancellationToken cancellationToken)
    {
        var entry = await db.DncNumbers.SingleOrDefaultAsync(entry => entry.Id == id, cancellationToken);
        if (entry is null) return new SaveDncResult(SaveDncOutcome.NotFound);

        var phone = tcpa.Normalize(request.Phone);
        if (phone is null) return new SaveDncResult(SaveDncOutcome.InvalidPhone);
        if (await db.DncNumbers.AnyAsync(other => other.Id != id && other.Phone == phone, cancellationToken)) return new SaveDncResult(SaveDncOutcome.Duplicate);

        entry.Phone = phone;
        Apply(entry, request);
        await db.SaveChangesAsync(cancellationToken);
        return new SaveDncResult(SaveDncOutcome.Saved, ToResponse(entry));
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var entry = await db.DncNumbers.SingleOrDefaultAsync(entry => entry.Id == id, cancellationToken);
        if (entry is null) return false;
        db.DncNumbers.Remove(entry);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static void Apply(DncNumber entry, SaveDncNumberRequest request)
    {
        entry.FirstName = Clean(request.FirstName);
        entry.LastName = Clean(request.LastName);
        entry.State = Clean(request.State)?.ToUpperInvariant();
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static DncNumberResponse ToResponse(DncNumber entry) =>
        new(entry.Id, entry.Phone, entry.FirstName, entry.LastName, entry.State, new DateTimeOffset(DateTime.SpecifyKind(entry.CreatedOn, DateTimeKind.Utc)));
}
