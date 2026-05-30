using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Add custom fields if needed
        public string? FullName { get; set; }
        public string Role { get; set; } = "User"; // Default to User; Admin, Sales, Inventory, Accountant
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
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
        }
    }
}





