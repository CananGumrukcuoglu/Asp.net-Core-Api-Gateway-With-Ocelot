using System.Collections.Generic;
using CustomerService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CustomerService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : Controller
    {
        [HttpGet("list")]
        public IEnumerable<Customer> GetCustomers()
        {
            for (int i = 1; i < 3; i++)
            {
                yield return new Customer() { CustomerId = i, Name = $"Customer-{i}" };
            }
        }
    }
}