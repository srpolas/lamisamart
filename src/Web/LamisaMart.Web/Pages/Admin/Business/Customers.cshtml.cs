using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LamisaMart.Web.Pages.Admin.Business;

[Authorize(Roles = "SuperAdmin,SuperUser,Admin")]
public class CustomersModel : PageModel
{
    public List<CustomerViewModel> CustomersList { get; set; } = new();

    public class CustomerViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public decimal LifetimeSpend { get; set; }
        public DateTime JoinedDate { get; set; }
    }

    public void OnGet()
    {
        CustomersList = new List<CustomerViewModel>
        {
            new() { Id = Guid.NewGuid(), Name = "Sharmin Akter", Email = "sharmin.akter@gmail.com", Phone = "01711223344", City = "Dhaka", TotalOrders = 12, LifetimeSpend = 48500, JoinedDate = DateTime.UtcNow.AddMonths(-8) },
            new() { Id = Guid.NewGuid(), Name = "Nusrat Jahan", Email = "nusrat.jahan@yahoo.com", Phone = "01822334455", City = "Chittagong", TotalOrders = 8, LifetimeSpend = 36200, JoinedDate = DateTime.UtcNow.AddMonths(-5) },
            new() { Id = Guid.NewGuid(), Name = "Tariqul Islam", Email = "tariqul.islam@gmail.com", Phone = "01933445566", City = "Sylhet", TotalOrders = 6, LifetimeSpend = 24800, JoinedDate = DateTime.UtcNow.AddMonths(-3) },
            new() { Id = Guid.NewGuid(), Name = "Farhana Yasmin", Email = "farhana.y@outlook.com", Phone = "01655667788", City = "Rajshahi", TotalOrders = 4, LifetimeSpend = 16500, JoinedDate = DateTime.UtcNow.AddMonths(-2) },
            new() { Id = Guid.NewGuid(), Name = "Mahbubur Rahman", Email = "mahbub.r@gmail.com", Phone = "01544332211", City = "Khulna", TotalOrders = 3, LifetimeSpend = 11200, JoinedDate = DateTime.UtcNow.AddMonths(-1) }
        };
    }
}
