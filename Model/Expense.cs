namespace ExpenseAuthApi.Model
{
    public class Expense
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
