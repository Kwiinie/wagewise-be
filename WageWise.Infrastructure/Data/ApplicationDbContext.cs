using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WageWise.Domain.Entities;

namespace WageWise.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }
        public DbSet<CVMetaData> CVs => Set<CVMetaData>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CVMetaData>(entity =>
            {
                entity.ToTable("CVs");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.FileUrl).IsRequired();
                entity.Property(e => e.FileName).IsRequired();
                entity.Property(e => e.Province).IsRequired();
                entity.Property(e => e.District);
                entity.Property(e => e.PositionLevel).IsRequired();
                entity.Property(e => e.Field).IsRequired();
                entity.Property(e => e.JobCategory).IsRequired();
                entity.Property(e => e.Location).IsRequired();
                entity.Property(e => e.EstimatedSalary).IsRequired();
                entity.Property(e => e.UploadedAt).IsRequired();
            });
        }
    }
}
