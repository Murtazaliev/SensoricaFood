using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AVBDelivery.Models;
using System.Composition.Convention;


namespace AVBDelivery.Models
{
    public class ApplicationContext : IdentityDbContext<User>
    {

        public DbSet<DBLog> DBLog { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
            Database.Migrate();

            //Database.EnsureCreated();
        }
        public DbSet<WorkingHours> WorkingHours { get; set; }

        public DbSet<Nomenclature> Nomenclature { get; set; }

        public DbSet<ShoppingCart> ShoppingCart { get; set; }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductGroup> ProductGroups { get; set; }
        public DbSet<Order> Orders { get; set; }

        public DbSet<IikoServer> IikoServer { get; set; }
        public DbSet<DateOfComing> DateOfComing { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Settings> Settings { get; set; }

        public DbSet<Note> Notes { get; set; }

        public DbSet<SiteAnnouncement> SiteAnnouncements { get; set; }

        public DbSet<Menu> Menus { get; set; }
        public DbSet<MenuProduct> MenuProducts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MenuProduct>().HasKey(mp => new { mp.MenuId, mp.ProductId });

            modelBuilder.Entity<MenuProduct>()
                .HasOne(mp => mp.Menu)
                .WithMany(m => m.MenuProducts)
                .HasForeignKey(mp => mp.MenuId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Menu>()
                .HasMany(m => m.MenuProducts)
                .WithOne(mp => mp.Menu)
                .HasForeignKey(mp => mp.MenuId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Organization>()
                .HasOne(o => o.Menu)
                .WithMany()
                .HasForeignKey(o => o.MenuId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
