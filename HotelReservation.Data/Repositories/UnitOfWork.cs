using HotelReservation.Data.Context;
using HotelReservation.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace HotelReservation.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public IHotelRepository Hotels { get; }
    public IRoomRepository Rooms { get; }
    public IAmenityRepository Amenities { get; }
    public IReservationRepository Reservations { get; }

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Hotels = new HotelRepository(_context);
        Rooms = new RoomRepository(_context);
        Amenities = new AmenityRepository(_context);
        Reservations = new ReservationRepository(_context);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
