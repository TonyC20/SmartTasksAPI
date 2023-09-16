using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartTasksAPI.Entities;

namespace SmartTasksAPI.DbContexts
{
    public class SmartTasksContext : IdentityDbContext<UserAccount>
    {
        /// <summary>
        /// Checklists stored in the database
        /// </summary>
        public DbSet<Checklist> Checklists { get; set; } = null!;
        /// <summary>
        /// Tasks stored in the database
        /// </summary>
        public DbSet<TaskItem> TaskItems { get; set; } = null!;

        // Pass in configuration options
        public SmartTasksContext(DbContextOptions<SmartTasksContext> options) 
            : base(options) { }
    }
}
