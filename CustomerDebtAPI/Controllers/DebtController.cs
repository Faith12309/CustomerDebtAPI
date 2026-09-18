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
    
    public class DebtController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DebtController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DebtResponseDTO>>> GetDebts()
        {
            var debts = await _context.Debts
                .Include(d => d.Customer)
                .ToListAsync();

            foreach (var debt in debts)
            {
                if (debt.Status != "Paid" && debt.DueDate.Date < DateTime.Today)
                {
                    debt.Status = "Overdue";
                }
            }

            await _context.SaveChangesAsync();

            var result = debts.Select(debt => new DebtResponseDTO
            {
                Id = debt.Id,
                CustomerId = debt.CustomerId,
                CustomerName = debt.Customer?.FullName ?? "",
                Amount = debt.Amount,
                DueDate = debt.DueDate.ToString("yyyy-MM-dd"),
                RemainingBalance = debt.RemainingBalance,
                Status = debt.Status,
                CreatedAt = debt.CreatedAt.ToString("yyyy-MM-dd")
            });

            return Ok(result);
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("{id}")]
        public async Task<ActionResult<DebtResponseDTO>> GetDebt(int id)
        {
            var debt = await _context.Debts
                .Include(d => d.Customer)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (debt == null)
            {
                return NotFound();
            }

            var result = new DebtResponseDTO
            {
                Id = debt.Id,
                CustomerId = debt.CustomerId,
                CustomerName = debt.Customer?.FullName ?? "",
                Amount = debt.Amount,
                DueDate = debt.DueDate.ToString("yyyy-MM-dd"),
                RemainingBalance = debt.RemainingBalance,
                Status = debt.Status,
                CreatedAt = debt.CreatedAt.ToString("yyyy-MM-dd")
            };

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Debt>> CreateDebt(DebtDTO dto)
        {
            var customer = await _context.Customers.FindAsync(dto.CustomerId);

            if (customer == null)
            {
                return BadRequest("Customer not found.");
            }

            var debt = new Debt
            {
                CustomerId = dto.CustomerId,
                Amount = dto.Amount,
                DueDate = DateTime.Parse(dto.DueDate),
                RemainingBalance = dto.Amount,
                Status = "Unpaid"
            };

            _context.Debts.Add(debt);
            await _context.SaveChangesAsync();

            var response = new DebtResponseDTO
            {
                Id = debt.Id,
                CustomerId = debt.CustomerId,
                CustomerName = customer.FullName,
                Amount = debt.Amount,
                DueDate = debt.DueDate.ToString("yyyy-MM-dd"),
                RemainingBalance = debt.RemainingBalance,
                Status = debt.Status,
                CreatedAt = debt.CreatedAt.ToString("yyyy-MM-dd")
            };

            return CreatedAtAction(nameof(GetDebt), new { id = debt.Id }, response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDebt(int id, DebtDTO dto)
        {
            var debt = await _context.Debts.FindAsync(id);

            if (debt == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FindAsync(dto.CustomerId);

            if (customer == null)
            {
                return BadRequest("Customer not found.");
            }

            debt.CustomerId = dto.CustomerId;
            debt.Amount = dto.Amount;
            debt.DueDate = DateTime.Parse(dto.DueDate);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpPost("{id}/payment")]
        public async Task<IActionResult> PayDebt(int id, PaymentDTO dto)
        {
            var debt = await _context.Debts.FindAsync(id);

            if (debt == null)
            {
                return NotFound("Debt not found.");
            }

            if (dto.PaymentAmount <= 0)
            {
                return BadRequest("Payment amount must be greater than zero.");
            }

            if (dto.PaymentAmount > debt.RemainingBalance)
            {
                return BadRequest("Payment exceeds remaining balance.");
            }

            // Deduct payment
            debt.RemainingBalance -= dto.PaymentAmount;

            // Update status
            if (debt.RemainingBalance == 0)
            {
                debt.Status = "Paid";
            }
            else
            {
                debt.Status = "Partial Paid";
            }

            await _context.SaveChangesAsync();

            var customer = await _context.Customers.FindAsync(debt.CustomerId);

            var response = new DebtResponseDTO
            {
                Id = debt.Id,
                CustomerId = debt.CustomerId,
                CustomerName = customer?.FullName ?? "",
                Amount = debt.Amount,
                DueDate = debt.DueDate.ToString("yyyy-MM-dd"),
                RemainingBalance = debt.RemainingBalance,
                Status = debt.Status,
                CreatedAt = debt.CreatedAt.ToString("yyyy-MM-dd")
            };

            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDebt(int id)
        {
            var debt = await _context.Debts.FindAsync(id);

            if (debt == null)
            {
                return NotFound();
            }

            _context.Debts.Remove(debt);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<DebtResponseDTO>>> SearchDebt(string status)
        {
            var debts = await _context.Debts
                .Include(d => d.Customer)
                .Where(d => d.Status.ToLower() == status.ToLower())
                .ToListAsync();

            var result = debts.Select(debt => new DebtResponseDTO
            {
                Id = debt.Id,
                CustomerId = debt.CustomerId,
                CustomerName = debt.Customer?.FullName ?? "",
                Amount = debt.Amount,
                DueDate = debt.DueDate.ToString("yyyy-MM-dd"),
                RemainingBalance = debt.RemainingBalance,
                Status = debt.Status,
                CreatedAt = debt.CreatedAt.ToString("yyyy-MM-dd")
            });

            return Ok(result);
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("overdue")]
        public async Task<ActionResult<IEnumerable<DebtResponseDTO>>> GetOverdueDebts()
        {
            var overdueDebts = await _context.Debts
                .Include(d => d.Customer)
             .Where(d => d.DueDate.Date < DateTime.Today &&
            d.RemainingBalance > 0)
                .ToListAsync();

            var result = overdueDebts.Select(debt => new DebtResponseDTO
            {
                Id = debt.Id,
                CustomerId = debt.CustomerId,
                CustomerName = debt.Customer?.FullName ?? "",
                Amount = debt.Amount,
                DueDate = debt.DueDate.ToString("yyyy-MM-dd"),
                RemainingBalance = debt.RemainingBalance,
                Status = debt.Status,
                CreatedAt = debt.CreatedAt.ToString("yyyy-MM-dd")
            });

            return Ok(result);
        }
    }
}