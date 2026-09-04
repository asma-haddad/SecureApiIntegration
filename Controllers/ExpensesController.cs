using ExpenseAuthApi.Data;
using ExpenseAuthApi.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseAuthApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpensesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ExpensesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var expense = await _context.Expenses
                .FirstOrDefaultAsync(x => x.Id == id);

            if (expense == null)
                throw new NotFoundException("Expense not found");
            return Ok(expense);
        }
    }
}

