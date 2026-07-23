using Microsoft.EntityFrameworkCore;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Persistence.Contexts;

namespace MikroProje.Persistence.Repositories;

public class AppUserRepository : IAppUserRepository
{
    private readonly MikroProjeDbContext _context;

    public AppUserRepository(MikroProjeDbContext context)
    {
        _context = context;
    }

    public async Task<AppUser?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        // Case-insensitive in SQL Server by default with standard collation
        return await _context.AppUsers
            .FirstOrDefaultAsync(x => x.Email == email && !x.IsDeleted, cancellationToken);
    }

    public async Task<AppUser?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.AppUsers
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
    }

    public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken)
    {
        var exists = await _context.AppUsers
            .AnyAsync(x => x.Email == email && !x.IsDeleted, cancellationToken);
        return !exists;
    }

    public async Task AddAsync(AppUser user, CancellationToken cancellationToken)
    {
        await _context.AppUsers.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
