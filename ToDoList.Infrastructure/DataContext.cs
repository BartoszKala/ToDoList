using Microsoft.EntityFrameworkCore;
using ToDoList.Domain;

namespace ToDoList.Infrastructure
{
    public class DataContext : DbContext
    {
        // Constructor that takes DbContextOptions and passes it to the base class
        public DataContext(DbContextOptions options) : base(options) { }

        // DbSet representing the 'ToDoList' table in the database
        public virtual DbSet<ToDoItem> ToDoList { get; set; }
    }
}
