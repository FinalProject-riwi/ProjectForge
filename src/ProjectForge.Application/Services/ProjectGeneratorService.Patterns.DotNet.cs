using ProjectForge.Core.Enums;

namespace ProjectForge.Application.Services;

public partial class ProjectGeneratorService
{
    private static IReadOnlyList<(string RelativePath, string Content)> BuildDotNetPatternFiles(
        FrameworkType framework, string pattern)
    {
        return NormalizePatternToken(pattern) switch
        {
            "repository" => new[]
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
            },
            "cqrs" => new[]
            {
                ("src/Application/Commands/CreateItemCommand.cs", """
using MediatR;

namespace Application.Commands;

public record CreateItemCommand(string Name, string Description) : IRequest<int>;
"""),
                ("src/Application/Handlers/CreateItemCommandHandler.cs", """
using Application.Commands;
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
            "hexagonalarchitecture" => new[]
            {
                ("src/Core/Ports/IItemPort.cs", """
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

public class ItemEfAdapter(AppDbContext ctx) : IItemPort
{
    public async Task<Item?> FindByIdAsync(int id, CancellationToken ct = default) =>
        await ctx.Items.FindAsync([id], ct);
    public async Task<IEnumerable<Item>> FindAllAsync(CancellationToken ct = default) =>
        await ctx.Items.ToListAsync(ct);
    public async Task SaveAsync(Item item, CancellationToken ct = default)
    {
        ctx.Items.Add(item);
        await ctx.SaveChangesAsync(ct);
    }
}
"""),
            },
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
            "microservices" => new[]
            {
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
            _ => Array.Empty<(string, string)>()
        };
    }

    private static IReadOnlyList<(string RelativePath, string Content)> BuildJavaPatternFiles(
        FrameworkType framework, string pattern)
    {
        return NormalizePatternToken(pattern) switch
        {
            "repository" => new[]
            {
                ("src/main/java/domain/repository/ItemRepository.java", """
package domain.repository;

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

import domain.repository.ItemRepository;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

@Repository
public interface JpaItemRepository extends JpaRepository<ItemEntity, Long>, ItemRepository {}
"""),
            },
            "cqrs" => new[]
            {
                ("src/main/java/application/command/CreateItemCommand.java", """
package application.command;

public record CreateItemCommand(String name, String description) {}
"""),
                ("src/main/java/application/command/CreateItemCommandHandler.java", """
package application.command;

import domain.repository.ItemRepository;
import org.springframework.stereotype.Service;

@Service
public class CreateItemCommandHandler {
    private final ItemRepository repository;
    public CreateItemCommandHandler(ItemRepository repository) { this.repository = repository; }

    public Long handle(CreateItemCommand command) {
        var item = new Item(command.name(), command.description());
        return repository.save(item).getId();
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
            "cleanarchitecture" => new[]
            {
                ("src/main/java/domain/model/Item.java", """
package domain.model;

import java.time.Instant;

public class Item {
    private Long id;
    private String name;
    private String description;
    private final Instant createdAt = Instant.now();

    public static Item create(String name, String description) {
        var item = new Item();
        item.name = name;
        item.description = description;
        return item;
    }
    // getters omitted for brevity
}
"""),
                ("src/main/java/application/usecase/CreateItemUseCase.java", """
package application.usecase;

import domain.model.Item;
import domain.port.ItemRepository;

public class CreateItemUseCase {
    private final ItemRepository repository;
    public CreateItemUseCase(ItemRepository repository) { this.repository = repository; }

    public Long execute(String name, String description) {
        return repository.save(Item.create(name, description)).getId();
    }
}
"""),
            },
            "hexagonalarchitecture" => new[]
            {
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
                ("src/main/java/infrastructure/adapter/JpaItemAdapter.java", """
package infrastructure.adapter;

import domain.model.Item;
import domain.port.ItemRepository;
import org.springframework.stereotype.Component;
import java.util.List;
import java.util.Optional;

@Component
public class JpaItemAdapter implements ItemRepository {
    private final SpringDataItemRepository springRepo;
    public JpaItemAdapter(SpringDataItemRepository springRepo) { this.springRepo = springRepo; }

    @Override public Item save(Item item) { return springRepo.save(item); }
    @Override public Optional<Item> findById(Long id) { return springRepo.findById(id); }
    @Override public List<Item> findAll() { return springRepo.findAll(); }
}
"""),
            },
            "domaindrivendesign" => new[]
            {
                ("src/main/java/domain/model/AggregateRoot.java", """
package domain.model;

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
            "microservices" => new[]
            {
                ("services/orders/src/main/java/OrdersApplication.java", """
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.cloud.client.discovery.EnableDiscoveryClient;

@SpringBootApplication
@EnableDiscoveryClient
public class OrdersApplication {
    public static void main(String[] args) { SpringApplication.run(OrdersApplication.class, args); }
}
"""),
            },
            _ => Array.Empty<(string, string)>()
        };
    }

    private static IReadOnlyList<(string RelativePath, string Content)> BuildPythonPatternFiles(
        FrameworkType framework, string pattern)
    {
        return NormalizePatternToken(pattern) switch
        {
            "repository" => new[]
            {
                ("app/repositories/base.py", """
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
"""),
                ("app/repositories/item_repository.py", """
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select
from app.repositories.base import AbstractRepository
from app.domain.item import Item

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
            },
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
from app.commands.create_item import CreateItemCommand

class CreateItemHandler:
    def __init__(self, repository):
        self.repository = repository

    async def handle(self, command: CreateItemCommand):
        from app.domain.item import Item
        item = Item.create(command.name, command.description)
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
            _ => Array.Empty<(string, string)>()
        };
    }
}
