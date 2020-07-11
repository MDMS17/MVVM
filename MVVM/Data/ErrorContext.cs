using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using mcpdipData;

namespace mcpdandpcpa.Models
{
    public class ErrorContext : DbContext
    {
        public ErrorContext(DbContextOptions<ErrorContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("Error");
            modelBuilder.Entity<McpdGrievance>().ToTable("McpdGrievance");
            modelBuilder.Entity<McpdAppeal>().ToTable("McpdAppeal");
            modelBuilder.Entity<McpdContinuityOfCare>().ToTable("McpdContinuityOfCare");
            modelBuilder.Entity<McpdOutOfNetwork>().ToTable("McpdOutOfNetwork");
            modelBuilder.Entity<McpdHeader>().ToTable("McpdHeader");
            modelBuilder.Entity<PcpAssignment>().ToTable("PcpAssignment");
            modelBuilder.Entity<PcpHeader>().ToTable("PcpHeader");
        }
        public DbSet<McpdGrievance> Grievances { get; set; }
        public DbSet<McpdAppeal> Appeals { get; set; }
        public DbSet<mcpdipData.McpdContinuityOfCare> McpdContinuityOfCare { get; set; }
        public DbSet<mcpdipData.McpdOutOfNetwork> McpdOutOfNetwork { get; set; }
        public DbSet<McpdHeader> McpdHeaders { get; set; }
        public DbSet<PcpAssignment> PcpAssignments { get; set; }
        public DbSet<PcpHeader> PcpHeaders { get; set; }

    }
}

