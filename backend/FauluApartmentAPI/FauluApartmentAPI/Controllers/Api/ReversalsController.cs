
using FauluApartmentAPI.Models;
using FauluApartmentAPI.Models.Dtos;
using FauluApartmentAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FauluApartmentAPI.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReversalsController : ControllerBase
{
    private readonly IReversalService _reversalService;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<ReversalsController> _logger;

    public ReversalsController(IReversalService reversalService, IPaymentService paymentService, ILogger<ReversalsController> logger)
    {
        _reversalService = reversalService;
        _paymentService = paymentService;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out var id) ? id : 0;
    }

    [HttpPost("/api/payments/{paymentId}/reverse")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PaymentReversalDto>>> CreateReversal(int paymentId, [FromBody] CreateReversalDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var reversal = await _reversalService.CreateReversalAsync(paymentId, dto.Reason, cancellationToken);

            var payment = await _paymentService.GetPaymentByIdAsync(reversal.PaymentId, GetCurrentUserId(), cancellationToken);

            var tenantName = payment?.PayerName;

            var result = new PaymentReversalDto
            {
                Id = reversal.Id,
                PaymentId = reversal.PaymentId,
                TenantId = reversal.TenantId,
                BuildingId = reversal.BuildingId,
                Amount = reversal.Amount,
                Reason = reversal.Reason,
                Status = reversal.Status,
                RequestedAt = reversal.RequestedAt,
                PaymentReferenceId = reversal.PaymentReferenceId,
                TenantName = tenantName
            };

            return Ok(ApiResponse<PaymentReversalDto>.SuccessResponse(result, "Reversal request created"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error creating reversal for payment {PaymentId}", paymentId);
            return BadRequest(ApiResponse<PaymentReversalDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reversal for payment {PaymentId}", paymentId);
            return StatusCode(500, ApiResponse<PaymentReversalDto>.ErrorResponse("An error occurred while creating the reversal"));
        }
    }

    [HttpGet("/api/buildings/{buildingId}/reversals")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PaymentReversalDto>>>> GetReversalsForBuilding(int buildingId, CancellationToken cancellationToken)
    {
        try
        {
            var reversals = await _reversalService.GetReversalsByBuildingAsync(buildingId, cancellationToken);

            var list = reversals.Select(r => new PaymentReversalDto
            {
                Id = r.Id,
                PaymentId = r.PaymentId,
                TenantId = r.TenantId,
                BuildingId = r.BuildingId,
                Amount = r.Amount,
                Reason = r.Reason,
                Status = r.Status,
                RequestedAt = r.RequestedAt,
                ProcessedAt = r.ProcessedAt,
                PaymentReferenceId = r.PaymentReferenceId
            }).ToList();

            return Ok(ApiResponse<IEnumerable<PaymentReversalDto>>.SuccessResponse(list));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reversals for building {BuildingId}", buildingId);
            return StatusCode(500, ApiResponse<IEnumerable<PaymentReversalDto>>.ErrorResponse("An error occurred while retrieving reversals"));
        }
    }

    [HttpPatch("/api/reversals/{id}/status")]
    [Authorize(Policy = "Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PaymentReversalDto>>> UpdateStatus(int id, [FromBody] UpdateReversalStatusDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _reversalService.UpdateReversalStatusAsync(id, dto.Status, cancellationToken);

            var result = new PaymentReversalDto
            {
                Id = updated.Id,
                PaymentId = updated.PaymentId,
                TenantId = updated.TenantId,
                BuildingId = updated.BuildingId,
                Amount = updated.Amount,
                Reason = updated.Reason,
                Status = updated.Status,
                RequestedAt = updated.RequestedAt,
                ProcessedAt = updated.ProcessedAt,
                PaymentReferenceId = updated.PaymentReferenceId
            };

            return Ok(ApiResponse<PaymentReversalDto>.SuccessResponse(result, "Reversal status updated"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid status update for reversal {ReversalId}", id);
            return BadRequest(ApiResponse<PaymentReversalDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating reversal status {ReversalId}", id);
            return StatusCode(500, ApiResponse<PaymentReversalDto>.ErrorResponse("An error occurred while updating reversal status"));
        }
    }
}
