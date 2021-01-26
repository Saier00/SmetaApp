using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Microsoft.AspNet.Identity.EntityFramework;

namespace SmetaApp.Models
{
    public class SmetaContext: IdentityDbContext<ApplicationUser>
    {
        public SmetaContext() : base("DefaultConnection") 
        {
            var objectContext = (this as IObjectContextAdapter).ObjectContext;
            objectContext.CommandTimeout = 120;
        }
        public static SmetaContext Create()
        {
            return new SmetaContext();
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Job>().HasKey(p => p.Id );
            modelBuilder.Entity<MatPrice>().HasKey(p => p.Name );
            modelBuilder.Entity<MechPrice>().HasKey(p => p.Name);
            modelBuilder.Entity<MechNameMap>().HasKey(p =>  p.MechName);
            modelBuilder.Entity<MatNameMap>().HasKey(p => p.MatName);
            modelBuilder.Entity<Page>().HasKey(p => p.Name);

            modelBuilder.Entity<Job>().HasMany(j => j.Mats).WithRequired(m=>m.Job).HasForeignKey(m => m.JobId).WillCascadeOnDelete(true);
            modelBuilder.Entity<Job>().HasMany(j => j.Mechs).WithRequired(m => m.Job).HasForeignKey(m => m.JobId).WillCascadeOnDelete(true);

            modelBuilder.Entity<MechNameMap>().HasOptional(m => m.MechPrice).WithMany(p => p.AnotherNames).HasForeignKey(p => p.MechPriceName).WillCascadeOnDelete(false);
            modelBuilder.Entity<MatNameMap>().HasOptional(m => m.MatPrice).WithMany(p => p.AnotherNames).HasForeignKey(p => p.MatPriceName).WillCascadeOnDelete(false);

            modelBuilder.Entity<MechNameMap>().HasMany(m => m.Mechs).WithRequired(m => m.MechNameMap).HasForeignKey(m => m.Name).WillCascadeOnDelete(false);
            modelBuilder.Entity<MatNameMap>().HasMany(m => m.Mats).WithRequired(m => m.MatNameMap).HasForeignKey(m => m.Name).WillCascadeOnDelete(false);

            modelBuilder.Entity<Job>().HasOptional(j => j.Page).WithMany(p => p.Jobs).HasForeignKey(p => p.PageName).WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Mech> Mechs { get; set; }
        public DbSet<Mat> Mats { get; set; }
        public DbSet<MatNameMap> MatNameMaps { get; set; }
        public DbSet<MechNameMap> MechNameMaps { get; set; }
        public DbSet<MechPrice> MechPrices { get; set; }
        public DbSet<MatPrice> MatPrices { get; set; }

    }
}