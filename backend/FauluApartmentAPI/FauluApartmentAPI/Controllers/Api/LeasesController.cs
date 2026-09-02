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
public class LeasesController : ControllerBase
{
    private readonly ILeaseService _leaseService;
    private readonly ILogger<LeasesController> _logger;

    public LeasesController(ILeaseService leaseService, ILogger<LeasesController> logger)
    {
        _leaseService = leaseService;
        _logger = logger;
    }

    /// <summary>
    /// Get lease by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<LeaseDto>>> GetLeaseById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var lease = await _leaseService.GetLeaseByIdAsync(id, cancellationToken);
            if (lease == null)
                return NotFound(ApiResponse<LeaseDto>.ErrorResponse("Lease not found"));

            return Ok(ApiResponse<LeaseDto>.SuccessResponse(MapToDto(lease)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lease {LeaseId}", id);
            return StatusCode(500, ApiResponse<LeaseDto>.ErrorResponse("An error occurred while retrieving the lease"));
        }
    }

    /// <summary>
    /// Get leases by tenant
    /// </summary>
    [HttpGet("tenant/{tenantId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<LeaseDto>>>> GetLeasesByTenant(
        int tenantId, CancellationToken cancellationToken)
    {
        try
        {
            var leases = await _leaseService.GetLeasesByTenantAsync(tenantId, cancellationToken);
            var leaseDtos = leases.Select(MapToDto).ToList();
            return Ok(ApiResponse<IEnumerable<LeaseDto>>.SuccessResponse(leaseDtos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leases for tenant {TenantId}", tenantId);
            return StatusCode(500, ApiResponse<IEnumerable<LeaseDto>>.ErrorResponse("An error occurred while retrieving leases"));
        }
    }

    /// <summary>
    /// Get leases by unit
    /// </summary>
    [HttpGet("unit/{unitId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<LeaseDto>>>> GetLeasesByUnit(
        int unitId, CancellationToken cancellationToken)
    {
        try
        {
            var leases = await _leaseService.GetLeasesByUnitAsync(unitId, cancellationToken);
            var leaseDtos = leases.Select(MapToDto).ToList();
            return Ok(ApiResponse<IEnumerable<LeaseDto>>.SuccessResponse(leaseDtos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leases for unit {UnitId}", unitId);
            return StatusCode(500, ApiResponse<IEnumerable<LeaseDto>>.ErrorResponse("An error occurred while retrieving leases"));
        }
    }

    /// <summary>
    /// Get expiring leases
    /// </summary>
    [HttpGet("expiring/{daysUntilExpiry}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<LeaseDto>>>> GetExpiringLeases(
        int daysUntilExpiry, CancellationToken cancellationToken)
    {
        try
        {
            var leases = await _leaseService.GetExpiringLeasesAsync(daysUntilExpiry, cancellationToken);
            var leaseDtos = leases.Select(MapToDto).ToList();
            return Ok(ApiResponse<IEnumerable<LeaseDto>>.SuccessResponse(leaseDtos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expiring leases");
            return StatusCode(500, ApiResponse<IEnumerable<LeaseDto>>.ErrorResponse("An error occurred while retrieving expiring leases"));
        }
    }

    /// <summary>
    /// Create lease
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<LeaseDto>>> CreateLease(
        [FromBody] CreateLeaseDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var lease = new Lease
            {
                TenantId = dto.TenantId,
                UnitId = dto.UnitId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                MonthlyRent = dto.MonthlyRent,
                DepositAmount = dto.DepositAmount,
                SecondaryDeposit = dto.SecondaryDeposit,
                Status = "Active",
                LeaseTermType = dto.LeaseTermType,
                RenewalPolicy = dto.RenewalPolicy,
                AllowsPets = dto.AllowsPets,
                LeaseDocument = dto.LeaseDocument,
                SignedDate = DateTime.UtcNow
            };

            var createdLease = await _leaseService.CreateLeaseAsync(lease, cancellationToken);
            return CreatedAtAction(nameof(GetLeaseById), new { id = createdLease.Id },
                ApiResponse<LeaseDto>.SuccessResponse(MapToDto(createdLease), "Lease created successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Lease creation validation error");
            return BadRequest(ApiResponse<LeaseDto>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Lease creation error");
            return BadRequest(ApiResponse<LeaseDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating lease");
            return StatusCode(500, ApiResponse<LeaseDto>.ErrorResponse("An error occurred while creating the lease"));
        }
    }

    /// <summary>
    /// Update lease
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<LeaseDto>>> UpdateLease(
        int id, [FromBody] UpdateLeaseDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var lease = await _leaseService.GetLeaseByIdAsync(id, cancellationToken);
            if (lease == null)
                return NotFound(ApiResponse<LeaseDto>.ErrorResponse("Lease not found"));

            if (dto.EndDate.HasValue)
                lease.EndDate = dto.EndDate.Value;
            if (dto.MonthlyRent.HasValue)
                lease.MonthlyRent = dto.MonthlyRent.Value;
            if (!string.IsNullOrEmpty(dto.Status))
                lease.Status = dto.Status;
            if (!string.IsNullOrEmpty(dto.RenewalPolicy))
                lease.RenewalPolicy = dto.RenewalPolicy;
            if (dto.AllowsPets.HasValue)
                lease.AllowsPets = dto.AllowsPets.Value;
            if (!string.IsNullOrEmpty(dto.Notes))
                lease.Notes = dto.Notes;

            var updatedLease = await _leaseService.UpdateLeaseAsync(lease, cancellationToken);
            return Ok(ApiResponse<LeaseDto>.SuccessResponse(MapToDto(updatedLease), "Lease updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lease {LeaseId}", id);
            return StatusCode(500, ApiResponse<LeaseDto>.ErrorResponse("An error occurred while updating the lease"));
        }
    }

    /// <summary>
    /// Renew lease
    /// </summary>
    [HttpPost("{id}/renew")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<LeaseDto>>> RenewLease(
        int id, [FromBody] RenewLeaseRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var renewedLease = await _leaseService.RenewLeaseAsync(id, request.NewEndDate, cancellationToken);
            return Ok(ApiResponse<LeaseDto>.SuccessResponse(MapToDto(renewedLease), "Lease renewed successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Lease renewal error");
            return NotFound(ApiResponse<LeaseDto>.ErrorResponse(ex.Message));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Lease renewal validation error");
            return BadRequest(ApiResponse<LeaseDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error renewing lease {LeaseId}", id);
            return StatusCode(500, ApiResponse<LeaseDto>.ErrorResponse("An error occurred while renewing the lease"));
        }
    }

    /// <summary>
    /// Terminate lease
    /// </summary>
    [HttpPost("{id}/terminate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> TerminateLease(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _leaseService.TerminateLeaseAsync(id, cancellationToken);
            return Ok(ApiResponse.SuccessResponse("Lease terminated successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Lease termination error");
            return NotFound(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error terminating lease {LeaseId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while terminating the lease"));
        }
    }

    private static LeaseDto MapToDto(Lease lease)
    {
        return new LeaseDto
        {
            Id = lease.Id,
            TenantId = lease.TenantId,
            UnitId = lease.UnitId,
            StartDate = lease.StartDate,
            EndDate = lease.EndDate,
            MonthlyRent = lease.MonthlyRent,
            DepositAmount = lease.DepositAmount,
            SecondaryDeposit = lease.SecondaryDeposit,
            Status = lease.Status,
            LeaseTermType = lease.LeaseTermType,
            RenewalPolicy = lease.RenewalPolicy,
            AllowsPets = lease.AllowsPets,
            LeaseDocument = lease.LeaseDocument,
            SignedDate = lease.SignedDate,
            Notes = lease.Notes,
            CreatedAt = lease.CreatedAt,
            UpdatedAt = lease.UpdatedAt
        };
    }
}

public class RenewLeaseRequest
{
    public DateTime NewEndDate { get; set; }
}
