
namespace Meals.Service.Core
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;

    public class MealUserManager : UserManager<IdentityUser>
    {
        public MealUserManager() : base(new MealUserStore())
        {
        }
    }
}