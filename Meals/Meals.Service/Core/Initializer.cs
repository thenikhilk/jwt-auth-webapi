
namespace Meals.Service.Core
{
    using System.Data.Entity;

    public class Initializer : MigrateDatabaseToLatestVersion<MealsContext, Configuration>
    {
    }
}