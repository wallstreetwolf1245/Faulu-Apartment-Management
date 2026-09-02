namespace FauluApartmentAPI.Models.Dtos;

public class PullTransactionRegisterRequestDto
{
    public string ShortCode { get; set; } = string.Empty;
    public string RequestType { get; set; } = "Pull";
    public string NominatedNumber { get; set; } = string.Empty;
    public string CallBackURL { get; set; } = string.Empty;
}

public class PullTransactionRegisterResponseDto
{
    public string? ResponseRefID { get; set; }
    public string? ResponseStatus { get; set; } // "1000" = registered, "1001" = already registered
    public string? ShortCode { get; set; }
    public string? ResponseDescription { get; set; }
}

public class PullTransactionQueryRequestDto
{
    public string ShortCode { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty; // "yyyy-MM-dd HH:mm:ss"
    public string EndDate { get; set; } = string.Empty;
    public string OffSetValue { get; set; } = "0";
}

public class PullTransactionQueryResponseDto
{
    public string? ResponseRefID { get; set; }
    public string? ResponseCode { get; set; }
    public string? ResponseMessage { get; set; }
}

// Shape of each transaction inside the async callback Safaricom sends to CallBackURL
public class PulledTransactionDto
{
    public string? TransactionId { get; set; }
    public string? TransTime { get; set; }
    public string? MSISDN { get; set; }
    public decimal TransAmount { get; set; }
    public string? BusinessShortCode { get; set; }
    public string? BillRefNumber { get; set; }
    public string? OrgAccountBalance { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
}

public class PullTransactionCallbackDto
{
    public string? ShortCode { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string? OffSetValue { get; set; }
    public List<PulledTransactionDto> Response { get; set; } = new();
}