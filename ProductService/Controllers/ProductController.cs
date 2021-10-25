using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using ProductService.Models;

namespace ProductService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : Controller
    {
        [HttpGet("list")]
        public IEnumerable<Product> GetProducts()
        {
            for (int i = 1; i < 3; i++)
            {
                yield return new Product() { ProductId = i, Name = $"Product-{i}" };
            }
        }
    }
}