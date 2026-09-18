using CustomerDebtAPI.Data;
using CustomerDebtAPI.DTOs;
using CustomerDebtAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CustomerDebtAPI.DTOs;
using Microsoft.Extensions.FileProviders;

namespace CustomerDebtAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
  
    public class CustomerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/customer
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            return await _context.Customers.ToListAsync();
        }

        // POST: api/customer
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Customer>> AddCustomer(CustomerDTO dto)
        {
            var customer = new Customer
            {
                FullName = dto.FullName,
                Address = dto.Address,
                ContactNumber = dto.ContactNumber,
                IdType = dto.IdType,
                IdNumber = dto.IdNumber,
                IdImage = dto.IdImage,
                CreatedAt = DateTime.Now
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return Ok(customer);
        }

        // PUT: api/customer/1
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, CustomerDTO dto)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            customer.FullName = dto.FullName;
            customer.Address = dto.Address;
            customer.ContactNumber = dto.ContactNumber;
            customer.IdType = dto.IdType;
            customer.IdNumber = dto.IdNumber;
            customer.IdImage = dto.IdImage;

            await _context.SaveChangesAsync();

            return Ok(customer);
        }

        // DELETE: api/customer/1
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/customer/1
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            return customer;
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("{id}/debts")]
        public async Task<ActionResult<IEnumerable<Debt>>> GetCustomerDebts(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
            {
                return NotFound("Customer not found.");
            }

            var debts = await _context.Debts
                .Where(d => d.CustomerId == id)
                .ToListAsync();

            return Ok(debts);
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Customer>>> SearchCustomer(string name)
        {
            var customers = await _context.Customers
                .Where(c => c.FullName.Contains(name))
                .ToListAsync();

            return Ok(customers);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("upload-id")]
        public async Task<IActionResult> UploadCustomerId([FromForm] UploadIdDTO dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                return BadRequest("No file uploaded.");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid() + Path.GetExtension(dto.File.FileName);

            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            var imageUrl = $"{Request.Scheme}://{Request.Host}/Uploads/{fileName}";

            return Ok(new
            {
                imageUrl
            });
        }
    }
}