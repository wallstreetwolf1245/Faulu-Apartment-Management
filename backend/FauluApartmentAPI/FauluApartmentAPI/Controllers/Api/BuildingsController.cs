using System.Security.Claims;
using FauluApartmentAPI.Data.Entities;
using FauluApartmentAPI.Models;
using FauluApartmentAPI.Models.Dtos;
using FauluApartmentAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FauluApartmentAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BuildingsController : ControllerBase
{
    private readonly IBuildingService _buildingService;
    private readonly ILogger<BuildingsController> _logger;

    public BuildingsController(IBuildingService buildingService, ILogger<BuildingsController> logger)
    {
        _buildingService = buildingService;
        _logger = logger;
    }

    // Reads the logged-in user's id from the JWT (nameidentifier claim) —
    // this is the ONLY trustworthy source of "who is asking". Never take
    // ownership from the request body; a client could send any OwnerId.
    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out var id) ? id : 0;
    }

    /// <summary>
    /// Payment history aggregated by month for a building
    /// </summary>
    [HttpGet("{id}/payments/history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetPaymentHistory(int id, [FromQuery] string from, [FromQuery] string to, [FromServices] IPaymentService paymentService, CancellationToken cancellationToken)
    {
        try
        {
            // Ownership check first — don't leak payment history for a
            // building that isn't this user's.
            var building = await _buildingService.GetBuildingByIdAsync(id, cancellationToken);
            if (building == null || building.OwnerId != GetCurrentUserId())
                return NotFound(ApiResponse<IEnumerable<object>>.ErrorResponse("Building not found"));

            // Parse from/to in YYYY-MM format
            if (!DateTime.TryParseExact(from + "-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.AssumeUniversal, out var fromDate))
                return BadRequest(ApiResponse<IEnumerable<object>>.ErrorResponse("Invalid 'from' parameter. Use YYYY-MM"));

            if (!DateTime.TryParseExact(to + "-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.AssumeUniversal, out var toDate))
                return BadRequest(ApiResponse<IEnumerable<object>>.ErrorResponse("Invalid 'to' parameter. Use YYYY-MM"));

            // normalize to month range (start of from to end of to month)
            var start = new DateTime(fromDate.Year, fromDate.Month, 1);
            var end = new DateTime(toDate.Year, toDate.Month, 1).AddMonths(1).AddTicks(-1);

            // Use repository via paymentService to access payments queryable
            // limit to the building's owner (current user) so the service returns
            // landlord-scoped payments
            var payments = await paymentService.GetAllPaymentsAsync(building.OwnerId, cancellationToken);

            var filtered = payments.Where(p => p.DueDate >= start && p.DueDate <= end && (p.Unit != null && p.Unit.BuildingId == id || p.Lease != null && p.Lease.UnitId != 0 && p.Unit != null && p.Unit.BuildingId == id || p.TenantId != null));

            var grouped = filtered.GroupBy(p => new { p.DueDate.Year, p.DueDate.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    period = $"{g.Key.Year:D4}-{g.Key.Month:D2}",
                    expected = g.Sum(p => p.Amount),
                    collected = g.Sum(p => p.PaidAmount ?? 0m),
                    outstanding = g.Sum(p => p.Amount - (p.PaidAmount ?? 0m)),
                    tenantsPaid = g.Count(p => (p.PaidAmount ?? 0m) >= p.Amount),
                    totalTenants = g.Select(p => p.TenantId).Distinct().Count()
                }).ToList();

            return Ok(ApiResponse<IEnumerable<object>>.SuccessResponse(grouped));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment history for building {BuildingId}", id);
            return StatusCode(500, ApiResponse<IEnumerable<object>>.ErrorResponse("An error occurred while retrieving payment history"));
        }
    }

    /// <summary>
    /// Get all buildings belonging to the logged-in user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<BuildingDto>>>> GetAllBuildings(CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var buildings = await _buildingService.GetAllBuildingsAsync(cancellationToken);

            // This is the actual fix: only return buildings this user owns,
            // instead of every building in the database.
            var ownBuildings = buildings.Where(b => b.OwnerId == currentUserId);

            var buildingDtos = ownBuildings.Select(b => new BuildingDto
            {
                Id = b.Id,
                Name = b.Name,
                Address = b.Address,
                City = b.City,
                PostalCode = b.PostalCode,
                Country = b.Country,
                Description = b.Description,
                TotalUnits = b.TotalUnits,
                OwnerId = b.OwnerId,
                PropertyValue = b.PropertyValue,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            }).ToList();

            return Ok(ApiResponse<IEnumerable<BuildingDto>>.SuccessResponse(buildingDtos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving buildings");
            return StatusCode(500, ApiResponse<IEnumerable<BuildingDto>>.ErrorResponse("An error occurred while retrieving buildings"));
        }
    }

    /// <summary>
    /// Get building by ID — only if it belongs to the logged-in user
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BuildingDto>>> GetBuildingById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var building = await _buildingService.GetBuildingByIdAsync(id, cancellationToken);

            // Treat "exists but isn't yours" the same as "doesn't exist" —
            // don't reveal that a building with this id belongs to someone else.
            if (building == null || building.OwnerId != GetCurrentUserId())
                return NotFound(ApiResponse<BuildingDto>.ErrorResponse("Building not found"));

            var occupancyRate = await _buildingService.GetOccupancyRateAsync(id, cancellationToken);
            var buildingDto = new BuildingDto
            {
                Id = building.Id,
                Name = building.Name,
                Address = building.Address,
                City = building.City,
                PostalCode = building.PostalCode,
                Country = building.Country,
                Description = building.Description,
                TotalUnits = building.TotalUnits,
                OwnerId = building.OwnerId,
                PropertyValue = building.PropertyValue,
                OccupancyRate = occupancyRate,
                CreatedAt = building.CreatedAt,
                UpdatedAt = building.UpdatedAt
            };

            return Ok(ApiResponse<BuildingDto>.SuccessResponse(buildingDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving building {BuildingId}", id);
            return StatusCode(500, ApiResponse<BuildingDto>.ErrorResponse("An error occurred while retrieving the building"));
        }
    }

    /// <summary>
    /// Create a new building, owned by the logged-in user
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<BuildingDto>>> CreateBuilding(
        [FromBody] CreateBuildingDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var building = new Building
            {
                Name = dto.Name,
                Address = dto.Address,
                City = dto.City,
                PostalCode = dto.PostalCode,
                Country = "Kenya",
                Description = dto.Description,
                TotalUnits = dto.TotalUnits,
                // Ownership comes from the authenticated user, never from the
                // request body — dto.OwnerId is ignored on purpose now.
                OwnerId = GetCurrentUserId(),
                PropertyValue = dto.PropertyValue,
                YearBuilt = dto.YearBuilt.HasValue ? new DateTime(dto.YearBuilt.Value, 1, 1) : null
            };

            var createdBuilding = await _buildingService.CreateBuildingAsync(building, cancellationToken);
            var buildingDto = new BuildingDto
            {
                Id = createdBuilding.Id,
                Name = createdBuilding.Name,
                Address = createdBuilding.Address,
                City = createdBuilding.City,
                PostalCode = createdBuilding.PostalCode,
                Country = createdBuilding.Country,
                Description = createdBuilding.Description,
                TotalUnits = createdBuilding.TotalUnits,
                OwnerId = createdBuilding.OwnerId,
                PropertyValue = createdBuilding.PropertyValue,
                CreatedAt = createdBuilding.CreatedAt,
                UpdatedAt = createdBuilding.UpdatedAt
            };

            return CreatedAtAction(nameof(GetBuildingById), new { id = createdBuilding.Id },
                ApiResponse<BuildingDto>.SuccessResponse(buildingDto, "Building created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating building");
            return BadRequest(ApiResponse<BuildingDto>.ErrorResponse("An error occurred while creating the building"));
        }
    }

    /// <summary>
    /// Update building — only if it belongs to the logged-in user
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BuildingDto>>> UpdateBuilding(
        int id, [FromBody] UpdateBuildingDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var building = await _buildingService.GetBuildingByIdAsync(id, cancellationToken);
            if (building == null || building.OwnerId != GetCurrentUserId())
                return NotFound(ApiResponse<BuildingDto>.ErrorResponse("Building not found"));

            if (!string.IsNullOrEmpty(dto.Name))
                building.Name = dto.Name;
            if (!string.IsNullOrEmpty(dto.Address))
                building.Address = dto.Address;
            if (!string.IsNullOrEmpty(dto.City))
                building.City = dto.City;
            if (!string.IsNullOrEmpty(dto.PostalCode))
                building.PostalCode = dto.PostalCode;
            if (!string.IsNullOrEmpty(dto.Description))
                building.Description = dto.Description;
            if (dto.TotalUnits.HasValue)
                building.TotalUnits = dto.TotalUnits.Value;
            if (dto.PropertyValue.HasValue)
                building.PropertyValue = dto.PropertyValue.Value;

            var updatedBuilding = await _buildingService.UpdateBuildingAsync(building, cancellationToken);
            var buildingDto = new BuildingDto
            {
                Id = updatedBuilding.Id,
                Name = updatedBuilding.Name,
                Address = updatedBuilding.Address,
                City = updatedBuilding.City,
                PostalCode = updatedBuilding.PostalCode,
                Country = updatedBuilding.Country,
                Description = updatedBuilding.Description,
                TotalUnits = updatedBuilding.TotalUnits,
                OwnerId = updatedBuilding.OwnerId,
                PropertyValue = updatedBuilding.PropertyValue,
                CreatedAt = updatedBuilding.CreatedAt,
                UpdatedAt = updatedBuilding.UpdatedAt
            };

            return Ok(ApiResponse<BuildingDto>.SuccessResponse(buildingDto, "Building updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating building {BuildingId}", id);
            return StatusCode(500, ApiResponse<BuildingDto>.ErrorResponse("An error occurred while updating the building"));
        }
    }

    /// <summary>
    /// Delete building — only if it belongs to the logged-in user
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteBuilding(int id, CancellationToken cancellationToken)
    {
        try
        {
            var building = await _buildingService.GetBuildingByIdAsync(id, cancellationToken);
            if (building == null || building.OwnerId != GetCurrentUserId())
                return NotFound(ApiResponse.ErrorResponse("Building not found"));

            await _buildingService.DeleteBuildingAsync(id, cancellationToken);
            return Ok(ApiResponse.SuccessResponse("Building deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting building {BuildingId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while deleting the building"));
        }
    }
}
