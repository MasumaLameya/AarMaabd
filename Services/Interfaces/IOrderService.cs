using AarMaabd.Models;
using AarMaabd.ViewModels;

namespace AarMaabd.Services.Interfaces
{
    public class OrderPlacementResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Order? Order { get; set; }
    }

    public interface IOrderService
    {
        Task<OrderPlacementResult> PlaceOrderAsync(CheckoutViewModel model, string? userId);
        Task<Order?> GetOrderByNumberAsync(string orderNumber);
        Task<Order?> GetOrderForCustomerAsync(string orderNumber, string customerPhone);
        Task<List<Order>> GetCustomerOrdersAsync(string userId);
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, string adminUser, string? notes = null);
        Task<bool> UpdatePaymentStatusAsync(int orderId, PaymentStatus newStatus, string? transactionId = null, string? adminUser = null);
    }
}
