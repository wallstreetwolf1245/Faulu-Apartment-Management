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
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;
    private readonly ITransactionStatusLogRepository _txLogRepository;
    public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger, ITransactionStatusLogRepository txLogRepository)
    {
        _paymentService = paymentService;
        _logger = logger;
        _txLogRepository = txLogRepository;
    }

    [HttpGet("{paymentId}/transactionstatus")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetTransactionStatusByPayment(int paymentId, CancellationToken cancellationToken)
    {
        try
        {
            var payment = await _paymentService.GetPaymentByIdAsync(paymentId, CurrentLandlordId, cancellationToken);
            if (payment == null)
                return NotFound(ApiResponse<IEnumerable<object>>.ErrorResponse("Payment not found"));

            var logs = await _txLogRepository.GetByPaymentIdAsync(paymentId, cancellationToken);
            var result = logs.Select(l => new {
                id = l.Id,
                transactionId = l.TransactionId,
                receipt = l.ReceiptNumber,
                amount = l.Amount,
                msisdn = l.Msisdn,
                receivedAt = l.ReceivedAt,
                raw = l.RawPayload
            });

            return Ok(ApiResponse<IEnumerable<object>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transaction status logs for payment {PaymentId}", paymentId);
            return StatusCode(500, ApiResponse<IEnumerable<object>>.ErrorResponse("An error occurred while retrieving transaction status logs"));
        }
    }

    // Pulled from the JWT claims on every request — never trust a client-supplied
    // landlord/building id for authorization decisions. Same pattern as
    // TenantsController.CurrentLandlordId.
    private int CurrentLandlordId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PaymentDto>>>> GetAllPayments(CancellationToken cancellationToken)
    {
        try
        {
            var payments = await _paymentService.GetAllPaymentsAsync(CurrentLandlordId, cancellationToken);
            var paymentDtos = payments.Select(MapToDto).ToList();
            return Ok(ApiResponse<IEnumerable<PaymentDto>>.SuccessResponse(paymentDtos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all payments");
            return StatusCode(500, ApiResponse<IEnumerable<PaymentDto>>.ErrorResponse("An error occurred while retrieving payments"));
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> GetPaymentById(int id, CancellationToken cancellationToken)
    {
        try
        {
            // SCOPED: previously fetched by bare id — any authenticated landlord
            // could view any payment in the system by guessing an id.
            var payment = await _paymentService.GetPaymentByIdAsync(id, CurrentLandlordId, cancellationToken);
            if (payment == null)
                return NotFound(ApiResponse<PaymentDto>.ErrorResponse("Payment not found"));

            return Ok(ApiResponse<PaymentDto>.SuccessResponse(MapToDto(payment)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment {PaymentId}", id);
            return StatusCode(500, ApiResponse<PaymentDto>.ErrorResponse("An error occurred while retrieving the payment"));
        }
    }

    [HttpGet("tenant/{tenantId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PaymentDto>>>> GetPaymentsByTenant(
        int tenantId, CancellationToken cancellationToken)
    {
        try
        {
            var payments = await _paymentService.GetPaymentsByTenantAsync(tenantId, CurrentLandlordId, cancellationToken);
            var paymentDtos = payments.Select(MapToDto).ToList();
            return Ok(ApiResponse<IEnumerable<PaymentDto>>.SuccessResponse(paymentDtos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payments for tenant {TenantId}", tenantId);
            return StatusCode(500, ApiResponse<IEnumerable<PaymentDto>>.ErrorResponse("An error occurred while retrieving payments"));
        }
    }

    [HttpGet("lease/{leaseId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PaymentDto>>>> GetPaymentsByLease(
        int leaseId, CancellationToken cancellationToken)
    {
        try
        {
            var payments = await _paymentService.GetPaymentsByLeaseAsync(leaseId, CurrentLandlordId, cancellationToken);
            var paymentDtos = payments.Select(MapToDto).ToList();
            return Ok(ApiResponse<IEnumerable<PaymentDto>>.SuccessResponse(paymentDtos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payments for lease {LeaseId}", leaseId);
            return StatusCode(500, ApiResponse<IEnumerable<PaymentDto>>.ErrorResponse("An error occurred while retrieving payments"));
        }
    }

    [HttpGet("status/overdue")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PaymentDto>>>> GetOverduePayments(CancellationToken cancellationToken)
    {
        try
        {
            var payments = await _paymentService.GetOverduePaymentsAsync(CurrentLandlordId, cancellationToken);
            var paymentDtos = payments.Select(MapToDto).ToList();
            return Ok(ApiResponse<IEnumerable<PaymentDto>>.SuccessResponse(paymentDtos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overdue payments");
            return StatusCode(500, ApiResponse<IEnumerable<PaymentDto>>.ErrorResponse("An error occurred while retrieving overdue payments"));
        }
    }

    [HttpGet("tenant/{tenantId}/outstanding")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<decimal>>> GetTotalOutstanding(
        int tenantId, CancellationToken cancellationToken)
    {
        try
        {
            var outstanding = await _paymentService.GetTotalOutstandingAsync(tenantId, CurrentLandlordId, cancellationToken);
            return Ok(ApiResponse<decimal>.SuccessResponse(outstanding));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating outstanding balance for tenant {TenantId}", tenantId);
            return StatusCode(500, ApiResponse<decimal>.ErrorResponse("An error occurred while calculating the outstanding balance"));
        }
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> CreatePayment(
        [FromBody] CreatePaymentDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var payment = new Payment
            {
                TenantId = dto.TenantId,
                LeaseId = dto.LeaseId,
                Amount = dto.Amount,
                DueDate = dto.DueDate,
                PaymentType = dto.PaymentType,
                PaymentMethod = dto.PaymentMethod,
                Notes = dto.Notes,
                Status = "Pending",
                PaymentSource = "Manual"
            };

            var createdPayment = await _paymentService.CreatePaymentAsync(payment, CurrentLandlordId, cancellationToken);
            return Ok(ApiResponse<PaymentDto>.SuccessResponse(MapToDto(createdPayment), "Payment created successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Payment creation validation error");
            return BadRequest(ApiResponse<PaymentDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment");
            return StatusCode(500, ApiResponse<PaymentDto>.ErrorResponse("An error occurred while creating the payment"));
        }
    }

    /// <summary>
    /// Logs a payment that has already happened in full — e.g. cash handed to the
    /// landlord, or a bank transfer already confirmed outside the app. Unlike
    /// CreatePayment (which opens a Pending invoice due on a future date), this
    /// creates the record already marked Paid.
    /// </summary>
    [HttpPost("record-full")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> RecordFullPayment(
        [FromBody] RecordFullPaymentDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var payment = new Payment
            {
                TenantId = dto.TenantId,
                LeaseId = dto.LeaseId,
                Amount = dto.Amount,
                PaidDate = dto.PaidDate,
                PaymentMethod = dto.PaymentMethod,
                PaymentType = dto.PaymentType,
                TransactionReference = dto.TransactionReference,
                Notes = dto.Notes
            };

            var createdPayment = await _paymentService.RecordFullPaymentAsync(payment, dto.RentalPeriod, CurrentLandlordId, cancellationToken);
            return Ok(ApiResponse<PaymentDto>.SuccessResponse(MapToDto(createdPayment), "Payment recorded successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Payment recording validation error");
            return BadRequest(ApiResponse<PaymentDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording full payment");
            return StatusCode(500, ApiResponse<PaymentDto>.ErrorResponse("An error occurred while recording the payment"));
        }
    }

    [HttpPost("{id}/record")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> RecordPayment(
        int id, [FromBody] RecordPaymentDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var updatedPayment = await _paymentService.RecordPaymentAsync(
                id, dto.Amount, dto.PaymentMethod, dto.TransactionReference, CurrentLandlordId, cancellationToken);
            return Ok(ApiResponse<PaymentDto>.SuccessResponse(MapToDto(updatedPayment), "Payment recorded successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Payment recording validation error");
            return BadRequest(ApiResponse<PaymentDto>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Payment recording error");
            return NotFound(ApiResponse<PaymentDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording payment {PaymentId}", id);
            return StatusCode(500, ApiResponse<PaymentDto>.ErrorResponse("An error occurred while recording the payment"));
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> UpdatePayment(
        int id, [FromBody] UpdatePaymentDto dto, CancellationToken cancellationToken)
    {
        try
        {
            // SCOPED: previously fetched by bare id — any authenticated landlord
            // could edit any payment in the system by guessing an id.
            var payment = await _paymentService.GetPaymentByIdAsync(id, CurrentLandlordId, cancellationToken);
            if (payment == null)
                return NotFound(ApiResponse<PaymentDto>.ErrorResponse("Payment not found"));

            if (dto.Amount.HasValue) payment.Amount = dto.Amount.Value;
            if (dto.DueDate.HasValue) payment.DueDate = dto.DueDate.Value;
            if (!string.IsNullOrEmpty(dto.Status)) payment.Status = dto.Status;
            if (dto.LateFees.HasValue) payment.LateFees = dto.LateFees.Value;
            if (!string.IsNullOrEmpty(dto.PaymentMethod)) payment.PaymentMethod = dto.PaymentMethod;
            if (dto.Notes != null) payment.Notes = dto.Notes;
            if (dto.PaidDate.HasValue) payment.PaidDate = dto.PaidDate.Value;

            var updatedPayment = await _paymentService.UpdatePaymentAsync(payment, cancellationToken);
            return Ok(ApiResponse<PaymentDto>.SuccessResponse(MapToDto(updatedPayment), "Payment updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment {PaymentId}", id);
            return StatusCode(500, ApiResponse<PaymentDto>.ErrorResponse("An error occurred while updating the payment"));
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeletePayment(int id, CancellationToken cancellationToken)
    {
        try
        {
            // SCOPED: previously fetched by bare id — any authenticated landlord
            // could delete any payment in the system by guessing an id.
            var payment = await _paymentService.GetPaymentByIdAsync(id, CurrentLandlordId, cancellationToken);
            if (payment == null)
                return NotFound(ApiResponse.ErrorResponse("Payment not found"));

            await _paymentService.DeletePaymentAsync(id, cancellationToken);
            return Ok(ApiResponse.SuccessResponse("Payment deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting payment {PaymentId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while deleting the payment"));
        }
    }

    private static PaymentDto MapToDto(Payment payment)
    {
        return new PaymentDto
        {
            Id = payment.Id,
            TenantId = payment.TenantId,
            LeaseId = payment.LeaseId,
            UnitId = payment.UnitId,
            Amount = payment.Amount,
            LateFees = payment.LateFees,
            PaymentType = payment.PaymentType,
            Status = payment.Status,
            PaymentSource = payment.PaymentSource,
            DueDate = payment.DueDate,
            PaidDate = payment.PaidDate,
            PaymentMethod = payment.PaymentMethod,
            TransactionReference = payment.TransactionReference,
            Notes = payment.Notes,
            PaidAmount = payment.PaidAmount,
            MpesaReceiptNumber = payment.MpesaReceiptNumber,
            BillRefNumber = payment.BillRefNumber,
            PayerPhone = payment.PayerPhone,
            PayerName = payment.PayerName,
            CreatedAt = payment.CreatedAt,
            UpdatedAt = payment.UpdatedAt
        };
    }
}
