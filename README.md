# Shared.Services

This repository contains a set of shared .NET libraries intended for reuse across microservices. Its goal is to centralize common functionality used by multiple services: `Result` types, `Error` types, pagination helpers, repository/UnitOfWork abstractions, and an EventBus implementation using RabbitMQ.

## Structure

- `BuildingBlocks/` - Core utilities such as `Entity`, `Result`, `Command` abstractions, `Paginate`, and EventBus interfaces.
- `Repositories/` - Interfaces and implementations for `IRepository`, `IReadRepository`, `IWriteRepository`, `UnitOfWork`, and `BaseRepository`.
- `Shared.Services.sln` - The solution file.

## Requirements

- .NET 9 SDK (projects target `net9.0`).

## Build

From the repository root run:

```bash
dotnet build Shared.Services.sln --configuration Debug
```

## Usage and implementation examples

These libraries are meant to be referenced by service projects either via `ProjectReference` or a NuGet package. Below are common usage examples.

- Add a project reference to your consumer project (`.csproj`):

```xml
<ItemGroup>
  <ProjectReference Include="../BuildingBlocks/BuildingBlocks.csproj" />
</ItemGroup>
```

### Repository example

Typical flow: perform data operations through `IRepository<T>` / `IReadRepository<T>` and commit with `IUnitOfWork`.

Simple repository usage (constructor injection):

```csharp
public class UserService
{
	private readonly IReadRepository<User> _users;
	private readonly IUnitOfWork _uow;

	public UserService(IReadRepository<User> users, IUnitOfWork uow)
	{
		_users = users;
		_uow = uow;
	}

	public async Task<Result<User>> CreateAsync(User user)
	{
		await _users.AddAsync(user);
		await _uow.CommitAsync();
		return Result.Ok(user);
	}
}
```

Register `BaseRepository` and `UnitOfWork` in your DI container (e.g. in `Program.cs`):

```csharp
services.AddScoped(typeof(IReadRepository<>), typeof(BaseRepository<>));
services.AddScoped<IUnitOfWork, UnitOfWork>();
```

### UnitOfWork

Call `IUnitOfWork.CommitAsync()` to persist a set of write operations in a transactional manner. It's recommended to call `CommitAsync()` after performing write operations.

### EventBus (Integration Events)

EventBus abstractions are provided via `IEventBus`, `IEventSubscriptionManager`, and `IIntegrationEventHandler<TEvent>`. The RabbitMQ implementation is provided as `EventBusRabbitMQ`.

Example: create and publish an event

```csharp
public class UserCreatedEvent : IntegrationEvent
{
	public Guid UserId { get; }
	public string Email { get; }

	public UserCreatedEvent(Guid userId, string email)
	{
		UserId = userId;
		Email = email;
	}
}

// Publish example
await _eventBus.PublishAsync(new UserCreatedEvent(user.Id, user.Email));
```

Event handler example:

```csharp
public class SendWelcomeEmailHandler : IIntegrationEventHandler<UserCreatedEvent>
{
	public Task Handle(UserCreatedEvent @event)
	{
		// implement email sending logic
		return Task.CompletedTask;
	}
}
```

DI configuration and RabbitMQ connection (simplified example):

```csharp


services.AddSingleton<IEventBus>(sp=>{
    return EventBusFactory.Create( sp, new EventBusConfig());
});
```

Subscription (subscribe) is usually performed during application startup:

```csharp
var eventBus = app.Services.GetRequiredService<IEventBus>();
eventBus.Subscribe<UserCreatedEvent, SendWelcomeEmailHandler>();
```

### Command pattern (`ICommand` / `ICommandHandler`)

The `Commands` folder contains `ICommand` and `ICommandHandler<TCommand>` abstractions. This pattern is useful for centralized command execution in a CQRS-style approach.

Simple command and handler example:

```csharp
public class CreateUserCommand : ICommand
{
	public string Email { get; set; }
}

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand>
{
	private readonly IUserRepository _repo;

	public CreateUserCommandHandler(IUserRepository repo) => _repo = repo;

	public override async Task Execute(CreateUserCommand command)
	{
		var user = new User { Email = command.Email };
		await _repo.AddAsync(user);
		await _repo.UnitOfWork.CommitAsync();
	}
}
```

Command dispatch example (if you have a `CommandDispatcher` infrastructure):

```csharp
var command = new CreateUserCommand { Email = "a@b.com" };
await commandDispatcher.SendAsync(command);
```

If there is no dedicated `ICommandDispatcher`, you can resolve `ICommandHandler<T>` from DI and call `Handle` directly.

## Tests

This repository currently contains library code only. If you add tests, place them under a `tests/` folder and run them with `dotnet test`:

```bash
dotnet test
```

## Contributing

- Follow the existing code style.
- Add unit tests for new behavior.
- Open a pull request describing the changes and their impact.

## License

There is no license file in this repository. Please add an appropriate `LICENSE` file and update this README accordingly.

## Contact

For issues or integration questions, open an issue or contact the repository maintainer.
# Shared.Services
