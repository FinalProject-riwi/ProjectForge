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
        DatabaseType? db = null, InfrastructureType? infra = null)
    {
        var q = _set.Where(t => t.Architecture == arch && t.TemplateType == templateType && t.IsActive);
        if (db.HasValue) q = q.Where(t => t.Database == db || t.Database == null);
        if (infra.HasValue) q = q.Where(t => t.Infrastructure == infra || t.Infrastructure == null);
        return await q.OrderByDescending(t => t.Version).FirstOrDefaultAsync();
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
