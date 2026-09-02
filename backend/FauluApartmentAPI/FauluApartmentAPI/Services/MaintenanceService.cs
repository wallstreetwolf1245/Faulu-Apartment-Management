using FauluApartmentAPI.Data.Entities;
using FauluApartmentAPI.Data.Repositories;

namespace FauluApartmentAPI.Services;

/// <summary>
/// Maintenance service interface for maintenance operations
/// </summary>
public interface IMaintenanceService
{
    Task<MaintenanceOrder?> GetOrderByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<MaintenanceOrder>> GetOrdersByBuildingAsync(int buildingId, CancellationToken cancellationToken = default);
    Task<IEnumerable<MaintenanceOrder>> GetOpenOrdersAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<MaintenanceOrder>> GetUrgentOrdersAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<MaintenanceOrder>> GetOrdersByStatusAsync(string status, CancellationToken cancellationToken = default);
    Task<int> GetOpenOrdersCountAsync(int buildingId, CancellationToken cancellationToken = default);
    Task<MaintenanceOrder> CreateOrderAsync(MaintenanceOrder order, CancellationToken cancellationToken = default);
    Task<MaintenanceOrder> UpdateOrderAsync(MaintenanceOrder order, CancellationToken cancellationToken = default);
    Task<MaintenanceOrder> CompleteOrderAsync(int orderId, CancellationToken cancellationToken = default);
    Task DeleteOrderAsync(int orderId, CancellationToken cancellationToken = default);

    // Owner-scoped variants — return only orders on buildings the given
    // user owns. Controllers should use these (not the unscoped versions
    // above) for any endpoint serving a specific logged-in landlord.
    Task<IEnumerable<MaintenanceOrder>> GetOpenOrdersForOwnerAsync(int ownerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<MaintenanceOrder>> GetUrgentOrdersForOwnerAsync(int ownerId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Maintenance service implementation
/// </summary>
public class MaintenanceService : IMaintenanceService
{
    private readonly IMaintenanceOrderRepository _maintenanceOrderRepository;
    private readonly IRepository<Building> _buildingRepository;
    private readonly IRepository<Unit> _unitRepository;

    public MaintenanceService(IMaintenanceOrderRepository maintenanceOrderRepository,
        IRepository<Building> buildingRepository,
        IRepository<Unit> unitRepository)
    {
        _maintenanceOrderRepository = maintenanceOrderRepository;
        _buildingRepository = buildingRepository;
        _unitRepository = unitRepository;
    }

    public async Task<MaintenanceOrder?> GetOrderByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _maintenanceOrderRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<MaintenanceOrder>> GetOrdersByBuildingAsync(int buildingId, CancellationToken cancellationToken = default)
    {
        return await _maintenanceOrderRepository.GetOrdersByBuildingAsync(buildingId, cancellationToken);
    }

    public async Task<IEnumerable<MaintenanceOrder>> GetOpenOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await _maintenanceOrderRepository.GetOpenOrdersAsync(cancellationToken);
    }

    public async Task<IEnumerable<MaintenanceOrder>> GetUrgentOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await _maintenanceOrderRepository.GetUrgentOrdersAsync(cancellationToken);
    }

    public async Task<IEnumerable<MaintenanceOrder>> GetOrdersByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        return await _maintenanceOrderRepository.GetOrdersByStatusAsync(status, cancellationToken);
    }

    public async Task<int> GetOpenOrdersCountAsync(int buildingId, CancellationToken cancellationToken = default)
    {
        return await _maintenanceOrderRepository.GetOpenOrdersCountAsync(buildingId, cancellationToken);
    }

    public async Task<MaintenanceOrder> CreateOrderAsync(MaintenanceOrder order, CancellationToken cancellationToken = default)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        if (string.IsNullOrEmpty(order.Title))
            throw new ArgumentException("Order title is required");

        if (string.IsNullOrEmpty(order.Description))
            throw new ArgumentException("Order description is required");

        // Validate building exists
        var building = await _buildingRepository.GetByIdAsync(order.BuildingId, cancellationToken);
        if (building == null)
            throw new ArgumentException("Building not found");

        // If a unit was provided, validate it and ensure it belongs to the building
        Unit? unit = null;
        if (order.UnitId.HasValue)
        {
            unit = await _unitRepository.GetByIdAsync(order.UnitId.Value, cancellationToken);
            if (unit == null)
                throw new ArgumentException("Unit not found");
            if (unit.BuildingId != order.BuildingId)
                throw new ArgumentException("Unit does not belong to the specified building");
        }

        order.ReportedDate = DateTime.UtcNow;
        order.Status = "Open";

        // Attach navigation properties so EF Core can track relationships
        order.Building = building;
        order.Unit = unit;

        await _maintenanceOrderRepository.AddAsync(order, cancellationToken);
        await _maintenanceOrderRepository.SaveChangesAsync(cancellationToken);
        return order;
    }

    public async Task<MaintenanceOrder> UpdateOrderAsync(MaintenanceOrder order, CancellationToken cancellationToken = default)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        await _maintenanceOrderRepository.UpdateAsync(order, cancellationToken);
        await _maintenanceOrderRepository.SaveChangesAsync(cancellationToken);
        return order;
    }

    public async Task<MaintenanceOrder> CompleteOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var order = await _maintenanceOrderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
            throw new InvalidOperationException("Maintenance order not found");

        order.Status = "Completed";
        order.CompletedDate = DateTime.UtcNow;
        await _maintenanceOrderRepository.UpdateAsync(order, cancellationToken);
        await _maintenanceOrderRepository.SaveChangesAsync(cancellationToken);

        return order;
    }

    public async Task DeleteOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        await _maintenanceOrderRepository.DeleteAsync(orderId, cancellationToken);
        await _maintenanceOrderRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<MaintenanceOrder>> GetOpenOrdersForOwnerAsync(int ownerId, CancellationToken cancellationToken = default)
    {
        return await _maintenanceOrderRepository.GetOpenOrdersByOwnerAsync(ownerId, cancellationToken);
    }

    public async Task<IEnumerable<MaintenanceOrder>> GetUrgentOrdersForOwnerAsync(int ownerId, CancellationToken cancellationToken = default)
    {
        return await _maintenanceOrderRepository.GetUrgentOrdersByOwnerAsync(ownerId, cancellationToken);
    }
}