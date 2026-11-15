using AarMaabd.ViewModels;

namespace AarMaabd.Services.Interfaces
{
    public interface ICartService
    {
        string GetOrCreateCartSessionId();
        Task<CartSummaryViewModel> GetCartSummaryAsync(string? appliedCouponCode = null, string deliveryZone = "Inside Dhaka");
        Task<int> GetCartItemCountAsync();
        Task AddToCartAsync(AddToCartRequest request);
        Task UpdateQuantityAsync(int cartItemId, int quantity);
        Task RemoveFromCartAsync(int cartItemId);
        Task ClearCartAsync();
        Task MigrateGuestCartToUserAsync(string userId);
    }
}
