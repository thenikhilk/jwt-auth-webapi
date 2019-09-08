namespace Meals.Service.Core
{
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;
    using System.Data.Entity;

    public class MealsContext : IdentityDbContext
    {
        public DbSet<Meal> Meals { get; set; }
        public DbSet<Review> Reviews { get; set; }
    }
}