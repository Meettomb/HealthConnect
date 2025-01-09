using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using HealthConnect.Models;

namespace HealthConnect.Data
{
    public partial class HealthConnectContext : DbContext // Mark the class as partial
    {
        public HealthConnectContext()
        {
        }

        public HealthConnectContext(DbContextOptions<HealthConnectContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Doctor_approvel> DoctorApprovels { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Doctor_approvel>().ToTable("Doctor_approvel");
            OnModelCreatingPartial(modelBuilder); // Call the partial method if needed
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder); // Now valid within a partial class
    }
}
