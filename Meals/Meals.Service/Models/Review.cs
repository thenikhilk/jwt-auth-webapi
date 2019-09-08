
namespace Meals.Service.Models
{
    public class Review
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public int Rating { get; set; }
        public int MealId { get; set; }
    }
}