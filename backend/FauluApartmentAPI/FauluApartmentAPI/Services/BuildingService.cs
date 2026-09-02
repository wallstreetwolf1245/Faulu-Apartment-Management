using FauluApartmentAPI.Data.Entities;
using FauluApartmentAPI.Data.Repositories;

namespace FauluApartmentAPI.Services;

/// <summary>
/// Building service interface for building operations
/// </summary>
public interface IBuildingService
{
    Task<Building?> GetBuildingByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Building>> GetBuildingsByOwnerAsync(int ownerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Building>> GetAllBuildingsAsync(CancellationToken cancellationToken = default);
    Task<Building> CreateBuildingAsync(Building building, CancellationToken cancellationToken = default);
    Task<Building> UpdateBuildingAsync(Building building, CancellationToken cancellationToken = default);
    Task DeleteBuildingAsync(int buildingId, CancellationToken cancellationToken = default);
    Task<int> GetOccupancyRateAsync(int buildingId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Building service implementation
/// </summary>
public class BuildingService : IBuildingService
{
    private readonly IRepository<Building> _buildingRepository;
    private readonly IUnitRepository _unitRepository;

    public BuildingService(IRepository<Building> buildingRepository, IUnitRepository unitRepository)
    {
        _buildingRepository = buildingRepository;
        _unitRepository = unitRepository;
    }

    public async Task<Building?> GetBuildingByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _buildingRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<Building>> GetBuildingsByOwnerAsync(int ownerId, CancellationToken cancellationToken = default)
    {
        return await _buildingRepository.FindAsync(b => b.OwnerId == ownerId, cancellationToken);
    }

    public async Task<IEnumerable<Building>> GetAllBuildingsAsync(CancellationToken cancellationToken = default)
    {
        return await _buildingRepository.GetAllAsync(cancellationToken);
    }

    public async Task<Building> CreateBuildingAsync(Building building, CancellationToken cancellationToken = default)
    {
        if (building == null)
            throw new ArgumentNullException(nameof(building));

        if (string.IsNullOrEmpty(building.Name))
            throw new ArgumentException("Building name is required");

        await _buildingRepository.AddAsync(building, cancellationToken);
        await _buildingRepository.SaveChangesAsync(cancellationToken);
        return building;
    }

    public async Task<Building> UpdateBuildingAsync(Building building, CancellationToken cancellationToken = default)
    {
        if (building == null)
            throw new ArgumentNullException(nameof(building));

        await _buildingRepository.UpdateAsync(building, cancellationToken);
        await _buildingRepository.SaveChangesAsync(cancellationToken);
        return building;
    }

    public async Task DeleteBuildingAsync(int buildingId, CancellationToken cancellationToken = default)
    {
        await _buildingRepository.DeleteAsync(buildingId, cancellationToken);
        await _buildingRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetOccupancyRateAsync(int buildingId, CancellationToken cancellationToken = default)
    {
        var building = await _buildingRepository.GetByIdAsync(buildingId, cancellationToken);
        if (building == null)
            return 0;

        var units = await _unitRepository.GetUnitsByBuildingAsync(buildingId, cancellationToken);
        var totalUnits = units.Count();
        var occupiedUnits = units.Count(u => u.Status == "Occupied");

        if (totalUnits == 0)
            return 0;

        return (occupiedUnits * 100) / totalUnits;
    }
}
