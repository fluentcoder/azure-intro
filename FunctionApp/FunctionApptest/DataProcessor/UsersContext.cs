using System.Data.Entity;

namespace UserCreator
{
    public class UsersContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public UsersContext() : base("name=mycs")
        { }
    }
}