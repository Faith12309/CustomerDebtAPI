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
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Product
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products.ToListAsync();
            return Ok(products);
        }
        // GET: api/Product/search?keyword=milo
        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                var allProducts = await _context.Products.ToListAsync();
                return Ok(allProducts);
            }

            var products = await _context.Products
                .Where(p => p.Name.Contains(keyword))
                .ToListAsync();

            return Ok(products);
        }
        // GET: api/Product/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound("Product not found.");

            return Ok(product);
        }

        // POST: api/Product
        [HttpPost]
        public async Task<IActionResult> AddProduct(ProductDTO dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Category = dto.Category,
                Price = dto.Price,
                Stock = dto.Stock,
                Unit = dto.Unit,
                IsActive = true
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Ok(product);
        }

        // PUT: api/Product/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, ProductDTO dto)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound("Product not found.");

            product.Name = dto.Name;
            product.Category = dto.Category;
            product.Price = dto.Price;
            product.Stock = dto.Stock;
            product.Unit = dto.Unit;
            await _context.SaveChangesAsync();

            return Ok(product);
        }
        // POST: api/Product/5/restock
        [HttpPost("{id}/restock")]
        public async Task<IActionResult> RestockProduct(int id, RestockDTO dto)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound("Product not found.");

            if (dto.Quantity <= 0)
                return BadRequest("Quantity must be greater than zero.");

            product.Stock += dto.Quantity;

            await _context.SaveChangesAsync();

            return Ok(product);
        }
        // DELETE: api/Product/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound("Product not found.");

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok("Product deleted successfully.");
        }
    }
}