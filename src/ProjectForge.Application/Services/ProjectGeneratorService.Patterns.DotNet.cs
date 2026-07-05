using ProjectForge.Core.Enums;

namespace ProjectForge.Application.Services;

public partial class ProjectGeneratorService
{
    internal static IReadOnlyList<(string RelativePath, string Content)> BuildDotNetPatternFiles(
        FrameworkType framework, DatabaseType database, string pattern)
    {
        return NormalizePatternToken(pattern) switch
        {
            "repository" => BuildDotNetRepositoryPatternFiles(framework, database),
            // Self-contained: previously referenced an "IItemRepository"/"Item" that only ever
            // existed if the "Repository" pattern was ALSO picked — but the wizard only lets you
            // select one design pattern per project, so CQRS needs its own copy of both.
            "cqrs" => new[]
            {
                ("src/Domain/Item.cs", """
namespace Domain;

public class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
"""),
                ("src/Domain/IItemRepository.cs", """
namespace Domain;

public interface IItemRepository
{
    Task AddAsync(Item item, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
"""),
                ("src/Infrastructure/InMemoryItemRepository.cs", """
using Domain;

namespace Infrastructure;

// In-memory placeholder — swap in the real EF Core/Mongo/Redis repository this project was
// generated with (see the "Repository" pattern for a worked example) once you need persistence.
public class InMemoryItemRepository : IItemRepository
{
    private static readonly List<Item> _items = [];
    private static int _nextId = 1;

    public Task AddAsync(Item item, CancellationToken ct = default)
    {
        item.Id = _nextId++;
        _items.Add(item);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => Task.CompletedTask;
}
"""),
                ("src/Application/Commands/CreateItemCommand.cs", """
using MediatR;

namespace Application.Commands;

public record CreateItemCommand(string Name, string Description) : IRequest<int>;
"""),
                ("src/Application/Handlers/CreateItemCommandHandler.cs", """
using Application.Commands;
using Domain;
using MediatR;

namespace Application.Handlers;

public class CreateItemCommandHandler(IItemRepository repo) : IRequestHandler<CreateItemCommand, int>
{
    public async Task<int> Handle(CreateItemCommand request, CancellationToken ct)
    {
        var item = new Item { Name = request.Name, Description = request.Description };
        await repo.AddAsync(item, ct);
        await repo.SaveChangesAsync(ct);
        return item.Id;
    }
}
"""),
                ("src/Application/Queries/GetAllItemsQuery.cs", """
using MediatR;

namespace Application.Queries;

public record GetAllItemsQuery : IRequest<IEnumerable<ItemDto>>;
public record ItemDto(int Id, string Name, string Description);
"""),
            },
            "cleanarchitecture" => new[]
            {
                ("src/Domain/Entities/BaseEntity.cs", """
namespace Domain.Entities;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
"""),
                ("src/Domain/Interfaces/IUnitOfWork.cs", """
namespace Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    Task<int> CommitAsync(CancellationToken ct = default);
}
"""),
                ("src/Application/Common/Result.cs", """
namespace Application.Common;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    private Result(bool success, T? value, string? error) { IsSuccess = success; Value = value; Error = error; }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);
}
"""),
            },
            "hexagonalarchitecture" => BuildDotNetHexagonalPatternFiles(framework, database),
            "domaindrivendesign" => new[]
            {
                ("src/Domain/Aggregates/AggregateRoot.cs", """
namespace Domain.Aggregates;

public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _events = [];
    public IReadOnlyList<IDomainEvent> DomainEvents => _events.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent @event) => _events.Add(@event);
    public void ClearDomainEvents() => _events.Clear();
}

public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}
"""),
                ("src/Domain/ValueObjects/ValueObject.cs", """
namespace Domain.ValueObjects;

public abstract class ValueObject
{
    protected abstract IEnumerable<object?> GetEqualityComponents();
    public override bool Equals(object? obj) => obj is ValueObject other && GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    public override int GetHashCode() => GetEqualityComponents().Aggregate(0, HashCode.Combine);
}
"""),
            },
            "eventsourcing" => new[]
            {
                ("src/Domain/Events/IDomainEvent.cs", """
namespace Domain.Events;

public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredAt { get; }
    string AggregateId { get; }
}
"""),
                ("src/Infrastructure/EventStore/IEventStore.cs", """
using Domain.Events;

namespace Infrastructure.EventStore;

public interface IEventStore
{
    Task AppendAsync(string aggregateId, IEnumerable<IDomainEvent> events, CancellationToken ct = default);
    Task<IEnumerable<IDomainEvent>> LoadAsync(string aggregateId, CancellationToken ct = default);
}
"""),
            },
            "mediator" => new[]
            {
                ("src/Application/Behaviors/LoggingBehavior.cs", """
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        logger.LogInformation("Handling {Name}: {@Request}", typeof(TRequest).Name, request);
        var response = await next();
        logger.LogInformation("Handled {Name}: {@Response}", typeof(TRequest).Name, response);
        return response;
    }
}
"""),
            },
            // Dropping a second top-level-statements gateway/Program.cs directly into the main
            // project used to break the build for EVERY DotNet framework: the SDK-style .csproj
            // globs **/*.cs by default, so both Program.cs files landed in the same compilation
            // -> CS8802 "Only one compilation unit can have top-level statements". Giving the
            // gateway its own gateway.csproj fixes this two ways at once: the .NET SDK
            // automatically excludes any subfolder that contains its own project file from the
            // parent's default item glob, AND the Yarp.ReverseProxy package (referenced by
            // AddReverseProxy() below but never installed anywhere before) now has somewhere to
            // live without needing a NuGet-package pass over the main project too.
            "microservices" => new[]
            {
                ("gateway/gateway.csproj", """
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Yarp.ReverseProxy" Version="2.2.0" />
  </ItemGroup>

</Project>
"""),
                ("gateway/Program.cs", """
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
var app = builder.Build();
app.MapReverseProxy();
app.Run();
"""),
                ("gateway/appsettings.json", """
{
  "ReverseProxy": {
    "Routes": {
      "orders-route": {
        "ClusterId": "orders",
        "Match": { "Path": "/api/orders/{**catch-all}" }
      }
    },
    "Clusters": {
      "orders": {
        "Destinations": {
          "orders/destination1": { "Address": "http://orders-service:8080/" }
        }
      }
    }
  }
}
"""),
            },
            // System.Windows.Input.ICommand needs a desktop UI SDK (WPF/WinForms) — it isn't
            // referenced by ASP.NET Core Web API/MVC/Minimal API or Blazor projects, so the
            // previous RelayCommand.cs failed to compile in every DotNet framework this wizard
            // actually offers. This defines its own ICommand-shaped interface instead, which
            // works the same way from a Blazor component's @onclick without any extra package.
            "mvvm" => new[]
            {
                ("src/Presentation/ViewModels/ItemViewModel.cs", """
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Presentation.ViewModels;

public class ItemViewModel : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private string _description = string.Empty;

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    public string Description
    {
        get => _description;
        set { _description = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
"""),
                ("src/Presentation/Commands/IRelayCommand.cs", """
namespace Presentation.Commands;

// Framework-agnostic stand-in for System.Windows.Input.ICommand, which requires a desktop
// UI SDK (WPF/WinForms) that ASP.NET Core / Blazor projects don't reference.
public interface IRelayCommand
{
    bool CanExecute(object? parameter);
    void Execute(object? parameter);
}
"""),
                ("src/Presentation/Commands/RelayCommand.cs", """
namespace Presentation.Commands;

public class RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null) : IRelayCommand
{
    public bool CanExecute(object? parameter) => canExecute?.Invoke(parameter) ?? true;
    public void Execute(object? parameter) => execute(parameter);
}
"""),
            },
            "saga" => new[]
            {
                ("src/Application/Sagas/OrderSaga.cs", """
using MediatR;

namespace Application.Sagas;

/// <summary>
/// Saga that coordinates the order creation workflow.
/// Each step compensates on failure to maintain consistency.
/// </summary>
public class OrderSaga(IMediator mediator)
{
    public async Task<bool> ExecuteAsync(CreateOrderSagaRequest request, CancellationToken ct = default)
    {
        var orderId = Guid.NewGuid();
        var reservationId = default(Guid?);

        try
        {
            // Step 1: Create order
            await mediator.Send(new CreateOrderCommand(orderId, request.CustomerId, request.Items), ct);

            // Step 2: Reserve inventory
            reservationId = await mediator.Send(new ReserveInventoryCommand(orderId, request.Items), ct);

            // Step 3: Process payment
            await mediator.Send(new ProcessPaymentCommand(orderId, request.PaymentInfo), ct);

            // Step 4: Confirm order
            await mediator.Send(new ConfirmOrderCommand(orderId), ct);

            return true;
        }
        catch
        {
            // Compensation: rollback in reverse order
            if (reservationId.HasValue)
                await mediator.Send(new ReleaseInventoryCommand(reservationId.Value), ct);

            await mediator.Send(new CancelOrderCommand(orderId), ct);
            return false;
        }
    }
}

public record CreateOrderSagaRequest(Guid CustomerId, IEnumerable<object> Items, object PaymentInfo);
public record CreateOrderCommand(Guid OrderId, Guid CustomerId, IEnumerable<object> Items) : IRequest;
public record ReserveInventoryCommand(Guid OrderId, IEnumerable<object> Items) : IRequest<Guid>;
public record ProcessPaymentCommand(Guid OrderId, object PaymentInfo) : IRequest;
public record ConfirmOrderCommand(Guid OrderId) : IRequest;
public record CancelOrderCommand(Guid OrderId) : IRequest;
public record ReleaseInventoryCommand(Guid ReservationId) : IRequest;
"""),
            },
            _ => Array.Empty<(string, string)>()
        };
    }

    // The Repository/Hexagonal pattern files used to hardcode EF Core (DbContext/AppDbContext)
    // regardless of the selected database or framework. That broke two ways:
    //  - MongoDB/Redis: Microsoft.EntityFrameworkCore is never referenced for those databases
    //    (see GetImplicitLibraries), so "using Microsoft.EntityFrameworkCore;" failed to resolve.
    //  - BlazorWasm: runs entirely in the browser sandbox with no raw socket access, so it can
    //    never open an EF Core/Npgsql/SqlClient/Mongo/Redis connection no matter which database
    //    was picked — data access has to go through an HTTP API instead.
    internal static IReadOnlyList<(string RelativePath, string Content)> BuildDotNetRepositoryPatternFiles(
        FrameworkType framework, DatabaseType database)
    {
        if (framework == FrameworkType.BlazorWasm)
            return BuildDotNetHttpRepositoryFiles();

        return database switch
        {
            DatabaseType.MongoDB => BuildDotNetMongoRepositoryFiles(),
            DatabaseType.Redis => BuildDotNetRedisRepositoryFiles(),
            _ => new[]
            {
                ("src/Domain/Interfaces/IRepository.cs", """
using System.Linq.Expressions;

namespace Domain.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
"""),
                ("src/Infrastructure/Repositories/BaseRepository.cs", """
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repositories;

public class BaseRepository<T>(DbContext context) : IRepository<T> where T : class
{
    protected readonly DbContext _context = context;
    protected readonly DbSet<T> _set = context.Set<T>();

    public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default) => await _set.FindAsync([id], ct);
    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default) => await _set.ToListAsync(ct);
    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) => await _set.Where(predicate).ToListAsync(ct);
    public async Task AddAsync(T entity, CancellationToken ct = default) => await _set.AddAsync(entity, ct);
    public void Update(T entity) => _set.Update(entity);
    public void Remove(T entity) => _set.Remove(entity);
    public async Task<int> SaveChangesAsync(CancellationToken ct = default) => await _context.SaveChangesAsync(ct);
}
"""),
            }
        };
    }

    internal static IReadOnlyList<(string RelativePath, string Content)> BuildDotNetHexagonalPatternFiles(
        FrameworkType framework, DatabaseType database)
    {
        if (framework == FrameworkType.BlazorWasm)
            return BuildDotNetHttpRepositoryFiles();

        return database switch
        {
            DatabaseType.MongoDB => BuildDotNetMongoRepositoryFiles(),
            DatabaseType.Redis => BuildDotNetRedisRepositoryFiles(),
            _ => new[]
            {
                ("src/Core/Ports/IItemPort.cs", """
using Core.Domain;

namespace Core.Ports;

public interface IItemPort
{
    Task<Item?> FindByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Item>> FindAllAsync(CancellationToken ct = default);
    Task SaveAsync(Item item, CancellationToken ct = default);
}
"""),
                ("src/Core/Domain/Item.cs", """
namespace Core.Domain;

public class Item
{
    public int Id { get; init; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public static Item Create(string name, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Item { Name = name, Description = description };
    }

    public void Update(string name, string description) { Name = name; Description = description; }
}
"""),
                ("src/Infrastructure/Adapters/ItemEfAdapter.cs", """
using Core.Ports;
using Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters;

// Depends on the generic EF Core DbContext (registered in Program.cs as AppDbContext) rather
// than AppDbContext itself — AppDbContext lives in "{ProjectNamespace}.Data", a namespace this
// pattern generator can't know ahead of time, and even a fixed "using" wouldn't help since
// AppDbContext.cs only ever declares a commented-out "// DbSet<Item> Items" placeholder. Set<T>()
// works against any DbContext without needing a named DbSet property.
public class ItemEfAdapter(DbContext ctx) : IItemPort
{
    public async Task<Item?> FindByIdAsync(int id, CancellationToken ct = default) =>
        await ctx.Set<Item>().FindAsync([id], ct);
    public async Task<IEnumerable<Item>> FindAllAsync(CancellationToken ct = default) =>
        await ctx.Set<Item>().ToListAsync(ct);
    public async Task SaveAsync(Item item, CancellationToken ct = default)
    {
        ctx.Set<Item>().Add(item);
        await ctx.SaveChangesAsync(ct);
    }
}
"""),
            }
        };
    }

    internal static IReadOnlyList<(string RelativePath, string Content)> BuildDotNetHttpRepositoryFiles() => new[]
    {
        ("src/Services/IItemService.cs", """
namespace Services;

public record ItemDto(int Id, string Name, string Description);

public interface IItemService
{
    Task<IEnumerable<ItemDto>> GetAllAsync(CancellationToken ct = default);
    Task<ItemDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ItemDto> CreateAsync(ItemDto item, CancellationToken ct = default);
}
"""),
        ("src/Services/ItemService.cs", """
using System.Net.Http.Json;

namespace Services;

// Blazor WebAssembly runs entirely in the browser and can't open a direct database
// connection — no raw sockets, no native drivers. Data access always goes through an HTTP
// API instead. Point HttpClient.BaseAddress (registered in Program.cs) at your backend
// (e.g. an ASP.NET Core Web API project) rather than a database.
public class ItemService(HttpClient http) : IItemService
{
    public async Task<IEnumerable<ItemDto>> GetAllAsync(CancellationToken ct = default) =>
        await http.GetFromJsonAsync<IEnumerable<ItemDto>>("api/items", ct) ?? [];

    public async Task<ItemDto?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<ItemDto>($"api/items/{id}", ct);

    public async Task<ItemDto> CreateAsync(ItemDto item, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("api/items", item, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ItemDto>(ct))!;
    }
}
"""),
    };

    internal static IReadOnlyList<(string RelativePath, string Content)> BuildDotNetMongoRepositoryFiles() => new[]
    {
        ("src/Domain/Item.cs", """
namespace Domain;

public class Item
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
"""),
        ("src/Domain/Interfaces/IItemRepository.cs", """
namespace Domain.Interfaces;

using Domain;

public interface IItemRepository
{
    Task<Item?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IEnumerable<Item>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Item item, CancellationToken ct = default);
    Task UpdateAsync(Item item, CancellationToken ct = default);
    Task RemoveAsync(string id, CancellationToken ct = default);
}
"""),
        ("src/Infrastructure/Repositories/ItemRepository.cs", """
using Domain;
using Domain.Interfaces;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

// Register the MongoDB client in Program.cs, e.g.:
//   builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(builder.Configuration.GetConnectionString("Default")));
//   builder.Services.AddScoped(sp => sp.GetRequiredService<IMongoClient>().GetDatabase("{{DB_NAME}}"));
public class ItemRepository(IMongoDatabase database) : IItemRepository
{
    private readonly IMongoCollection<Item> _items = database.GetCollection<Item>("items");

    public async Task<Item?> GetByIdAsync(string id, CancellationToken ct = default) =>
        await (await _items.FindAsync(i => i.Id == id, cancellationToken: ct)).FirstOrDefaultAsync(ct);

    public async Task<IEnumerable<Item>> GetAllAsync(CancellationToken ct = default) =>
        await (await _items.FindAsync(FilterDefinition<Item>.Empty, cancellationToken: ct)).ToListAsync(ct);

    public Task AddAsync(Item item, CancellationToken ct = default) =>
        _items.InsertOneAsync(item, cancellationToken: ct);

    public Task UpdateAsync(Item item, CancellationToken ct = default) =>
        _items.ReplaceOneAsync(i => i.Id == item.Id, item, cancellationToken: ct);

    public Task RemoveAsync(string id, CancellationToken ct = default) =>
        _items.DeleteOneAsync(i => i.Id == id, ct);
}
"""),
    };

    internal static IReadOnlyList<(string RelativePath, string Content)> BuildDotNetRedisRepositoryFiles() => new[]
    {
        ("src/Domain/Item.cs", """
namespace Domain;

public class Item
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
"""),
        ("src/Domain/Interfaces/IItemRepository.cs", """
namespace Domain.Interfaces;

using Domain;

public interface IItemRepository
{
    Task<Item?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IEnumerable<Item>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Item item, CancellationToken ct = default);
    Task UpdateAsync(Item item, CancellationToken ct = default);
    Task RemoveAsync(string id, CancellationToken ct = default);
}
"""),
        ("src/Infrastructure/Repositories/ItemRepository.cs", """
using System.Text.Json;
using Domain;
using Domain.Interfaces;
using StackExchange.Redis;

namespace Infrastructure.Repositories;

// Register the Redis connection in Program.cs, e.g.:
//   builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
//       ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Default")!));
public class ItemRepository(IConnectionMultiplexer redis) : IItemRepository
{
    private readonly IDatabase _db = redis.GetDatabase();
    internal static string Key(string id) => $"item:{id}";

    public async Task<Item?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var value = await _db.StringGetAsync(Key(id));
        return value.HasValue ? JsonSerializer.Deserialize<Item>((string)value!) : null;
    }

    public async Task<IEnumerable<Item>> GetAllAsync(CancellationToken ct = default)
    {
        var server = redis.GetServer(redis.GetEndPoints()[0]);
        var items = new List<Item>();
        await foreach (var key in server.KeysAsync(pattern: "item:*"))
        {
            var value = await _db.StringGetAsync(key);
            if (value.HasValue) items.Add(JsonSerializer.Deserialize<Item>((string)value!)!);
        }
        return items;
    }

    public Task AddAsync(Item item, CancellationToken ct = default) =>
        _db.StringSetAsync(Key(item.Id), JsonSerializer.Serialize(item));

    public Task UpdateAsync(Item item, CancellationToken ct = default) => AddAsync(item, ct);

    public Task RemoveAsync(string id, CancellationToken ct = default) =>
        _db.KeyDeleteAsync(Key(id));
}
"""),
    };

    // Spring/Quarkus/Micronaut each use a different DI container (Spring stereotypes vs. Jakarta
    // CDI vs. Jakarta jakarta.inject) — every pattern below that needs a managed bean picks its
    // import/annotation from these instead of hardcoding Spring's, which is what broke every
    // pattern the moment Quarkus or Micronaut was selected (the wizard offers the exact same
    // "(Spring Boot)"-labeled pattern catalog entries to all three Java frameworks).
    private static string JavaDiImport(FrameworkType framework) => framework switch
    {
        FrameworkType.Quarkus   => "import jakarta.enterprise.context.ApplicationScoped;",
        FrameworkType.Micronaut => "import jakarta.inject.Singleton;",
        _                       => "import org.springframework.stereotype.Service;"
    };

    private static string JavaDiAnnotation(FrameworkType framework) => framework switch
    {
        FrameworkType.Quarkus   => "@ApplicationScoped",
        FrameworkType.Micronaut => "@Singleton",
        _                       => "@Service"
    };

    // Self-contained in-memory Item + repository — shared by "repository" for MongoDB/Redis
    // (where the base pom.xml wires a Mongo/Redis client, not JPA or a JDBC DataSource, so
    // neither of the framework-specific persistence branches below would compile) and reused
    // wherever else a plain, always-compiles fallback is useful.
    private static IReadOnlyList<(string RelativePath, string Content)> BuildJavaInMemoryRepositoryFiles(string di, string diImport) => new[]
    {
        ("src/main/java/domain/model/Item.java", """
package domain.model;

public class Item {
    private final Long id;
    private final String name;
    private final String description;

    public Item(Long id, String name, String description) {
        this.id = id;
        this.name = name;
        this.description = description;
    }

    public Long getId() { return id; }
    public String getName() { return name; }
    public String getDescription() { return description; }
}
"""),
        ("src/main/java/domain/repository/ItemRepository.java", """
package domain.repository;

import domain.model.Item;
import java.util.List;
import java.util.Optional;

public interface ItemRepository {
    Optional<Item> findById(Long id);
    List<Item> findAll();
    Item save(Item item);
    void deleteById(Long id);
}
"""),
        ($$"""src/main/java/infrastructure/persistence/InMemoryItemRepository.java""", $$"""
package infrastructure.persistence;

import domain.model.Item;
import domain.repository.ItemRepository;
{{diImport}}
import java.util.List;
import java.util.Optional;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.atomic.AtomicLong;

// Plug in the MongoDB/Redis client this project was generated with (see app config) once you
// need real persistence — this in-memory store keeps the Repository pattern's shape compilable
// without depending on database-specific driver APIs that differ per Java framework.
{{di}}
public class InMemoryItemRepository implements ItemRepository {
    private final ConcurrentHashMap<Long, Item> store = new ConcurrentHashMap<>();
    private final AtomicLong sequence = new AtomicLong();

    @Override public Optional<Item> findById(Long id) { return Optional.ofNullable(store.get(id)); }
    @Override public List<Item> findAll() { return List.copyOf(store.values()); }

    @Override
    public Item save(Item item) {
        var id = item.getId() != null ? item.getId() : sequence.incrementAndGet();
        var saved = new Item(id, item.getName(), item.getDescription());
        store.put(id, saved);
        return saved;
    }

    @Override public void deleteById(Long id) { store.remove(id); }
}
"""),
    };

    internal static IReadOnlyList<(string RelativePath, string Content)> BuildJavaPatternFiles(
        FrameworkType framework, DatabaseType database, string pattern)
    {
        var di = JavaDiAnnotation(framework);
        var diImport = JavaDiImport(framework);

        return NormalizePatternToken(pattern) switch
        {
            // The only pattern that actually needs real persistence. MongoDB/Redis get the
            // framework-agnostic in-memory fallback (the base pom.xml wires a Mongo/Redis client
            // for those, not JPA or a JDBC DataSource). Relational DBs get a real implementation,
            // and that differs per framework — Quarkus/Micronaut don't have Spring Data JPA on
            // the classpath, so each gets its own Item + repository instead of sharing Spring's.
            "repository" when database is DatabaseType.MongoDB or DatabaseType.Redis =>
                BuildJavaInMemoryRepositoryFiles(di, diImport),

            "repository" => framework switch
            {
                FrameworkType.Quarkus => new[]
                {
                    ("src/main/java/domain/model/Item.java", """
package domain.model;

import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;

@Entity
public class Item {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;
    private String name;
    private String description;

    protected Item() {}
    public Item(String name, String description) { this.name = name; this.description = description; }

    public Long getId() { return id; }
    public String getName() { return name; }
    public String getDescription() { return description; }
}
"""),
                    ("src/main/java/domain/repository/ItemRepository.java", """
package domain.repository;

import domain.model.Item;
import java.util.List;
import java.util.Optional;

public interface ItemRepository {
    Optional<Item> findById(Long id);
    List<Item> findAll();
    Item save(Item item);
    void deleteById(Long id);
}
"""),
                    ("src/main/java/infrastructure/persistence/JpaItemRepository.java", """
package infrastructure.persistence;

import domain.model.Item;
import domain.repository.ItemRepository;
import jakarta.enterprise.context.ApplicationScoped;
import jakarta.persistence.EntityManager;
import jakarta.transaction.Transactional;
import java.util.List;
import java.util.Optional;

@ApplicationScoped
public class JpaItemRepository implements ItemRepository {
    private final EntityManager em;
    public JpaItemRepository(EntityManager em) { this.em = em; }

    @Override
    public Optional<Item> findById(Long id) { return Optional.ofNullable(em.find(Item.class, id)); }

    @Override
    public List<Item> findAll() { return em.createQuery("from Item", Item.class).getResultList(); }

    @Override
    @Transactional
    public Item save(Item item) {
        if (item.getId() == null) { em.persist(item); return item; }
        return em.merge(item);
    }

    @Override
    @Transactional
    public void deleteById(Long id) {
        var item = em.find(Item.class, id);
        if (item != null) em.remove(item);
    }
}
"""),
                },
                FrameworkType.Micronaut => new[]
                {
                    // Micronaut's pom only wires micronaut-jdbc-hikari (a plain DataSource) — no
                    // JPA/Hibernate — so Item stays a plain POJO and the repository maps rows by
                    // hand instead of relying on an ORM that isn't on the classpath.
                    ("src/main/java/domain/model/Item.java", """
package domain.model;

public class Item {
    private final Long id;
    private final String name;
    private final String description;

    public Item(Long id, String name, String description) {
        this.id = id;
        this.name = name;
        this.description = description;
    }

    public Long getId() { return id; }
    public String getName() { return name; }
    public String getDescription() { return description; }
}
"""),
                    ("src/main/java/domain/repository/ItemRepository.java", """
package domain.repository;

import domain.model.Item;
import java.util.List;
import java.util.Optional;

public interface ItemRepository {
    Optional<Item> findById(Long id);
    List<Item> findAll();
    Item save(Item item);
    void deleteById(Long id);
}
"""),
                    ("src/main/java/infrastructure/persistence/JdbcItemRepository.java", """
package infrastructure.persistence;

import domain.model.Item;
import domain.repository.ItemRepository;
import jakarta.inject.Singleton;
import javax.sql.DataSource;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Statement;
import java.util.ArrayList;
import java.util.List;
import java.util.Optional;

// Assumes an "items(id, name, description)" table — this generator doesn't wire a migration
// tool for Micronaut, so create it manually before relying on this repository.
@Singleton
public class JdbcItemRepository implements ItemRepository {
    private final DataSource dataSource;
    public JdbcItemRepository(DataSource dataSource) { this.dataSource = dataSource; }

    @Override
    public Optional<Item> findById(Long id) {
        try (var conn = dataSource.getConnection();
             var stmt = conn.prepareStatement("SELECT id, name, description FROM items WHERE id = ?")) {
            stmt.setLong(1, id);
            try (var rs = stmt.executeQuery()) {
                return rs.next() ? Optional.of(map(rs)) : Optional.empty();
            }
        } catch (SQLException e) { throw new RuntimeException(e); }
    }

    @Override
    public List<Item> findAll() {
        var items = new ArrayList<Item>();
        try (var conn = dataSource.getConnection();
             var stmt = conn.prepareStatement("SELECT id, name, description FROM items")) {
            try (var rs = stmt.executeQuery()) {
                while (rs.next()) items.add(map(rs));
            }
        } catch (SQLException e) { throw new RuntimeException(e); }
        return items;
    }

    @Override
    public Item save(Item item) {
        try (var conn = dataSource.getConnection();
             var stmt = conn.prepareStatement(
                     "INSERT INTO items (name, description) VALUES (?, ?)", Statement.RETURN_GENERATED_KEYS)) {
            stmt.setString(1, item.getName());
            stmt.setString(2, item.getDescription());
            stmt.executeUpdate();
            try (var keys = stmt.getGeneratedKeys()) {
                return keys.next() ? new Item(keys.getLong(1), item.getName(), item.getDescription()) : item;
            }
        } catch (SQLException e) { throw new RuntimeException(e); }
    }

    @Override
    public void deleteById(Long id) {
        try (var conn = dataSource.getConnection();
             var stmt = conn.prepareStatement("DELETE FROM items WHERE id = ?")) {
            stmt.setLong(1, id);
            stmt.executeUpdate();
        } catch (SQLException e) { throw new RuntimeException(e); }
    }

    private static Item map(ResultSet rs) throws SQLException {
        return new Item(rs.getLong("id"), rs.getString("name"), rs.getString("description"));
    }
}
"""),
                },
                _ => new[]
                {
                    ("src/main/java/domain/model/Item.java", """
package domain.model;

import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;

@Entity
public class Item {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;
    private String name;
    private String description;

    protected Item() {}
    public Item(String name, String description) { this.name = name; this.description = description; }

    public Long getId() { return id; }
    public String getName() { return name; }
    public String getDescription() { return description; }
}
"""),
                    ("src/main/java/domain/repository/ItemRepository.java", """
package domain.repository;

import domain.model.Item;
import java.util.List;
import java.util.Optional;

public interface ItemRepository {
    Optional<Item> findById(Long id);
    List<Item> findAll();
    Item save(Item item);
    void deleteById(Long id);
}
"""),
                    ("src/main/java/infrastructure/persistence/JpaItemRepository.java", """
package infrastructure.persistence;

import domain.model.Item;
import domain.repository.ItemRepository;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

@Repository
public interface JpaItemRepository extends JpaRepository<Item, Long>, ItemRepository {}
"""),
                }
            },

            // Self-contained in-memory store — CQRS's teaching point is command/query
            // separation, not persistence, so it doesn't need a real DB dependency at all (and
            // therefore works identically on all three Java frameworks).
            "cqrs" => new[]
            {
                ("src/main/java/domain/model/Item.java", $$"""
package domain.model;

public class Item {
    private final Long id;
    private final String name;
    private final String description;

    public Item(Long id, String name, String description) {
        this.id = id;
        this.name = name;
        this.description = description;
    }

    public Long getId() { return id; }
    public String getName() { return name; }
    public String getDescription() { return description; }
}
"""),
                ("src/main/java/application/command/CreateItemCommand.java", """
package application.command;

public record CreateItemCommand(String name, String description) {}
"""),
                ("src/main/java/application/command/CreateItemCommandHandler.java", $$"""
package application.command;

import domain.model.Item;
{{diImport}}
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.atomic.AtomicLong;

{{di}}
public class CreateItemCommandHandler {
    private static final ConcurrentHashMap<Long, Item> STORE = new ConcurrentHashMap<>();
    private static final AtomicLong SEQUENCE = new AtomicLong();

    public Long handle(CreateItemCommand command) {
        long id = SEQUENCE.incrementAndGet();
        STORE.put(id, new Item(id, command.name(), command.description()));
        return id;
    }
}
"""),
                ("src/main/java/application/query/GetAllItemsQuery.java", """
package application.query;

import java.util.List;

public interface GetAllItemsQuery {
    List<ItemDto> execute();
    record ItemDto(Long id, String name, String description) {}
}
"""),
            },

            // Clean Architecture: domain + use case layer, deliberately independent of any
            // framework — the in-memory adapter below is the one piece that needs a managed-bean
            // annotation, so it's the only one keyed off `di`.
            "cleanarchitecture" => new[]
            {
                ("src/main/java/domain/model/Item.java", """
package domain.model;

import java.time.Instant;

public class Item {
    private final Long id;
    private final String name;
    private final String description;
    private final Instant createdAt;

    public Item(Long id, String name, String description) {
        this.id = id;
        this.name = name;
        this.description = description;
        this.createdAt = Instant.now();
    }

    public static Item create(Long id, String name, String description) { return new Item(id, name, description); }

    public Long getId() { return id; }
    public String getName() { return name; }
    public String getDescription() { return description; }
    public Instant getCreatedAt() { return createdAt; }
}
"""),
                ("src/main/java/domain/port/ItemRepository.java", """
package domain.port;

import domain.model.Item;
import java.util.List;
import java.util.Optional;

public interface ItemRepository {
    Item save(Item item);
    Optional<Item> findById(Long id);
    List<Item> findAll();
}
"""),
                ("src/main/java/application/usecase/CreateItemUseCase.java", """
package application.usecase;

import domain.model.Item;
import domain.port.ItemRepository;
import java.util.concurrent.atomic.AtomicLong;

public class CreateItemUseCase {
    private static final AtomicLong SEQUENCE = new AtomicLong();
    private final ItemRepository repository;
    public CreateItemUseCase(ItemRepository repository) { this.repository = repository; }

    public Long execute(String name, String description) {
        var item = Item.create(SEQUENCE.incrementAndGet(), name, description);
        return repository.save(item).getId();
    }
}
"""),
                ("src/main/java/infrastructure/persistence/InMemoryItemRepository.java", $$"""
package infrastructure.persistence;

import domain.model.Item;
import domain.port.ItemRepository;
{{diImport}}
import java.util.List;
import java.util.Optional;
import java.util.concurrent.ConcurrentHashMap;

{{di}}
public class InMemoryItemRepository implements ItemRepository {
    private final ConcurrentHashMap<Long, Item> store = new ConcurrentHashMap<>();

    @Override public Item save(Item item) { store.put(item.getId(), item); return item; }
    @Override public Optional<Item> findById(Long id) { return Optional.ofNullable(store.get(id)); }
    @Override public List<Item> findAll() { return List.copyOf(store.values()); }
}
"""),
            },

            // Hexagonal Architecture (ports & adapters) — same shape as Clean Architecture above,
            // named per hexagonal convention (port/adapter instead of port/use case).
            "hexagonalarchitecture" => new[]
            {
                ("src/main/java/domain/model/Item.java", """
package domain.model;

public class Item {
    private final Long id;
    private final String name;
    private final String description;

    public Item(Long id, String name, String description) {
        this.id = id;
        this.name = name;
        this.description = description;
    }

    public Long getId() { return id; }
    public String getName() { return name; }
    public String getDescription() { return description; }
}
"""),
                ("src/main/java/domain/port/ItemRepository.java", """
package domain.port;

import domain.model.Item;
import java.util.List;
import java.util.Optional;

public interface ItemRepository {
    Item save(Item item);
    Optional<Item> findById(Long id);
    List<Item> findAll();
}
"""),
                ("src/main/java/infrastructure/adapter/InMemoryItemAdapter.java", $$"""
package infrastructure.adapter;

import domain.model.Item;
import domain.port.ItemRepository;
{{diImport}}
import java.util.List;
import java.util.Optional;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.atomic.AtomicLong;

{{di}}
public class InMemoryItemAdapter implements ItemRepository {
    private final ConcurrentHashMap<Long, Item> store = new ConcurrentHashMap<>();
    private final AtomicLong sequence = new AtomicLong();

    @Override
    public Item save(Item item) {
        var id = item.getId() != null ? item.getId() : sequence.incrementAndGet();
        var saved = new Item(id, item.getName(), item.getDescription());
        store.put(id, saved);
        return saved;
    }

    @Override public Optional<Item> findById(Long id) { return Optional.ofNullable(store.get(id)); }
    @Override public List<Item> findAll() { return List.copyOf(store.values()); }
}
"""),
            },

            // Domain-Driven Design: AggregateRoot + DomainEvent are generated together here
            // (rather than assuming the separate "Event Sourcing" pattern is also selected — the
            // wizard only lets you pick one design pattern per project).
            "domaindrivendesign" => new[]
            {
                ("src/main/java/domain/event/DomainEvent.java", """
package domain.event;

import java.time.Instant;
import java.util.UUID;

public abstract class DomainEvent {
    private final UUID id = UUID.randomUUID();
    private final Instant occurredAt = Instant.now();

    public UUID getId() { return id; }
    public Instant getOccurredAt() { return occurredAt; }
}
"""),
                ("src/main/java/domain/model/AggregateRoot.java", """
package domain.model;

import domain.event.DomainEvent;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

public abstract class AggregateRoot {
    private final List<DomainEvent> events = new ArrayList<>();

    protected void registerEvent(DomainEvent event) { events.add(event); }
    public List<DomainEvent> getDomainEvents() { return Collections.unmodifiableList(events); }
    public void clearDomainEvents() { events.clear(); }
}
"""),
            },

            "eventsourcing" => new[]
            {
                ("src/main/java/domain/event/DomainEvent.java", """
package domain.event;

import java.time.Instant;
import java.util.UUID;

public abstract class DomainEvent {
    private final UUID id = UUID.randomUUID();
    private final Instant occurredAt = Instant.now();
    private final String aggregateId;

    protected DomainEvent(String aggregateId) { this.aggregateId = aggregateId; }
    public UUID getId() { return id; }
    public Instant getOccurredAt() { return occurredAt; }
    public String getAggregateId() { return aggregateId; }
}
"""),
            },

            // Spring Cloud (Eureka discovery) has no Quarkus/Micronaut equivalent wired into this
            // generator's dependency injection, so this stays Spring-only — matching how e.g. the
            // PHP generator returns an empty file set (and logs "patrón no soportado") for
            // pattern/framework combinations it doesn't support, rather than emitting code that
            // references a dependency nothing ever adds to the build.
            "microservices" => framework == FrameworkType.SpringBoot ? new[]
            {
                ("services/orders/src/main/java/OrdersApplication.java", """
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;

@SpringBootApplication
public class OrdersApplication {
    public static void main(String[] args) { SpringApplication.run(OrdersApplication.class, args); }
}
"""),
            } : Array.Empty<(string, string)>(),

            "mediator" => new[]
            {
                ("src/main/java/application/mediator/Mediator.java", """
package application.mediator;

import java.util.HashMap;
import java.util.Map;

public class Mediator {
    private final Map<Class<?>, RequestHandler<?, ?>> handlers = new HashMap<>();

    @SuppressWarnings("unchecked")
    public <TRequest, TResponse> void register(Class<TRequest> type, RequestHandler<TRequest, TResponse> handler) {
        handlers.put(type, handler);
    }

    @SuppressWarnings("unchecked")
    public <TResponse> TResponse send(Object request) throws Exception {
        var handler = (RequestHandler<Object, TResponse>) handlers.get(request.getClass());
        if (handler == null) throw new IllegalArgumentException("No handler for " + request.getClass().getSimpleName());
        return handler.handle(request);
    }
}
"""),
                ("src/main/java/application/mediator/RequestHandler.java", """
package application.mediator;

@FunctionalInterface
public interface RequestHandler<TRequest, TResponse> {
    TResponse handle(TRequest request) throws Exception;
}
"""),
            },

            "saga" => new[]
            {
                ("src/main/java/application/saga/OrderSaga.java", $$"""
package application.saga;

{{diImport}}

{{di}}
public class OrderSaga {
    public boolean execute(String customerId, Object[] items, Object paymentInfo) {
        String orderId = java.util.UUID.randomUUID().toString();
        String reservationId = null;

        try {
            createOrder(orderId, customerId, items);
            reservationId = reserveInventory(orderId, items);
            processPayment(orderId, paymentInfo);
            confirmOrder(orderId);
            return true;
        } catch (Exception e) {
            // Compensate in reverse order
            if (reservationId != null) releaseInventory(reservationId);
            cancelOrder(orderId);
            return false;
        }
    }

    private void createOrder(String id, String customerId, Object[] items) { /* implement */ }
    private String reserveInventory(String orderId, Object[] items) { return java.util.UUID.randomUUID().toString(); }
    private void processPayment(String orderId, Object paymentInfo) { /* implement */ }
    private void confirmOrder(String orderId) { /* implement */ }
    private void cancelOrder(String orderId) { /* implement */ }
    private void releaseInventory(String reservationId) { /* implement */ }
}
"""),
            },

            _ => Array.Empty<(string, string)>()
        };
    }

    private const string PythonAbstractRepositoryFile = """
from abc import ABC, abstractmethod
from typing import Generic, TypeVar, Optional, List

T = TypeVar('T')

class AbstractRepository(ABC, Generic[T]):
    @abstractmethod
    async def get_by_id(self, id: int) -> Optional[T]: ...
    @abstractmethod
    async def get_all(self) -> List[T]: ...
    @abstractmethod
    async def save(self, entity: T) -> T: ...
    @abstractmethod
    async def delete(self, id: int) -> None: ...
""";

    // Self-contained in-memory Item + repository — the real SQLAlchemy-backed one below only
    // makes sense for FastAPI + a relational DB (async SQLAlchemy is what ScaffoldPythonBaseFilesAsync
    // wires up for that combo specifically). Flask uses sync SQLAlchemy, Django doesn't generate
    // an app/models.py at all through this pipeline, and Mongo/Redis get a different client
    // entirely — reusing the async-SQLAlchemy version for any of those previously either
    // ImportError'd (the old code imported a nonexistent "app.domain.item", which only ever
    // existed if "Clean Architecture" was ALSO picked as the pattern — but only one pattern is
    // ever selected) or silently mismatched the actual DB client that project was generated with.
    private static readonly IReadOnlyList<(string RelativePath, string Content)> PythonInMemoryRepositoryFiles = new[]
    {
        ("app/repositories/base.py", PythonAbstractRepositoryFile),
        ("app/repositories/item_repository.py", """
from dataclasses import dataclass, field
from datetime import datetime
from itertools import count
from typing import Optional, List, Dict
from app.repositories.base import AbstractRepository

@dataclass
class Item:
    name: str
    description: Optional[str] = None
    id: int = 0
    created_at: datetime = field(default_factory=datetime.utcnow)

class ItemRepository(AbstractRepository[Item]):
    # In-memory store — swap in the real DB client this project was generated with (see
    # app/database.py) once you need actual persistence.
    _ids = count(1)

    def __init__(self):
        self._items: Dict[int, Item] = {}

    async def get_by_id(self, id: int) -> Optional[Item]:
        return self._items.get(id)

    async def get_all(self) -> List[Item]:
        return list(self._items.values())

    async def save(self, entity: Item) -> Item:
        if not entity.id:
            entity.id = next(self._ids)
        self._items[entity.id] = entity
        return entity

    async def delete(self, id: int) -> None:
        self._items.pop(id, None)
"""),
    };

    internal static IReadOnlyList<(string RelativePath, string Content)> BuildPythonPatternFiles(
        FrameworkType framework, DatabaseType database, string pattern)
    {
        var isRelational = database is DatabaseType.PostgreSQL or DatabaseType.MySQL
            or DatabaseType.SqlServer or DatabaseType.SQLite;

        return NormalizePatternToken(pattern) switch
        {
            "repository" => framework == FrameworkType.FastAPI && isRelational
                ? new[]
                {
                    ("app/repositories/base.py", PythonAbstractRepositoryFile),
                    ("app/repositories/item_repository.py", """
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select
from app.repositories.base import AbstractRepository
from app.models import Item

class ItemRepository(AbstractRepository[Item]):
    def __init__(self, session: AsyncSession):
        self.session = session

    async def get_by_id(self, id: int):
        result = await self.session.execute(select(Item).where(Item.id == id))
        return result.scalar_one_or_none()

    async def get_all(self):
        result = await self.session.execute(select(Item))
        return result.scalars().all()

    async def save(self, entity: Item):
        self.session.add(entity)
        await self.session.commit()
        await self.session.refresh(entity)
        return entity

    async def delete(self, id: int):
        item = await self.get_by_id(id)
        if item:
            await self.session.delete(item)
            await self.session.commit()
"""),
                }
                : PythonInMemoryRepositoryFiles,
            "cleanarchitecture" => new[]
            {
                ("app/domain/__init__.py", ""),
                ("app/domain/item.py", """
from dataclasses import dataclass, field
from datetime import datetime

@dataclass
class Item:
    name: str
    description: str
    id: int = 0
    created_at: datetime = field(default_factory=datetime.utcnow)

    @classmethod
    def create(cls, name: str, description: str) -> 'Item':
        if not name:
            raise ValueError("name cannot be empty")
        return cls(name=name, description=description)
"""),
                ("app/application/use_cases/create_item.py", """
from app.domain.item import Item

class CreateItemUseCase:
    def __init__(self, repository):
        self.repository = repository

    async def execute(self, name: str, description: str) -> Item:
        item = Item.create(name, description)
        return await self.repository.save(item)
"""),
            },
            "cqrs" => new[]
            {
                ("app/commands/create_item.py", """
from dataclasses import dataclass

@dataclass
class CreateItemCommand:
    name: str
    description: str
"""),
                ("app/handlers/create_item_handler.py", """
from dataclasses import dataclass, field
from datetime import datetime
from typing import Optional
from app.commands.create_item import CreateItemCommand

@dataclass
class Item:
    name: str
    description: Optional[str] = None
    id: int = 0
    created_at: datetime = field(default_factory=datetime.utcnow)

class CreateItemHandler:
    def __init__(self, repository):
        self.repository = repository

    async def handle(self, command: CreateItemCommand):
        item = Item(name=command.name, description=command.description)
        return await self.repository.save(item)
"""),
                ("app/queries/get_all_items.py", """
class GetAllItemsQuery:
    def __init__(self, repository):
        self.repository = repository

    async def execute(self):
        return await self.repository.get_all()
"""),
            },
            "hexagonalarchitecture" => new[]
            {
                ("app/core/ports/item_port.py", """
from abc import ABC, abstractmethod
from typing import Optional, List

class ItemPort(ABC):
    @abstractmethod
    async def find_by_id(self, id: int) -> Optional[dict]: ...
    @abstractmethod
    async def find_all(self) -> List[dict]: ...
    @abstractmethod
    async def save(self, item: dict) -> dict: ...
"""),
                ("app/infrastructure/adapters/sqlalchemy_item_adapter.py", """
from app.core.ports.item_port import ItemPort

class SqlAlchemyItemAdapter(ItemPort):
    def __init__(self, session):
        self.session = session

    async def find_by_id(self, id: int):
        # SQLAlchemy implementation
        pass

    async def find_all(self):
        pass

    async def save(self, item: dict):
        pass
"""),
            },
            "domaindrivendesign" => new[]
            {
                ("app/domain/aggregates/aggregate_root.py", """
from typing import List
from dataclasses import dataclass, field

@dataclass
class AggregateRoot:
    _events: List = field(default_factory=list, repr=False)

    def register_event(self, event) -> None:
        self._events.append(event)

    def clear_events(self) -> List:
        events = self._events.copy()
        self._events.clear()
        return events
"""),
                ("app/domain/value_objects/base.py", """
from dataclasses import dataclass

@dataclass(frozen=True)
class ValueObject:
    # Base class for all Value Objects. Immutable by default (frozen=True).
    pass
"""),
            },
            "microservices" => new[]
            {
                ("services/items/main.py", """
from fastapi import FastAPI

app = FastAPI(title="Items Service")

@app.get("/health")
def health():
    return {"service": "items", "status": "ok"}

@app.get("/items")
def list_items():
    return []
"""),
                ("services/notifications/main.py", """
from fastapi import FastAPI

app = FastAPI(title="Notifications Service")

@app.get("/health")
def health():
    return {"service": "notifications", "status": "ok"}
"""),
                ("services/gateway/main.py", """
import httpx
from fastapi import FastAPI, Request

app = FastAPI(title="API Gateway")

SERVICES = {
    "items": "http://items-service:8000",
    "notifications": "http://notifications-service:8001",
}

@app.get("/health")
def health():
    return {"gateway": True, "services": list(SERVICES.keys())}
"""),
            },
            "mediator" => new[]
            {
                ("app/application/mediator.py", """
from typing import Any, Callable, Dict, Type

class Mediator:
    def __init__(self):
        self._handlers: Dict[Type, Callable] = {}

    def register(self, request_type: Type, handler: Callable) -> None:
        self._handlers[request_type] = handler

    async def send(self, request: Any) -> Any:
        handler = self._handlers.get(type(request))
        if not handler:
            raise ValueError(f"No handler registered for {type(request).__name__}")
        return await handler(request)
"""),
                ("app/application/messages.py", """
from dataclasses import dataclass

@dataclass
class CreateItemRequest:
    name: str
    description: str

@dataclass
class GetAllItemsRequest:
    pass
"""),
            },
            "saga" => new[]
            {
                ("app/application/sagas/order_saga.py", """"
import uuid
from typing import Optional

class OrderSaga:
    """Orchestration saga for order creation workflow."""

    async def execute(self, customer_id: str, items: list, payment_info: dict) -> bool:
        order_id = str(uuid.uuid4())
        reservation_id: Optional[str] = None

        try:
            await self._create_order(order_id, customer_id, items)
            reservation_id = await self._reserve_inventory(order_id, items)
            await self._process_payment(order_id, payment_info)
            await self._confirm_order(order_id)
            return True
        except Exception as e:
            print(f"Saga failed: {e} — compensating...")
            if reservation_id:
                await self._release_inventory(reservation_id)
            await self._cancel_order(order_id)
            return False

    async def _create_order(self, order_id, customer_id, items): pass
    async def _reserve_inventory(self, order_id, items) -> str: return str(uuid.uuid4())
    async def _process_payment(self, order_id, payment_info): pass
    async def _confirm_order(self, order_id): pass
    async def _cancel_order(self, order_id): pass
    async def _release_inventory(self, reservation_id): pass
""""),
            },
            _ => Array.Empty<(string, string)>()
        };
    }
}
