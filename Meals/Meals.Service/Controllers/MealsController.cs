
namespace Meals.Service.Controllers
{
    using Core;
    using System.Data.Entity;
    using System.Threading.Tasks;
    using System.Web.Http;

    public class MealsController : ApiController
    {
        [HttpGet]
        public async Task<IHttpActionResult> Get()
        {
            using (var context = new MealsContext())
            {
                return Ok(await context.Meals.Include(meal => meal.Reviews).ToListAsync());
            }
        }
    }
}