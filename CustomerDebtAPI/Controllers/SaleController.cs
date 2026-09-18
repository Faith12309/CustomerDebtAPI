using CustomerDebtAPI.Data;
using CustomerDebtAPI.DTOs;
using CustomerDebtAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerDebtAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SaleController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SaleController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSale(SaleDTO dto)
        {
            if (dto.Items == null || !dto.Items.Any())
                return BadRequest("No products selected.");
            if (dto.IsCredit && dto.CustomerId == null)
            {
                return BadRequest("Customer is required for credit sales.");
            }
            decimal total = 0;

            var sale = new Sale
            {
                CustomerId = dto.CustomerId,
                IsCredit = dto.IsCredit,
                DateSold = DateTime.Now
            };

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            foreach (var item in dto.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);

                if (product == null)
                    return BadRequest($"Product ID {item.ProductId} not found.");

                if (product.Stock < item.Quantity)
                    return BadRequest($"{product.Name} has insufficient stock.");

                product.Stock -= item.Quantity;

                decimal subtotal = product.Price * item.Quantity;

                total += subtotal;

                _context.SaleItems.Add(new SaleItem
                {
                    SaleId = sale.Id,
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    SubTotal = subtotal
                });
            }

            sale.TotalAmount = total;

            await _context.SaveChangesAsync();

            if (dto.IsCredit)
            {
                var debt = await _context.Debts
    .FirstOrDefaultAsync(d => d.CustomerId == dto.CustomerId);

                if (debt == null)
                {
                    debt = new Debt
                    {
                        CustomerId = dto.CustomerId!.Value,
                        Amount = total,
                        RemainingBalance = total,
                        DueDate = dto.DueDate ?? DateTime.Now.AddDays(30),
                        Status = "Unpaid",
                        CreatedAt = DateTime.Now
                    };

                    _context.Debts.Add(debt);
                }
                else
                {
                    debt.Amount += total;
                    debt.RemainingBalance += total;
                    debt.Status = "Unpaid";

                    if (dto.DueDate.HasValue)
                    {
                        debt.DueDate = dto.DueDate.Value;
                    }
                }

                await _context.SaveChangesAsync();
            }

            return Ok(new SaleResponseDTO
            {
                SaleId = sale.Id,
                TotalAmount = sale.TotalAmount,
                IsCredit = sale.IsCredit,
                DateSold = sale.DateSold
            });


        }

        [HttpGet]
        public async Task<IActionResult> GetAllSales()
        {
            var sales = await _context.Sales
                .Include(s => s.Customer)
                .OrderByDescending(s => s.DateSold)
                .Select(s => new SaleResponseDTO
                {
                    SaleId = s.Id,
                    CustomerName = s.Customer != null
                        ? s.Customer.FullName
                        : "Walk-in Customer",
                    TotalAmount = s.TotalAmount,
                    IsCredit = s.IsCredit,
                    DateSold = s.DateSold
                })
                .ToListAsync();

            return Ok(sales);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSaleById(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null)
                return NotFound("Sale not found.");

            return Ok(new
            {
                SaleId = sale.Id,
                Customer = sale.Customer != null
                    ? sale.Customer.FullName
                    : "Walk-in Customer",
                sale.TotalAmount,
                sale.IsCredit,
                sale.DateSold,
                Items = sale.SaleItems.Select(i => new
                {
                    Product = i.Product.Name,
                    i.Quantity,
                    i.UnitPrice,
                    i.SubTotal
                })
            });
        }

        [HttpGet("today")]
        public async Task<IActionResult> GetTodaySales()
        {
            var today = DateTime.Today;

            var sales = await _context.Sales
                .Include(s => s.Customer)
                .Where(s => s.DateSold.Date == today)
                .Select(s => new SaleResponseDTO
                {
                    SaleId = s.Id,
                    CustomerName = s.Customer != null
                        ? s.Customer.FullName
                        : "Walk-in Customer",
                    TotalAmount = s.TotalAmount,
                    IsCredit = s.IsCredit,
                    DateSold = s.DateSold
                })
                .ToListAsync();

            return Ok(sales);
        }

        [HttpGet("cash")]
        public async Task<IActionResult> GetCashSales()
        {
            var sales = await _context.Sales
                .Include(s => s.Customer)
                .Where(s => !s.IsCredit)
                .OrderByDescending(s => s.DateSold)
                .Select(s => new SaleResponseDTO
                {
                    SaleId = s.Id,
                    CustomerName = s.Customer != null
                        ? s.Customer.FullName
                        : "Walk-in Customer",
                    TotalAmount = s.TotalAmount,
                    IsCredit = s.IsCredit,
                    DateSold = s.DateSold
                })
                .ToListAsync();

            return Ok(sales);
        }

        [HttpGet("credit")]
        public async Task<IActionResult> GetCreditSales()
        {
            var sales = await _context.Sales
                .Include(s => s.Customer)
                .Where(s => s.IsCredit)
                .OrderByDescending(s => s.DateSold)
                .Select(s => new SaleResponseDTO
                {
                    SaleId = s.Id,
                    CustomerName = s.Customer != null
                        ? s.Customer.FullName
                        : "Walk-in Customer",
                    TotalAmount = s.TotalAmount,
                    IsCredit = s.IsCredit,
                    DateSold = s.DateSold
                })
                .ToListAsync();

            return Ok(sales);
        }

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetCustomerCreditSales(int customerId)
        {
            var sales = await _context.Sales
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                .Where(s => s.CustomerId == customerId && s.IsCredit)
                .OrderByDescending(s => s.DateSold)
                .Select(s => new
                {
                    SaleId = s.Id,
                    DateSold = s.DateSold,
                    TotalAmount = s.TotalAmount,
                    Items = s.SaleItems.Select(i => new
                    {
                        Product = i.Product.Name,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        SubTotal = i.SubTotal
                    })
                })
                .ToListAsync();

            return Ok(sales);
        }
    }
}