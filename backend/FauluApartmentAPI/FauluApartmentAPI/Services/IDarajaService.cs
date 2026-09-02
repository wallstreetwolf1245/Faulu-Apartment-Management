namespace FauluApartmentAPI.Services;

public interface IDarajaService
{
    Task<string> GetAccessTokenAsync();
    Task<bool> RegisterC2BUrlsAsync();
    Task<FauluApartmentAPI.Models.Dtos.StkPushResponseDto> InitiateStkPushAsync(FauluApartmentAPI.Models.Dtos.StkPushRequestDto request);
    Task<FauluApartmentAPI.Models.Dtos.StkPushQueryResponseDto> QueryStkPushStatusAsync(string checkoutRequestId);
    Task<string> RequestTransactionStatusAsync(string transactionId);
    Task<FauluApartmentAPI.Models.Dtos.PullTransactionRegisterResponseDto> RegisterPullTransactionsAsync();
    Task<FauluApartmentAPI.Models.Dtos.PullTransactionQueryResponseDto> QueryPullTransactionsAsync(DateTime startDate, DateTime endDate, int offset = 0);
}