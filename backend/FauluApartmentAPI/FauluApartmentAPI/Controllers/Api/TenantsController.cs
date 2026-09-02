using System.Security.Claims;
using FauluApartmentAPI.Data.Entities;
using FauluApartmentAPI.Data.Repositories;
using FauluApartmentAPI.Models;
using FauluApartmentAPI.Models.Dtos;
using FauluApartmentAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FauluApartmentAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TenantsController : ControllerBase
{
    private readonly ITenantService _tenantService;
    private readonly IRepository<Lease> _leaseRepository;
    private readonly IRepository<Unit> _unitRepository;
    private readonly ILogger<TenantsController> _logger;

    public TenantsController(
        ITenantService tenantService,
        IRepository<Lease> leaseRepository,
        IRepository<Unit> unitRepository,
        ILogger<TenantsController> logger)
    {
        _tenantService = tenantService;
        _leaseRepository = leaseRepository;
        _unitRepository = unitRepository;
        _logger = logger;
    }

    // Pulled from the JWT claims on every request — never trust a client-supplied
    // landlord/building id for authorization decisions.
    private int CurrentLandlordId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<TenantDto>>>> GetAllTenants(CancellationToken cancellationToken)
    {
        try
        {
            var tenants = await _tenantService.GetAllTenantsAsync(CurrentLandlordId, cancellationToken);
            var tenantDtos = tenants.Select(MapToDto).ToList();
            return Ok(ApiResponse<IEnumerable<TenantDto>>.SuccessResponse(tenantDtos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tenants");
            return StatusCode(500, ApiResponse<IEnumerable<TenantDto>>.ErrorResponse("An error occurred while retrieving tenants"));
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TenantDto>>> GetTenantById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await _tenantService.GetTenantByIdAsync(id, CurrentLandlordId, cancellationToken);
            if (tenant == null)
                return NotFound(ApiResponse<TenantDto>.ErrorResponse("Tenant not found"));

            return Ok(ApiResponse<TenantDto>.SuccessResponse(MapToDto(tenant)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tenant {TenantId}", id);
            return StatusCode(500, ApiResponse<TenantDto>.ErrorResponse("An error occurred while retrieving the tenant"));
        }
    }

    [HttpGet("search/{name}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<TenantDto>>>> SearchTenants(
        string name, CancellationToken cancellationToken)
    {
        try
        {
            var tenants = await _tenantService.SearchTenantsByNameAsync(name, CurrentLandlordId, cancellationToken);
            var tenantDtos = tenants.Select(MapToDto).ToList();
            return Ok(ApiResponse<IEnumerable<TenantDto>>.SuccessResponse(tenantDtos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching tenants");
            return StatusCode(500, ApiResponse<IEnumerable<TenantDto>>.ErrorResponse("An error occurred while searching tenants"));
        }
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<TenantDto>>> CreateTenant(
        [FromBody] CreateTenantDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var tenant = new Tenant
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                IdentificationNumber = dto.IdentificationNumber,
                IdentificationType = dto.IdentificationType,
                DateOfBirth = dto.DateOfBirth,
                Occupation = dto.Occupation,
                Employer = dto.Employer,
                EmergencyContactName = dto.EmergencyContactName,
                EmergencyContactPhone = dto.EmergencyContactPhone,
                EmergencyContactRelation = dto.EmergencyContactRelation,
                Status = "Active"
            };

            var createdTenant = await _tenantService.CreateTenantAsync(tenant, cancellationToken);

            // TODO (follow-up, not part of the cross-user leak fix): verify dto.UnitId
            // belongs to a building owned by CurrentLandlordId before creating the lease,
            // otherwise a landlord could attach a tenant to another landlord's unit.
            if (dto.UnitId.HasValue && dto.MoveInDate.HasValue && dto.RentAmount.HasValue)
            {
                var lease = new Lease
                {
                    TenantId = createdTenant.Id,
                    UnitId = dto.UnitId.Value,
                    StartDate = dto.MoveInDate.Value,
                    EndDate = dto.MoveInDate.Value.AddMonths(12),
                    MonthlyRent = dto.RentAmount.Value,
                    Status = "Active"
                };

                await _leaseRepository.AddAsync(lease, cancellationToken);

                var newUnit = await _unitRepository.GetByIdAsync(dto.UnitId.Value, cancellationToken);
                if (newUnit != null)
                {
                    newUnit.Status = "Occupied";
                    await _unitRepository.UpdateAsync(newUnit, cancellationToken);
                }
                else
                {
                    _logger.LogWarning("CreateTenant: Unit {UnitId} not found when marking Occupied", dto.UnitId.Value);
                }

                await _leaseRepository.SaveChangesAsync(cancellationToken);
            }
            else
            {
                _logger.LogWarning(
                    "Tenant {TenantId} created without UnitId/MoveInDate/RentAmount — no lease created",
                    createdTenant.Id);
            }

            var tenantWithLease = await _tenantService.GetTenantByIdAsync(createdTenant.Id, CurrentLandlordId, cancellationToken);

            return Ok(ApiResponse<TenantDto>.SuccessResponse(MapToDto(tenantWithLease!), "Tenant created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Tenant creation validation error");
            return BadRequest(ApiResponse<TenantDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tenant");
            return BadRequest(ApiResponse<TenantDto>.ErrorResponse("An error occurred while creating the tenant"));
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TenantDto>>> UpdateTenant(
        int id, [FromBody] UpdateTenantDto dto, CancellationToken cancellationToken)
    {
        try
        {
            // SCOPED: previously fetched by bare id — a landlord could edit any
            // tenant in the system by guessing an id. Now returns null (-> 404)
            // if the tenant isn't owned by the caller.
            var tenant = await _tenantService.GetTenantByIdAsync(id, CurrentLandlordId, cancellationToken);
            if (tenant == null)
                return NotFound(ApiResponse<TenantDto>.ErrorResponse("Tenant not found"));

            if (!string.IsNullOrEmpty(dto.Email)) tenant.Email = dto.Email;
            if (!string.IsNullOrEmpty(dto.PhoneNumber)) tenant.PhoneNumber = dto.PhoneNumber;
            if (!string.IsNullOrEmpty(dto.Occupation)) tenant.Occupation = dto.Occupation;
            if (!string.IsNullOrEmpty(dto.Employer)) tenant.Employer = dto.Employer;
            if (!string.IsNullOrEmpty(dto.EmergencyContactName)) tenant.EmergencyContactName = dto.EmergencyContactName;
            if (!string.IsNullOrEmpty(dto.EmergencyContactPhone)) tenant.EmergencyContactPhone = dto.EmergencyContactPhone;
            if (!string.IsNullOrEmpty(dto.EmergencyContactRelation)) tenant.EmergencyContactRelation = dto.EmergencyContactRelation;
            if (!string.IsNullOrEmpty(dto.Status)) tenant.Status = dto.Status;
            if (!string.IsNullOrEmpty(dto.Notes)) tenant.Notes = dto.Notes;

            var activeLease = tenant.Leases?.FirstOrDefault(l => l.Status == "Active");

            if (dto.UnitId.HasValue)
            {
                // TODO (follow-up): verify dto.UnitId belongs to a building owned by
                // CurrentLandlordId before reassigning, same as CreateTenant above.
                if (activeLease == null)
                {
                    await AssignNewLeaseAsync(tenant.Id, dto.UnitId.Value, dto.RentAmount, dto.MoveInDate, cancellationToken);
                }
                else if (activeLease.UnitId != dto.UnitId.Value)
                {
                    activeLease.Status = "Completed";
                    activeLease.EndDate = DateTime.UtcNow;
                    await _leaseRepository.UpdateAsync(activeLease, cancellationToken);

                    var oldUnit = await _unitRepository.GetByIdAsync(activeLease.UnitId, cancellationToken);
                    if (oldUnit != null)
                    {
                        oldUnit.Status = "Vacant";
                        await _unitRepository.UpdateAsync(oldUnit, cancellationToken);
                    }
                    else
                    {
                        _logger.LogWarning("UpdateTenant: old Unit {UnitId} not found when vacating", activeLease.UnitId);
                    }

                    await AssignNewLeaseAsync(tenant.Id, dto.UnitId.Value, dto.RentAmount, dto.MoveInDate, cancellationToken);
                }
                else
                {
                    if (dto.RentAmount.HasValue) activeLease.MonthlyRent = dto.RentAmount.Value;
                    if (dto.MoveInDate.HasValue) activeLease.StartDate = dto.MoveInDate.Value;
                    await _leaseRepository.UpdateAsync(activeLease, cancellationToken);
                }
            }
            else if (activeLease != null && (dto.RentAmount.HasValue || dto.MoveInDate.HasValue))
            {
                if (dto.RentAmount.HasValue) activeLease.MonthlyRent = dto.RentAmount.Value;
                if (dto.MoveInDate.HasValue) activeLease.StartDate = dto.MoveInDate.Value;
                await _leaseRepository.UpdateAsync(activeLease, cancellationToken);
            }

            await _leaseRepository.SaveChangesAsync(cancellationToken);

            var updatedTenant = await _tenantService.UpdateTenantAsync(tenant, cancellationToken);

            var tenantWithLease = await _tenantService.GetTenantByIdAsync(updatedTenant.Id, CurrentLandlordId, cancellationToken);

            return Ok(ApiResponse<TenantDto>.SuccessResponse(MapToDto(tenantWithLease ?? updatedTenant), "Tenant updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tenant {TenantId}", id);
            return StatusCode(500, ApiResponse<TenantDto>.ErrorResponse("An error occurred while updating the tenant"));
        }
    }

    private async Task AssignNewLeaseAsync(
        int tenantId, int unitId, decimal? rentAmount, DateTime? moveInDate, CancellationToken cancellationToken)
    {
        var newUnit = await _unitRepository.GetByIdAsync(unitId, cancellationToken);
        var startDate = moveInDate ?? DateTime.UtcNow;

        var newLease = new Lease
        {
            TenantId = tenantId,
            UnitId = unitId,
            StartDate = startDate,
            EndDate = startDate.AddMonths(12),
            MonthlyRent = rentAmount ?? newUnit?.MonthlyRent ?? 0,
            Status = "Active"
        };

        await _leaseRepository.AddAsync(newLease, cancellationToken);

        if (newUnit != null)
        {
            newUnit.Status = "Occupied";
            await _unitRepository.UpdateAsync(newUnit, cancellationToken);
        }
        else
        {
            _logger.LogWarning("AssignNewLeaseAsync: new Unit {UnitId} not found when marking Occupied", unitId);
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteTenant(int id, CancellationToken cancellationToken)
    {
        try
        {
            // SCOPED: previously fetched by bare id — a landlord could delete any
            // tenant in the system by guessing an id.
            var tenant = await _tenantService.GetTenantByIdAsync(id, CurrentLandlordId, cancellationToken);
            if (tenant == null)
                return NotFound(ApiResponse.ErrorResponse("Tenant not found"));

            await _tenantService.DeleteTenantAsync(id, cancellationToken);
            return Ok(ApiResponse.SuccessResponse("Tenant deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tenant {TenantId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while deleting the tenant"));
        }
    }

    private static TenantDto MapToDto(Tenant tenant)
    {
        var activeLease = tenant.Leases?.FirstOrDefault(l => l.Status == "Active");
        var unit = activeLease?.Unit;
        var building = unit?.Building;

        return new TenantDto
        {
            Id = tenant.Id,
            FirstName = tenant.FirstName,
            LastName = tenant.LastName,
            Email = tenant.Email,
            PhoneNumber = tenant.PhoneNumber,
            IdentificationNumber = tenant.IdentificationNumber,
            IdentificationType = tenant.IdentificationType,
            DateOfBirth = tenant.DateOfBirth,
            Occupation = tenant.Occupation,
            Employer = tenant.Employer,
            EmergencyContactName = tenant.EmergencyContactName,
            EmergencyContactPhone = tenant.EmergencyContactPhone,
            EmergencyContactRelation = tenant.EmergencyContactRelation,
            Status = tenant.Status,
            Notes = tenant.Notes,
            CreatedAt = tenant.CreatedAt,
            UpdatedAt = tenant.UpdatedAt,
            ActiveLeaseId = activeLease?.Id,
            UnitId = unit?.Id,
            UnitNumber = unit?.UnitNumber,
            UnitType = unit?.UnitType,
            BuildingId = building?.Id,
            PropertyName = building?.Name,
            RentAmount = activeLease?.MonthlyRent,
            MoveInDate = activeLease?.StartDate,
            LeaseEndDate = activeLease?.EndDate,
            LeaseStatus = activeLease?.Status
        };
    }
}