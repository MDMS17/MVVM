using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using mcpdipData;

namespace mcpdandpcpa.Models
{
    public class ResponseContext : DbContext
    {
        public ResponseContext(DbContextOptions<ResponseContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("Response");
            modelBuilder.Entity<McpdipHeader>().ToTable("McpdipHeader");
            modelBuilder.Entity<McpdipDetail>().ToTable("McpdipDetail");
            modelBuilder.Entity<DetailResponse>().ToTable("DetailResponse");
        }
        public DbSet<McpdipHeader> McpdipHeaders { get; set; }
        public DbSet<McpdipDetail> McpdipDetails { get; set; }
        public DbSet<DetailResponse> DetailResponses { get; set; }
        public DbSet<mcpdipData.McpdContinuityOfCare> McpdContinuityOfCare { get; set; }

    }
}


