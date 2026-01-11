namespace StoreScrapper.Models;

public class AdapterResult
{
    public bool Success { get; set; }
    public List<int> AvailableProductSkus { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public static AdapterResult SuccessResult(List<int> skus)
    {
        return new AdapterResult
        {
            Success = true,
            AvailableProductSkus = skus
        };
    }

    public static AdapterResult ErrorResult(string errorMessage)
    {
        return new AdapterResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}
