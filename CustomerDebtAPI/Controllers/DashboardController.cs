using CustomerDebtAPI.Data;
using CustomerDebtAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerDebtAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<DashboardDTO>> GetDashboard()
        {
            var dashboard = new DashboardDTO
            {
                TotalCustomers = await _context.Customers.CountAsync(),

                TotalProducts = await _context.Products.CountAsync(),

                TotalDebts = await _context.Debts.CountAsync(),

                OutstandingDebt = await _context.Debts
         .Where(d => d.Status != "Paid")
         .SumAsync(d => (decimal?)d.RemainingBalance) ?? 0,

                PaidDebts = await _context.Debts
         .CountAsync(d => d.Status == "Paid"),

                UnpaidDebts = await _context.Debts
         .CountAsync(d => d.Status == "Unpaid"),

                PartialPaidDebts = await _context.Debts
         .CountAsync(d => d.Status == "Partial Paid"),

                OverdueDebts = await _context.Debts
         .CountAsync(d =>
             d.DueDate < DateTime.Today &&
             d.Status != "Paid")
            };

            return Ok(dashboard);
        }

        [HttpGet("latest-debts")]
        public async Task<IActionResult> GetLatestDebts()
        {
            var latestDebts = await _context.Debts
                .Include(d => d.Customer)
                .OrderByDescending(d => d.Id)
                .Take(5)
                .Select(d => new
                {
                    d.Id,
                    CustomerName = d.Customer.FullName,
                    d.Amount,
                    d.RemainingBalance,
                    d.Status,
                    d.DueDate
                })
                .ToListAsync();

            return Ok(latestDebts);
        }

        [HttpGet("due-alerts")]
        public async Task<IActionResult> GetDueAlerts()
        {
            var today = DateTime.Today;
            var threeDays = today.AddDays(3);

            var alerts = await _context.Debts
                .Include(d => d.Customer)
                .Where(d => d.Status != "Paid" &&
                            d.DueDate <= threeDays)
                .OrderBy(d => d.DueDate)
                .Select(d => new
                {
                    d.Id,
                    CustomerName = d.Customer.FullName,
                    d.RemainingBalance,
                    d.DueDate,
                    AlertType = d.DueDate < today
           ? $"Overdue by {(today - d.DueDate).Days} day(s)"
             : d.DueDate == today
                ? "Due Today"
               : $"Due in {(d.DueDate - today).Days} day(s)"
                })
                .ToListAsync();

            return Ok(alerts);
        }

        [HttpGet("sales-summary")]
        public async Task<IActionResult> GetSalesSummary()
        {
            var today = DateTime.Today;

            var todaySales = await _context.Sales
                .Where(s => s.DateSold.Date == today)
                .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;

            var cashSales = await _context.Sales
                .Where(s => !s.IsCredit)
                .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;

            var creditSales = await _context.Sales
                .Where(s => s.IsCredit)
                .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;

            var totalSales = await _context.Sales
                .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;

            var totalTransactions = await _context.Sales.CountAsync();

            return Ok(new DashboardSalesDTO
            {
                TodaySales = todaySales,
                CashSales = cashSales,
                CreditSales = creditSales,
                TotalSales = totalSales,
                TotalTransactions = totalTransactions
            });

        }
        [HttpGet("recent-sales")]
        public async Task<IActionResult> GetRecentSales()
        {
            var recentSales = await _context.Sales
                .Include(s => s.Customer)
                .OrderByDescending(s => s.DateSold)
                .Take(5)
                .Select(s => new
                {
                    s.Id,
                    CustomerName = s.Customer.FullName,
                    Amount = s.TotalAmount,
                    Type = s.IsCredit ? "Credit" : "Cash",
                    Date = s.DateSold
                })
                .ToListAsync();

            return Ok(recentSales);
        }
    }
}