using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.DataAccess.BuildingBlocks.Repositories;
using Shared.Services.BuildingBlocks.Repositories;
using Shared.Services.BuildingBlocks.Common;
using Shared.Services.BuildingBlocks.Exceptions;

public class UnitOfWork<TContext> : IUnitOfWork
    where TContext : DbContext
{
    private readonly TContext _context;
    private readonly IServiceProvider _provider;

    public UnitOfWork(TContext context, IServiceProvider provider)
    {
        _context = context;
        _provider = provider;
    }

    public IRepository<TEntity> Repository<TEntity>()
        where TEntity : Entity
    {
        return _provider.GetRequiredService<IRepository<TEntity>>();
    }

    public async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            await action();
            await _context.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            throw new TransactionFailedException(
                "Transaction failed. All changes were rolled back.",
                ex
            );
        }
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public void Dispose()
        => _context.Dispose();
}