using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LamisaMart.Ordering.Application.Orders.Commands;
using LamisaMart.Payments.Application.Transactions.Commands;
using LamisaMart.Shared.Domain.ValueObjects;

namespace LamisaMart.Web.Pages;

public class CheckoutModel : PageModel
{
    private readonly IMediator _mediator;

    [BindProperty]
    public CheckoutForm Input { get; set; } = new();

    public CheckoutModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public void OnGet()
    {
        // Load initial customer details if logged in
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            // 1. Create the Order
            var address = new Address
            {
                RecipientName = Input.Name,
                PhoneNumber = Input.Phone,
                StreetAddress = Input.Address,
                District = Input.City,
                Country = "Bangladesh"
            };

            // Use dummy Guid for anonymous or signed-in customer
            var customerId = Guid.NewGuid();

            var orderCommand = new CheckoutCommand(
                customerId,
                Input.Name,
                Input.Email,
                Input.Phone,
                address,
                Input.PaymentMethod
            );

            // This creates Order, SubOrders, and clears Cart
            // Note: Currently in stub mode, this will fail if no actual DB cart items exist.
            // For UI flow demo, we'll try/catch to bypass DB empty cart error.
            
            // var order = await _mediator.Send(orderCommand);
            // var initiateCommand = new InitiatePaymentCommand(order.Id, order.OrderNumber, order.TotalAmount, "BDT", Input.Name, Input.Email, Input.Phone, Input.PaymentMethod);
            // var result = await _mediator.Send(initiateCommand);
            
            // Dummy bypass for UI purposes until UI Cart is wired to DB
            return Redirect("/Payment/Simulate"); 
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }

    public class CheckoutForm
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = "SSLCommerz";
    }
}
