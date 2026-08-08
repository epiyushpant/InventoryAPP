using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Inventory.Services;

namespace Inventory.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Add custom fields if needed
        public string? FullName { get; set; }
        public string Role { get; set; } = "User"; // Default to User; Admin, Sales, Inventory, Accountant
        /// <summary>Shop this user belongs to. JWT claim tenant_id.</summary>
        public int TenantId { get; set; } = 1;
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly ITenantContext? _tenantContext;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantContext tenantContext)
            : base(options)
        {
            _tenantContext = tenantContext;
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleDetail> SaleDetails { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PurchaseRequisition> PurchaseRequisitions { get; set; }
        public DbSet<GRN> GRNs { get; set; }
        public DbSet<DeliveryNote> DeliveryNotes { get; set; }
        public DbSet<SalesInvoice> SalesInvoices { get; set; }
        public DbSet<StockAdjustment> StockAdjustments { get; set; }
        public DbSet<StockTransfer> StockTransfers { get; set; }
        public DbSet<UnitConversion> UnitConversions { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Province> Provinces { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Municipality> Municipalities { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<TenantCapability> TenantCapabilities { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Category
            modelBuilder.Entity<Category>()
                .HasKey(c => c.CategoryID);
            modelBuilder.Entity<Category>()
                .ToTable("Categories");

            // Configure Supplier
            modelBuilder.Entity<Supplier>()
                .HasKey(s => s.SupplierID);
            modelBuilder.Entity<Supplier>()
                .ToTable("Suppliers");

            // Configure Customer
            modelBuilder.Entity<Customer>()
                .HasKey(c => c.CustomerID);
            modelBuilder.Entity<Customer>()
                .ToTable("Customers");

            // Configure Location
            modelBuilder.Entity<Location>()
                .HasKey(l => l.LocationID);
            modelBuilder.Entity<Location>()
                .ToTable("Locations");

            // Configure Product
            modelBuilder.Entity<Product>()
                .HasKey(p => p.ProductID);
            modelBuilder.Entity<Product>()
                .ToTable("Products");

            // Configure Inventory
            modelBuilder.Entity<Inventory>()
                .HasKey(i => i.InventoryID);
            modelBuilder.Entity<Inventory>()
                .ToTable("Inventory");

            // Configure PurchaseOrder
            modelBuilder.Entity<PurchaseOrder>()
                .HasKey(po => po.PurchaseOrderID);
            modelBuilder.Entity<PurchaseOrder>()
                .ToTable("PurchaseOrders");

            // Configure PurchaseOrderDetail
            modelBuilder.Entity<PurchaseOrderDetail>()
                .HasKey(pod => pod.PODetailID);
            modelBuilder.Entity<PurchaseOrderDetail>()
                .ToTable("PurchaseOrderDetails");

            // Configure Sale
            modelBuilder.Entity<Sale>()
                .HasKey(s => s.SaleID);
            modelBuilder.Entity<Sale>()
                .ToTable("Sales");

            // Configure SaleDetail
            modelBuilder.Entity<SaleDetail>()
                .HasKey(sd => sd.SaleDetailID);
            modelBuilder.Entity<SaleDetail>()
                .ToTable("SalesDetails");

            // Configure StockMovement
            modelBuilder.Entity<StockMovement>()
                .HasKey(sm => sm.MovementID);
            modelBuilder.Entity<StockMovement>()
                .ToTable("StockMovements");

            modelBuilder.Entity<Post>()
                .HasOne(p => p.Author)
                .WithMany()
                .HasForeignKey(p => p.AuthorId);

            // Configure PurchaseRequisition
            modelBuilder.Entity<PurchaseRequisition>()
                .HasKey(pr => pr.PRID);
            modelBuilder.Entity<PurchaseRequisition>()
                .ToTable("PurchaseRequisitions");

            modelBuilder.Entity<GRN>()
                .ToTable("GRNs");

            // Configure DeliveryNote
            modelBuilder.Entity<DeliveryNote>()
                .HasKey(dn => dn.DeliveryID);
            modelBuilder.Entity<DeliveryNote>()
                .ToTable("DeliveryNotes");

            modelBuilder.Entity<SalesInvoice>()
                .ToTable("SalesInvoices");

            // Configure StockAdjustment
            modelBuilder.Entity<StockAdjustment>()
                .HasKey(sa => sa.AdjustmentID);
            modelBuilder.Entity<StockAdjustment>()
                .ToTable("StockAdjustments");

            // Configure StockTransfer
            modelBuilder.Entity<StockTransfer>()
                .HasKey(st => st.TransferID);
            modelBuilder.Entity<StockTransfer>()
                .ToTable("StockTransfers");

            // Configure Country
            modelBuilder.Entity<Country>()
                .HasKey(c => c.CountryID);
            modelBuilder.Entity<Country>()
                .ToTable("Countries");

            // Configure Province
            modelBuilder.Entity<Province>()
                .HasKey(p => p.ProvinceID);
            modelBuilder.Entity<Province>()
                .ToTable("Provinces")
                .HasOne(p => p.Country)
                .WithMany()
                .HasForeignKey(p => p.CountryID);

            // Configure District
            modelBuilder.Entity<District>()
                .HasKey(d => d.DistrictID);
            modelBuilder.Entity<District>()
                .ToTable("Districts")
                .HasOne(d => d.Province)
                .WithMany()
                .HasForeignKey(d => d.ProvinceID);

            // Configure Municipality
            modelBuilder.Entity<Municipality>()
                .HasKey(m => m.MunicipalityID);
            modelBuilder.Entity<Municipality>()
                .ToTable("Municipalities")
                .HasOne(m => m.District)
                .WithMany()
                .HasForeignKey(m => m.DistrictID);

            modelBuilder.Entity<Tenant>()
                .ToTable("Tenants")
                .HasKey(t => t.TenantId);

            modelBuilder.Entity<TenantCapability>()
                .ToTable("TenantCapabilities")
                .HasKey(c => c.TenantCapabilityId);
            modelBuilder.Entity<TenantCapability>()
                .HasIndex(c => new { c.TenantId, c.Key })
                .IsUnique();
            modelBuilder.Entity<TenantCapability>()
                .HasOne(c => c.Tenant)
                .WithMany(t => t.Capabilities)
                .HasForeignKey(c => c.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermission>()
                .ToTable("RolePermissions")
                .HasKey(p => p.RolePermissionId);
            modelBuilder.Entity<RolePermission>()
                .HasIndex(p => new { p.TenantId, p.RoleName, p.Key })
                .IsUnique();
            modelBuilder.Entity<RolePermission>()
                .HasOne(p => p.Tenant)
                .WithMany()
                .HasForeignKey(p => p.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Wave F — tenant isolation (skipped when no tenant on context, e.g. migrations/startup)
            ApplyTenantFilter<Category>(modelBuilder);
            ApplyTenantFilter<Supplier>(modelBuilder);
            ApplyTenantFilter<Customer>(modelBuilder);
            ApplyTenantFilter<Location>(modelBuilder);
            ApplyTenantFilter<Product>(modelBuilder);
            ApplyTenantFilter<Inventory>(modelBuilder);
            ApplyTenantFilter<PurchaseOrder>(modelBuilder);
            ApplyTenantFilter<PurchaseOrderDetail>(modelBuilder);
            ApplyTenantFilter<Sale>(modelBuilder);
            ApplyTenantFilter<SaleDetail>(modelBuilder);
            ApplyTenantFilter<StockMovement>(modelBuilder);
            ApplyTenantFilter<PurchaseRequisition>(modelBuilder);
            ApplyTenantFilter<GRN>(modelBuilder);
            ApplyTenantFilter<DeliveryNote>(modelBuilder);
            ApplyTenantFilter<SalesInvoice>(modelBuilder);
            ApplyTenantFilter<StockAdjustment>(modelBuilder);
            ApplyTenantFilter<StockTransfer>(modelBuilder);
            ApplyTenantFilter<UnitConversion>(modelBuilder);
            ApplyTenantFilter<Post>(modelBuilder);

            modelBuilder.Entity<ApplicationUser>()
                .HasQueryFilter(u => _tenantContext == null || !_tenantContext.HasTenant || u.TenantId == _tenantContext.TenantId);
        }

        private void ApplyTenantFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : class, ITenantScoped
        {
            modelBuilder.Entity<TEntity>()
                .HasIndex(e => e.TenantId);
            modelBuilder.Entity<TEntity>()
                .HasQueryFilter(e => _tenantContext == null || !_tenantContext.HasTenant || e.TenantId == _tenantContext.TenantId);
        }

        public override int SaveChanges()
        {
            StampTenantIds();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            StampTenantIds();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void StampTenantIds()
        {
            if (_tenantContext == null || !_tenantContext.HasTenant) return;

            foreach (var entry in ChangeTracker.Entries<ITenantScoped>())
            {
                if (entry.State == EntityState.Added)
                    entry.Entity.TenantId = _tenantContext.TenantId;
            }

            foreach (var entry in ChangeTracker.Entries<ApplicationUser>())
            {
                if (entry.State == EntityState.Added)
                    entry.Entity.TenantId = _tenantContext.TenantId;
            }
        }
    }
}





