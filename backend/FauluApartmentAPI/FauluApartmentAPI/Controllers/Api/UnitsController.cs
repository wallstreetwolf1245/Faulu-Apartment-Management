using FauluApartmentAPI.Data.Entities;
using FauluApartmentAPI.Data.Repositories;
using FauluApartmentAPI.Models;
using FauluApartmentAPI.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FauluApartmentAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UnitsController : ControllerBase
{
    private readonly IUnitRepository _unitRepository;
    private readonly ILogger<UnitsController> _logger;

    public UnitsController(IUnitRepository unitRepository, ILogger<UnitsController> logger)
    {
        _unitRepository = unitRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all units
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<UnitDto>>>> GetAllUnits(CancellationToken cancellationToken)
    {
        try
        {
            var units = await _unitRepository.GetAllAsync(cancellationToken);
            var unitDtos = units.Select(u => MapToDto(u)).ToList();
            return Ok(ApiResponse<IEnumerable<UnitDto>>.SuccessResponse(unitDtos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all units");
            return StatusCode(500, ApiResponse<IEnumerable<UnitDto>>.ErrorResponse("An error occurred while retrieving units"));
        }
    }

    /// <summary>
    /// Get units by building
    /// </summary>
    [HttpGet("building/{buildingId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<UnitDto>>>> GetUnitsByBuilding(
        int buildingId, CancellationToken cancellationToken)
    {
        try
        {
            var units = await _unitRepository.GetUnitsByBuildingAsync(buildingId, cancellationToken);
            var unitDtos = units.Select(u => MapToDto(u)).ToList();
            return Ok(ApiResponse<IEnumerable<UnitDto>>.SuccessResponse(unitDtos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving units for building {BuildingId}", buildingId);
            return StatusCode(500, ApiResponse<IEnumerable<UnitDto>>.ErrorResponse("An error occurred while retrieving units"));
        }
    }

    /// <summary>
    /// Get unit by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UnitDto>>> GetUnitById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var unit = await _unitRepository.GetByIdAsync(id, cancellationToken);
            if (unit == null)
                return NotFound(ApiResponse<UnitDto>.ErrorResponse("Unit not found"));

            return Ok(ApiResponse<UnitDto>.SuccessResponse(MapToDto(unit)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unit {UnitId}", id);
            return StatusCode(500, ApiResponse<UnitDto>.ErrorResponse("An error occurred while retrieving the unit"));
        }
    }

    /// <summary>
    /// Get vacant units
    /// </summary>
    [HttpGet("building/{buildingId}/vacant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<UnitDto>>>> GetVacantUnits(
        int buildingId, CancellationToken cancellationToken)
    {
        try
        {
            var units = await _unitRepository.GetVacantUnitsAsync(buildingId, cancellationToken);
            var unitDtos = units.Select(u => MapToDto(u)).ToList();
            return Ok(ApiResponse<IEnumerable<UnitDto>>.SuccessResponse(unitDtos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vacant units");
            return StatusCode(500, ApiResponse<IEnumerable<UnitDto>>.ErrorResponse("An error occurred while retrieving vacant units"));
        }
    }

    /// <summary>
    /// Create unit
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<UnitDto>>> CreateUnit(
        [FromBody] CreateUnitDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var unit = new Unit
            {
                UnitNumber = dto.UnitNumber,
                UnitType = dto.UnitType,
                BuildingId = dto.BuildingId,
                FloorNumber = dto.FloorNumber,
                MonthlyRent = dto.MonthlyRent,
                Deposit = dto.Deposit,
                BedroomCount = dto.BedroomCount,
                BathroomCount = dto.BathroomCount,
                SquareFootage = dto.SquareFootage,
                IsFurnished = dto.IsFurnished,
                Amenities = dto.Amenities,
                Notes = dto.Notes,
                Status = "Vacant",
                CreatedAt = DateTime.UtcNow,
            };

            await _unitRepository.AddAsync(unit, cancellationToken);
            await _unitRepository.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<UnitDto>.SuccessResponse(MapToDto(unit), "Unit created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating unit: {Message} | Inner: {Inner}",
                ex.Message, ex.InnerException?.Message);

            if (ex.InnerException?.Message.Contains("duplicate key") == true ||
                ex.InnerException?.Message.Contains("UNIQUE") == true)
            {
                return BadRequest(ApiResponse<UnitDto>.ErrorResponse(
                    $"Unit number '{dto.UnitNumber}' already exists in this building."));
            }

            return BadRequest(ApiResponse<UnitDto>.ErrorResponse("An error occurred while creating the unit"));
        }
    }

    /// <summary>
    /// Update unit
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UnitDto>>> UpdateUnit(
        int id, [FromBody] UpdateUnitDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var unit = await _unitRepository.GetByIdAsync(id, cancellationToken);
            if (unit == null)
                return NotFound(ApiResponse<UnitDto>.ErrorResponse("Unit not found"));

            if (!string.IsNullOrEmpty(dto.UnitNumber))
                unit.UnitNumber = dto.UnitNumber;
            if (!string.IsNullOrEmpty(dto.UnitType))
                unit.UnitType = dto.UnitType;
            if (dto.MonthlyRent.HasValue)
                unit.MonthlyRent = dto.MonthlyRent.Value;
            if (dto.Deposit.HasValue)
                unit.Deposit = dto.Deposit.Value;
            if (dto.BedroomCount.HasValue)
                unit.BedroomCount = dto.BedroomCount.Value;
            if (dto.BathroomCount.HasValue)
                unit.BathroomCount = dto.BathroomCount.Value;
            if (dto.SquareFootage.HasValue)
                unit.SquareFootage = dto.SquareFootage.Value;
            if (dto.IsFurnished.HasValue)
                unit.IsFurnished = dto.IsFurnished.Value;
            if (!string.IsNullOrEmpty(dto.Status))
                unit.Status = dto.Status;
            if (!string.IsNullOrEmpty(dto.Amenities))
                unit.Amenities = dto.Amenities;
            if (!string.IsNullOrEmpty(dto.Notes))
                unit.Notes = dto.Notes;

            unit.UpdatedAt = DateTime.UtcNow;

            await _unitRepository.UpdateAsync(unit, cancellationToken);
            await _unitRepository.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<UnitDto>.SuccessResponse(MapToDto(unit), "Unit updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating unit {UnitId}: {Message} | Inner: {Inner}",
                id, ex.Message, ex.InnerException?.Message);

            return StatusCode(500, ApiResponse<UnitDto>.ErrorResponse("An error occurred while updating the unit"));
        }
    }

    /// <summary>
    /// Delete unit
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteUnit(int id, CancellationToken cancellationToken)
    {
        try
        {
            var unit = await _unitRepository.GetByIdAsync(id, cancellationToken);
            if (unit == null)
                return NotFound(ApiResponse.ErrorResponse("Unit not found"));

            await _unitRepository.DeleteAsync(id, cancellationToken);
            await _unitRepository.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse.SuccessResponse("Unit deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting unit {UnitId}: {Message} | Inner: {Inner}",
                id, ex.Message, ex.InnerException?.Message);

            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while deleting the unit"));
        }
    }

    private static UnitDto MapToDto(Unit unit)
    {
        return new UnitDto
        {
            Id = unit.Id,
            UnitNumber = unit.UnitNumber,
            UnitType = unit.UnitType,
            BuildingId = unit.BuildingId,
            FloorNumber = unit.FloorNumber,
            MonthlyRent = unit.MonthlyRent,
            Deposit = unit.Deposit,
            BedroomCount = unit.BedroomCount,
            BathroomCount = unit.BathroomCount,
            SquareFootage = unit.SquareFootage,
            IsFurnished = unit.IsFurnished,
            Status = unit.Status,
            Amenities = unit.Amenities,
            Notes = unit.Notes,
            CreatedAt = unit.CreatedAt,
            UpdatedAt = unit.UpdatedAt
        };
    }
}