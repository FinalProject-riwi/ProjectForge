using Microsoft.EntityFrameworkCore;
using ProjectForge.Core.Entities;
using ProjectForge.Core.Enums;
using ProjectForge.Core.Interfaces;
using ProjectForge.Infrastructure.Data;

namespace ProjectForge.Infrastructure.Repositories;

// ─── Generic Repository ───────────────────────────────────────────────────────

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _db;
    protected readonly DbSet<T> _set;

    public Repository(AppDbContext db)
    {
        _db = db;
        _set = db.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id) => await _set.FindAsync(id);
    public virtual async Task<IEnumerable<T>> GetAllAsync() => await _set.ToListAsync();
    public virtual async Task<T> AddAsync(T entity) { await _set.AddAsync(entity); await _db.SaveChangesAsync(); return entity; }
    public virtual async Task UpdateAsync(T entity) { _db.Entry(entity).State = EntityState.Modified; await _db.SaveChangesAsync(); }
    public virtual async Task DeleteAsync(int id) { var e = await GetByIdAsync(id); if (e != null) { _set.Remove(e); await _db.SaveChangesAsync(); } }
}

// ─── Project Repository ───────────────────────────────────────────────────────

public class ProjectRepository : Repository<Project>, IProjectRepository
{
    public ProjectRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<Project>> GetByUserIdAsync(int userId) =>
        await _set.Where(p => p.UserId == userId)
                  .Include(p => p.WizardConfig)
                  .OrderByDescending(p => p.CreatedAt)
                  .ToListAsync();

    public async Task<Project?> GetWithLogsAsync(int projectId) =>
        await _set.Include(p => p.Logs)
                  .FirstOrDefaultAsync(p => p.Id == projectId);

    public async Task<Project?> GetFullAsync(int projectId) =>
        await _set
            .Include(p => p.User)
            .Include(p => p.WizardConfig).ThenInclude(w => w.VpsCredentials)
            .Include(p => p.Logs)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == projectId);
}

// ─── Template Repository ──────────────────────────────────────────────────────

public class TemplateRepository : Repository<ProjectTemplate>, ITemplateRepository
{
    public TemplateRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<ProjectTemplate>> GetByArchitectureAsync(ArchitectureType arch) =>
        await _set.Where(t => t.Architecture == arch && t.IsActive).ToListAsync();

    public async Task<ProjectTemplate?> GetTemplateAsync(
        ArchitectureType arch, string templateType,
        DatabaseType? db = null, InfrastructureType? infra = null, FrameworkType? framework = null)
    {
        var q = _set.Where(t => t.Architecture == arch && t.TemplateType == templateType && t.IsActive);

        if (db.HasValue || infra.HasValue || framework.HasValue)
        {
            // Boost rows matching Database/Infrastructure/Framework, newest Version wins among
            // those — but several seeders insert rows that share Architecture+TemplateType+Database
            // and only differ by Framework (previously not filtered here at all), and some go
            // further and insert outright duplicate Framework+Database rows with different
            // Content. ThenByDescending(Id) doesn't resolve which duplicate is "correct" content,
            // but it makes the pick deterministic/reproducible instead of depending on the SQL
            // provider's physical row order.
            q = q
                .OrderByDescending(t => db.HasValue && t.Database == db)
                .ThenByDescending(t => infra.HasValue && t.Infrastructure == infra)
                .ThenByDescending(t => framework.HasValue && t.Framework == framework)
                .ThenByDescending(t => t.Version)
                .ThenByDescending(t => t.Id);

            return await q.FirstOrDefaultAsync();
        }

        return await q.OrderByDescending(t => t.Version).ThenByDescending(t => t.Id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ProjectTemplate>> GetInfraTemplatesAsync(
        InfrastructureType infra, DatabaseType db) =>
        await _set.Where(t => t.Infrastructure == infra && (t.Database == db || t.Database == null) && t.IsActive)
                  .ToListAsync();
}

// ─── Library Repository ───────────────────────────────────────────────────────

public class LibraryRepository : Repository<LibraryRecommendation>, ILibraryRepository
{
    public LibraryRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<LibraryRecommendation>> GetByArchitectureAndFrameworkAsync(
        ArchitectureType arch, FrameworkType framework) =>
        await _set.Where(l => l.Architecture == arch && (l.Framework == framework || l.Framework == null))
                  .OrderByDescending(l => l.PopularityScore)
                  .ToListAsync();
}

// ─── DesignPattern Repository ─────────────────────────────────────────────────

public class DesignPatternRepository : Repository<DesignPatternEntry>, IDesignPatternRepository
{
    public DesignPatternRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<DesignPatternEntry>> GetByArchitectureAsync(ArchitectureType arch) =>
        await _set.Where(d => d.Architecture == arch).ToListAsync();
}

// ─── AiSuggestionCache Repository ────────────────────────────────────────────

public class AiSuggestionCacheRepository : Repository<AiSuggestionCache>, IAiSuggestionCacheRepository
{
    public AiSuggestionCacheRepository(AppDbContext db) : base(db) { }

    public async Task<AiSuggestionCache?> GetByCacheKeyAsync(string cacheKey) =>
        await _set.FirstOrDefaultAsync(c => c.CacheKey == cacheKey && c.ExpiresAt > DateTime.UtcNow);
}
