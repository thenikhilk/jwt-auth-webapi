
namespace Meals.Service.ViewModels
{
    using Models; 

    public class ReviewViewModel
    {
        public ReviewViewModel()
        {
        }

        public ReviewViewModel(Review review)
        {
            if (review == null)
            {
                return;
            }

            MealId = review.MealId;
            Rating = review.Rating;
            Description = review.Description;
        }

        public int MealId { get; set; }
        public int Rating { get; set; }
        public string Description { get; set; }

        public Review ToReview()
        {
            return new Review
            {
                MealId = MealId,
                Description = Description,
                Rating = Rating
            };
        }
    }
}