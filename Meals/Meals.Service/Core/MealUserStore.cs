
namespace Meals.Service.Core
{
    using Microsoft.AspNet.Identity.EntityFramework;

    public class MealUserStore : UserStore<IdentityUser>
    {
        public MealUserStore() : base(new MealsContext())
        {
        }
    }
}