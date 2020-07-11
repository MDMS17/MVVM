using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using mcpdipData;

namespace mcpdandpcpa.Models
{
    public class LogContext : DbContext
    {
        public LogContext(DbContextOptions<LogContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("dbo");
            modelBuilder.Entity<ProcessLog>().ToTable("ProcessLog");
            modelBuilder.Entity<SubmissionLog>().ToTable("SubmissionLog");
            modelBuilder.Entity<OperationLog>().ToTable("OperationLog");
        }
        public DbSet<ProcessLog> ProcessLogs { get; set; }
        public DbSet<SubmissionLog> SubmissionLogs { get; set; }
        public DbSet<OperationLog> OperationLogs { get; set; }

    }
}
