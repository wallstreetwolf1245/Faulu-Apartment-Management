namespace FauluApartmentAPI.Models.Dtos;

public class C2BValidationDto
{
    public string TransactionType { get; set; } = string.Empty;
    public string TransID { get; set; } = string.Empty;
    public string TransTime { get; set; } = string.Empty;
    public string TransAmount { get; set; } = string.Empty;
    public string BusinessShortCode { get; set; } = string.Empty;
    public string BillRefNumber { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public string OrgAccountBalance { get; set; } = string.Empty;
    public string ThirdPartyTransID { get; set; } = string.Empty;
    public string MSISDN { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class C2BConfirmationDto
{
    public string TransactionType { get; set; } = string.Empty;
    public string TransID { get; set; } = string.Empty;
    public string TransTime { get; set; } = string.Empty;
    public string TransAmount { get; set; } = string.Empty;
    public string BusinessShortCode { get; set; } = string.Empty;
    public string BillRefNumber { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public string OrgAccountBalance { get; set; } = string.Empty;
    public string ThirdPartyTransID { get; set; } = string.Empty;
    public string MSISDN { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class DarajaTokenResponse
{
    public string access_token { get; set; } = string.Empty;
    public string expires_in { get; set; } = string.Empty;
}

public class DarajaRegisterUrlResponse
{
    public string OriginatorCoversationID { get; set; } = string.Empty;
    public string ResponseCode { get; set; } = string.Empty;
    public string ResponseDescription { get; set; } = string.Empty;
}

public class MatchPaymentDto
{
    public int UnitId { get; set; }
}