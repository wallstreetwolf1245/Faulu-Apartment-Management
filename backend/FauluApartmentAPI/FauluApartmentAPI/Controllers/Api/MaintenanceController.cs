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
public class MaintenanceController : ControllerBase
{
    private readonly IMaintenanceService _maintenanceService;
    private readonly IBuildingService _buildingService;
    private readonly ILogger<MaintenanceController> _logger;

    public MaintenanceController(IMaintenanceService maintenanceService, IBuildingService buildingService, ILogger<MaintenanceController> logger)
    {
        _maintenanceService = maintenanceService;
        _buildingService = buildingService;
        _logger = logger;
    }

    // Reads the logged-in user's id from the JWT (nameidentifier claim) —
    // same pattern as BuildingsController. Never trust an OwnerId/UserId
    // sent in the request body.
    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out var id) ? id : 0;
    }

    /// <summary>
    /// Get all maintenance orders for buildings owned by the logged-in user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<MaintenanceOrderDto>>>> GetAllOrders(CancellationToken cancellationToken)
    {
        try
        {
            var orders = await _maintenanceService.GetOpenOrdersForOwnerAsync(GetCurrentUserId(), cancellationToken);
            var orderDtos = orders.Select(MapToDto).ToList();
            return Ok(ApiResponse<IEnumerable<MaintenanceOrderDto>>.SuccessResponse(orderDtos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all maintenance orders");
            return StatusCode(500, ApiResponse<IEnumerable<MaintenanceOrderDto>>.ErrorResponse("An error occurred while retrieving maintenance orders"));
        }
    }

    /// <summary>
    /// Get maintenance order by ID — only if it belongs to a building the logged-in user owns
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<MaintenanceOrderDto>>> GetOrderById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _maintenanceService.GetOrderByIdAsync(id, cancellationToken);
            if (order == null || !await IsOwnedByCurrentUserAsync(order.BuildingId, cancellationToken))
                return NotFound(ApiResponse<MaintenanceOrderDto>.ErrorResponse("Maintenance order not found"));

            return Ok(ApiResponse<MaintenanceOrderDto>.SuccessResponse(MapToDto(order)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving maintenance order {OrderId}", id);
            return StatusCode(500, ApiResponse<MaintenanceOrderDto>.ErrorResponse("An error occurred while retrieving the maintenance order"));
        }
    }

    /// <summary>
    /// Get maintenance orders by building — only if the building belongs to the logged-in user
    /// </summary>
    [HttpGet("building/{buildingId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IEnumerable<MaintenanceOrderDto>>>> GetOrdersByBuilding(
        int buildingId, CancellationToken cancellationToken)
    {
        try
        {
            if (!await IsOwnedByCurrentUserAsync(buildingId, cancellationToken))
                return NotFound(ApiResponse<IEnumerable<MaintenanceOrderDto>>.ErrorResponse("Building not found"));

            var orders = await _maintenanceService.GetOrdersByBuildingAsync(buildingId, cancellationToken);
            var orderDtos = orders.Select(MapToDto).ToList();
            return Ok(ApiResponse<IEnumerable<MaintenanceOrderDto>>.SuccessResponse(orderDtos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving maintenance orders for building {BuildingId}", buildingId);
            return StatusCode(500, ApiResponse<IEnumerable<MaintenanceOrderDto>>.ErrorResponse("An error occurred while retrieving maintenance orders"));
        }
    }

    /// <summary>
    /// Get open maintenance orders for buildings owned by the logged-in user
    /// </summary>
    [HttpGet("status/open")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<MaintenanceOrderDto>>>> GetOpenOrders(CancellationToken cancellationToken)
    {
        try
        {
            var orders = await _maintenanceService.GetOpenOrdersForOwnerAsync(GetCurrentUserId(), cancellationToken);
            var orderDtos = orders.Select(MapToDto).ToList();
            return Ok(ApiResponse<IEnumerable<MaintenanceOrderDto>>.SuccessResponse(orderDtos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving open maintenance orders");
            return StatusCode(500, ApiResponse<IEnumerable<MaintenanceOrderDto>>.ErrorResponse("An error occurred while retrieving open maintenance orders"));
        }
    }

    /// <summary>
    /// Get urgent maintenance orders for buildings owned by the logged-in user
    /// </summary>
    [HttpGet("priority/urgent")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<MaintenanceOrderDto>>>> GetUrgentOrders(CancellationToken cancellationToken)
    {
        try
        {
            var orders = await _maintenanceService.GetUrgentOrdersForOwnerAsync(GetCurrentUserId(), cancellationToken);
            var orderDtos = orders.Select(MapToDto).ToList();
            return Ok(ApiResponse<IEnumerable<MaintenanceOrderDto>>.SuccessResponse(orderDtos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving urgent maintenance orders");
            return StatusCode(500, ApiResponse<IEnumerable<MaintenanceOrderDto>>.ErrorResponse("An error occurred while retrieving urgent maintenance orders"));
        }
    }

    /// <summary>
    /// Create maintenance order — only against a building the logged-in user owns
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<MaintenanceOrderDto>>> CreateOrder(
        [FromBody] CreateMaintenanceOrderDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (!await IsOwnedByCurrentUserAsync(dto.BuildingId, cancellationToken))
                return BadRequest(ApiResponse<MaintenanceOrderDto>.ErrorResponse("Building not found"));

            var order = new MaintenanceOrder
            {
                BuildingId = dto.BuildingId,
                UnitId = dto.UnitId,
                TenantId = dto.TenantId,
                Title = dto.Title,
                Description = dto.Description,
                Priority = dto.Priority,
                Category = dto.Category,
                EstimatedCost = dto.EstimatedCost,
                AssignedVendor = dto.AssignedVendor,
                Status = "Open",
                ReportedDate = DateTime.UtcNow
            };

            var createdOrder = await _maintenanceService.CreateOrderAsync(order, cancellationToken);
            return CreatedAtAction(nameof(GetOrderById), new { id = createdOrder.Id },
                ApiResponse<MaintenanceOrderDto>.SuccessResponse(MapToDto(createdOrder), "Maintenance order created successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Maintenance order creation validation error");
            return BadRequest(ApiResponse<MaintenanceOrderDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating maintenance order");
            return StatusCode(500, ApiResponse<MaintenanceOrderDto>.ErrorResponse("An error occurred while creating the maintenance order"));
        }
    }

    /// <summary>
    /// Update maintenance order — only if it belongs to a building the logged-in user owns
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<MaintenanceOrderDto>>> UpdateOrder(
        int id, [FromBody] UpdateMaintenanceOrderDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _maintenanceService.GetOrderByIdAsync(id, cancellationToken);
            if (order == null || !await IsOwnedByCurrentUserAsync(order.BuildingId, cancellationToken))
                return NotFound(ApiResponse<MaintenanceOrderDto>.ErrorResponse("Maintenance order not found"));

            if (!string.IsNullOrEmpty(dto.Title))
                order.Title = dto.Title;
            if (!string.IsNullOrEmpty(dto.Description))
                order.Description = dto.Description;
            if (!string.IsNullOrEmpty(dto.Priority))
                order.Priority = dto.Priority;
            if (!string.IsNullOrEmpty(dto.Status))
                order.Status = dto.Status;
            if (!string.IsNullOrEmpty(dto.Category))
                order.Category = dto.Category;
            if (dto.ScheduledDate.HasValue)
                order.ScheduledDate = dto.ScheduledDate.Value;
            if (dto.ActualCost.HasValue)
                order.ActualCost = dto.ActualCost.Value;
            if (!string.IsNullOrEmpty(dto.AssignedVendor))
                order.AssignedVendor = dto.AssignedVendor;
            if (!string.IsNullOrEmpty(dto.Notes))
                order.Notes = dto.Notes;

            var updatedOrder = await _maintenanceService.UpdateOrderAsync(order, cancellationToken);
            return Ok(ApiResponse<MaintenanceOrderDto>.SuccessResponse(MapToDto(updatedOrder), "Maintenance order updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating maintenance order {OrderId}", id);
            return StatusCode(500, ApiResponse<MaintenanceOrderDto>.ErrorResponse("An error occurred while updating the maintenance order"));
        }
    }

    /// <summary>
    /// Complete maintenance order — only if it belongs to a building the logged-in user owns
    /// </summary>
    [HttpPost("{id}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<MaintenanceOrderDto>>> CompleteOrder(int id, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _maintenanceService.GetOrderByIdAsync(id, cancellationToken);
            if (order == null || !await IsOwnedByCurrentUserAsync(order.BuildingId, cancellationToken))
                return NotFound(ApiResponse<MaintenanceOrderDto>.ErrorResponse("Maintenance order not found"));

            var completedOrder = await _maintenanceService.CompleteOrderAsync(id, cancellationToken);
            return Ok(ApiResponse<MaintenanceOrderDto>.SuccessResponse(MapToDto(completedOrder), "Maintenance order completed successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Maintenance order completion error");
            return NotFound(ApiResponse<MaintenanceOrderDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing maintenance order {OrderId}", id);
            return StatusCode(500, ApiResponse<MaintenanceOrderDto>.ErrorResponse("An error occurred while completing the maintenance order"));
        }
    }

    /// <summary>
    /// Delete maintenance order — only if it belongs to a building the logged-in user owns
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteOrder(int id, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _maintenanceService.GetOrderByIdAsync(id, cancellationToken);
            if (order == null || !await IsOwnedByCurrentUserAsync(order.BuildingId, cancellationToken))
                return NotFound(ApiResponse.ErrorResponse("Maintenance order not found"));

            await _maintenanceService.DeleteOrderAsync(id, cancellationToken);
            return Ok(ApiResponse.SuccessResponse("Maintenance order deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting maintenance order {OrderId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while deleting the maintenance order"));
        }
    }

    // Centralized ownership check — a building "counts" as owned only if it
    // exists AND its OwnerId matches the logged-in user. Reused by every
    // action above so the rule can't drift between endpoints.
    private async Task<bool> IsOwnedByCurrentUserAsync(int buildingId, CancellationToken cancellationToken)
    {
        var building = await _buildingService.GetBuildingByIdAsync(buildingId, cancellationToken);
        return building != null && building.OwnerId == GetCurrentUserId();
    }

    private static MaintenanceOrderDto MapToDto(MaintenanceOrder order)
    {
        return new MaintenanceOrderDto
        {
            Id = order.Id,
            BuildingId = order.BuildingId,
            UnitId = order.UnitId,
            TenantId = order.TenantId,
            Title = order.Title,
            Description = order.Description,
            Priority = order.Priority,
            Status = order.Status,
            Category = order.Category,
            ReportedDate = order.ReportedDate,
            ScheduledDate = order.ScheduledDate,
            CompletedDate = order.CompletedDate,
            EstimatedCost = order.EstimatedCost,
            ActualCost = order.ActualCost,
            AssignedVendor = order.AssignedVendor,
            Notes = order.Notes,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt
        };
    }
}