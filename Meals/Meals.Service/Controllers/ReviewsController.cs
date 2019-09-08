
namespace Meals.Service.Controllers
{
    using Core;
    using Models;
    using System.Data.Entity;
    using System.Threading.Tasks;
    using System.Web.Http;
    using ViewModels;

    public class ReviewsController : ApiController
    {
        [HttpPost]
        public async Task<IHttpActionResult> Post([FromBody] ReviewViewModel review)
        {
            using (var context = new MealsContext())
            {
                var meal = await context.Meals.FirstOrDefaultAsync(b => b.Id == review.MealId);
                if (meal == null)
                {
                    return NotFound();
                }

                var newReview = context.Reviews.Add(new Review
                {
                    MealId = meal.Id,
                    Description = review.Description,
                    Rating = review.Rating
                });

                await context.SaveChangesAsync();
                return Ok(new ReviewViewModel(newReview));
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Administrator")]
        public async Task<IHttpActionResult> Delete(int id)
        {
            using (var context = new MealsContext())
            {
                var review = await context.Reviews.FirstOrDefaultAsync(r => r.Id == id);
                if (review == null)
                {
                    return NotFound();
                }

                context.Reviews.Remove(review);
                await context.SaveChangesAsync();
            }
            return Ok();
        }
    }
}