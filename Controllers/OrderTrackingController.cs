using AarMaabd.Services.Interfaces;
using AarMaabd.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AarMaabd.Controllers
{
    public class OrderTrackingController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderTrackingController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new OrderTrackingRequestViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Track(OrderTrackingRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            var order = await _orderService.GetOrderForCustomerAsync(model.OrderNumber.Trim(), model.CustomerPhone.Trim());
            if (order == null)
            {
                ViewBag.ErrorMessage = "No order found matching the provided Order Number and Phone Number. Please double check your order receipt.";
                return View("Index", model);
            }

            return View("TrackResult", order);
        }
    }
}
