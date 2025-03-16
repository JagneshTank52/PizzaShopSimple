using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PizzaShop.Entity.Models;

namespace PizzaShop.Entity.Data;

public partial class PizzaShopContext : DbContext
{
    public PizzaShopContext()
    {
    }

    public PizzaShopContext(DbContextOptions<PizzaShopContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<FoodType> FoodTypes { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<ItemsModifier> ItemsModifiers { get; set; }

    public virtual DbSet<MeasuringUnit> MeasuringUnits { get; set; }

    public virtual DbSet<Modifier> Modifiers { get; set; }

    public virtual DbSet<ModifierGroup> ModifierGroups { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<OrderModifier> OrderModifiers { get; set; }

    public virtual DbSet<OrderStatus> OrderStatuses { get; set; }

    public virtual DbSet<OrdersTax> OrdersTaxes { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<PaymentMode> PaymentModes { get; set; }

    public virtual DbSet<PaymentStatus> PaymentStatuses { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<RoleWisePermission> RoleWisePermissions { get; set; }

    public virtual DbSet<Section> Sections { get; set; }

    public virtual DbSet<State> States { get; set; }

    public virtual DbSet<Table> Tables { get; set; }

    public virtual DbSet<TableOrderMapping> TableOrderMappings { get; set; }

    public virtual DbSet<TableStatus> TableStatuses { get; set; }

    public virtual DbSet<TaxAndFee> TaxAndFees { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<Wating> Watings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Name=PostgreSqlConnectionString");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("PizzaShop", "ORDERSTATUS", new[] { "PENDING", "COMPLETED", "CANCELLED" })
            .HasPostgresEnum("PizzaShop", "TABLEAVALABILITY", new[] { "Available", "Occupied" });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Category_pkey");

            entity.ToTable("Category", "PizzaShop");

            entity.HasIndex(e => e.Name, "Category_Name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.ModifiedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Name).HasMaxLength(250);
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("City_pkey");

            entity.ToTable("City", "PizzaShop");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(250);
            entity.Property(e => e.StateId).HasColumnName("StateID");

            entity.HasOne(d => d.State).WithMany(p => p.Cities)
                .HasForeignKey(d => d.StateId)
                .HasConstraintName("City_StateID_fkey");
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Country_pkey");

            entity.ToTable("Country", "PizzaShop");

            entity.HasIndex(e => e.Name, "Country_Name_key").IsUnique();

            entity.HasIndex(e => e.ShortName, "Country_ShortName_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(250);
            entity.Property(e => e.ShortName).HasMaxLength(5);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Customer_pkey");

            entity.ToTable("Customer", "PizzaShop");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Email).HasMaxLength(250);
            entity.Property(e => e.ModifiedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Name).HasMaxLength(250);
            entity.Property(e => e.Phone).HasMaxLength(20);
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Feedback_pkey");

            entity.ToTable("Feedback", "PizzaShop");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
        });

        modelBuilder.Entity<FoodType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("FoodType_pkey");

            entity.ToTable("FoodType", "PizzaShop");

            entity.HasIndex(e => e.Name, "FoodType_Name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Invoice_pkey");

            entity.ToTable("Invoice", "PizzaShop");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.ModifiedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.ModifierId).HasColumnName("ModifierID");
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.RateOfModifier).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Item_pkey");

            entity.ToTable("Item", "PizzaShop");

            entity.HasIndex(e => e.Name, "Item_Name_key").IsUnique();

            entity.HasIndex(e => e.ShortCode, "Item_ShortCode_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.DefaultTax)
                .IsRequired()
                .HasDefaultValueSql("true");
            entity.Property(e => e.ModifiedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.ShortCode).HasMaxLength(5);
            entity.Property(e => e.TextPercentage).HasPrecision(4, 2);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);

            entity.HasOne(d => d.Category).WithMany(p => p.Items)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("Item_CategoryID_fkey");

            entity.HasOne(d => d.ItemTypeNavigation).WithMany(p => p.Items)
                .HasForeignKey(d => d.ItemType)
                .HasConstraintName("Item_ItemType_fkey");

            entity.HasOne(d => d.UnitNavigation).WithMany(p => p.Items)
                .HasForeignKey(d => d.Unit)
                .HasConstraintName("Item_Unit_fkey");
        });

        modelBuilder.Entity<ItemsModifier>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ItemsModifier_pkey");

            entity.ToTable("ItemsModifier", "PizzaShop");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.DeletedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.ModifiedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.ModifierGorupId).HasColumnName("ModifierGorupID");

            entity.HasOne(d => d.Item).WithMany(p => p.ItemsModifiers)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("ItemsModifier_ItemID_fkey");
        });

        modelBuilder.Entity<MeasuringUnit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("MeasuringUnits_pkey");

            entity.ToTable("MeasuringUnits", "PizzaShop");

            entity.HasIndex(e => e.Name, "MeasuringUnits_Name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Description).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Modifier>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Modifier_pkey");

            entity.ToTable("Modifier", "PizzaShop");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.ModifiedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Name).HasMaxLength(250);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ModifierCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("Modifier_CreatedBy_fkey");

            entity.HasOne(d => d.ModifiedByNavigation).WithMany(p => p.ModifierModifiedByNavigations)
                .HasForeignKey(d => d.ModifiedBy)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Modifier_ModifiedBy_fkey");

            entity.HasOne(d => d.ModifierGroup).WithMany(p => p.Modifiers)
                .HasForeignKey(d => d.ModifierGroupId)
                .HasConstraintName("Modifier_ModifierGroupId_fkey");
        });

        modelBuilder.Entity<ModifierGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ModifierGroup_pkey");

            entity.ToTable("ModifierGroup", "PizzaShop");

            entity.HasIndex(e => e.Name, "ModifierGroup_Name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.ModifiedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Name).HasMaxLength(250);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Order_pkey");

            entity.ToTable("Order", "PizzaShop");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.Date).HasDefaultValueSql("now()");
            entity.Property(e => e.IsAvaiable)
                .IsRequired()
                .HasDefaultValueSql("true");
            entity.Property(e => e.ModifiedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.PaidAmount).HasPrecision(18, 2);
            entity.Property(e => e.SubAmount).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.OrderCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("Order_CreatedBy_fkey");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Order_CustomerID_fkey");

            entity.HasOne(d => d.ModifiedByNavigation).WithMany(p => p.OrderModifiedByNavigations)
                .HasForeignKey(d => d.ModifiedBy)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Order_ModifiedBy_fkey");

            entity.HasOne(d => d.PymentModeNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.PymentMode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Order_PymentMode_fkey");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("OrderItem_pkey");

            entity.ToTable("OrderItem", "PizzaShop");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.ItemName).HasMaxLength(250);
            entity.Property(e => e.ModifiedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.Rate).HasPrecision(18, 2);
            entity.Property(e => e.TaxPercantage).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);

            entity.HasOne(d => d.Item).WithMany(p => p.OrderItemItems)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("OrderItem_ItemID_fkey");

            entity.HasOne(d => d.ItemNameNavigation).WithMany(p => p.OrderItemItemNameNavigations)
                .HasPrincipalKey(p => p.Name)
                .HasForeignKey(d => d.ItemName)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("OrderItem_ItemName_fkey");

            entity.HasOne(d => d.OrderStatusNavigation).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderStatus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("OrderItem_OrderStatus_fkey");
        });

        modelBuilder.Entity<OrderModifier>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("OrderModifier_pkey");

            entity.ToTable("OrderModifier", "PizzaShop");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.ModifiedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Modifierid).HasColumnName("MODIFIERID");
            entity.Property(e => e.OrderItemId).HasColumnName("OrderItemID");
            entity.Property(e => e.Rate).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<OrderStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("OrderStatus_pkey");

            entity.ToTable("OrderStatus", "PizzaShop");

            entity.HasIndex(e => e.Name, "OrderStatus_Name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(250);
        });

        modelBuilder.Entity<OrdersTax>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("OrdersTax_pkey");

            entity.ToTable("OrdersTax", "PizzaShop");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.TaxId).HasColumnName("TaxID");

            entity.HasOne(d => d.Tax).WithMany(p => p.OrdersTaxes)
                .HasForeignKey(d => d.TaxId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("OrdersTax_TaxID_fkey");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Payment_pkey");

            entity.ToTable("Payment", "PizzaShop");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.InvoiceId).HasColumnName("InvoiceID");
            entity.Property(e => e.PaymentMethodId).HasColumnName("PaymentMethodID");
            entity.Property(e => e.PaymentStatusId).HasColumnName("PaymentStatusID");
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);

            entity.HasOne(d => d.Invoice).WithMany(p => p.Payments)
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Payment_InvoiceID_fkey");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.Payments)
                .HasForeignKey(d => d.PaymentMethodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Payment_PaymentMethodID_fkey");

            entity.HasOne(d => d.PaymentStatus).WithMany(p => p.Payments)
                .HasForeignKey(d => d.PaymentStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Payment_PaymentStatusID_fkey");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.PaymentMethodId).HasName("PaymentMethod_pkey");

            entity.ToTable("PaymentMethod", "PizzaShop");

            entity.HasIndex(e => e.Name, "PaymentMethod_Name_key").IsUnique();

            entity.Property(e => e.PaymentMethodId).HasColumnName("PaymentMethodID");
            entity.Property(e => e.Name).HasMaxLength(250);
        });

        modelBuilder.Entity<PaymentMode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PaymentMode_pkey");

            entity.ToTable("PaymentMode", "PizzaShop");

            entity.HasIndex(e => e.Name, "PaymentMode_Name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(250);
        });

        modelBuilder.Entity<PaymentStatus>(entity =>
        {
            entity.HasKey(e => e.PaymentStatusId).HasName("PaymentStatus_pkey");

            entity.ToTable("PaymentStatus", "PizzaShop");

            entity.HasIndex(e => e.Name, "PaymentStatus_Name_key").IsUnique();

            entity.Property(e => e.PaymentStatusId).HasColumnName("PaymentStatusID");
            entity.Property(e => e.Name).HasMaxLength(250);
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Permissions_pkey");

            entity.ToTable("Permissions", "PizzaShop");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.ModifiedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Name).HasMaxLength(250);
        });

        modelBuilder.Entity<RoleWisePermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("RoleWisePermissions_pkey");

            entity.ToTable("RoleWisePermissions", "PizzaShop");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.ModifiedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.PermissionId).HasColumnName("PermissionID");
            entity.Property(e => e.RoleId).HasColumnName("RoleID");

            entity.HasOne(d => d.Permission).WithMany(p => p.RoleWisePermissions)
                .HasForeignKey(d => d.PermissionId)
                .HasConstraintName("RoleWisePermissions_PermissionID_fkey");

            entity.HasOne(d => d.Role).WithMany(p => p.RoleWisePermissions)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("RoleWisePermissions_RoleID_fkey");
        });

        modelBuilder.Entity<Section>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Section_pkey");

            entity.ToTable("Section", "PizzaShop");

            entity.HasIndex(e => e.Name, "Section_Name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.ModifiedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<State>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("State_pkey");

            entity.ToTable("State", "PizzaShop");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CountryId).HasColumnName("CountryID");
            entity.Property(e => e.Name).HasMaxLength(250);

            entity.HasOne(d => d.Country).WithMany(p => p.States)
                .HasForeignKey(d => d.CountryId)
                .HasConstraintName("State_CountryID_fkey");
        });

        modelBuilder.Entity<Table>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Table_pkey");

            entity.ToTable("Table", "PizzaShop");

            entity.HasIndex(e => e.Name, "Table_Name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.IsAvaiable)
                .IsRequired()
                .HasDefaultValueSql("true");
            entity.Property(e => e.ModifiedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Name).HasMaxLength(250);
            entity.Property(e => e.SectionId).HasColumnName("SectionID");

            entity.HasOne(d => d.Section).WithMany(p => p.Tables)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Table_SectionID_fkey");

            entity.HasOne(d => d.StatusNavigation).WithMany(p => p.Tables)
                .HasForeignKey(d => d.Status)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Table_Status_fkey");
        });

        modelBuilder.Entity<TableOrderMapping>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("TableOrderMapping_pkey");

            entity.ToTable("TableOrderMapping", "PizzaShop");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.ModifiedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.TableId).HasColumnName("TableID");
        });

        modelBuilder.Entity<TableStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("TableStatus_pkey");

            entity.ToTable("TableStatus", "PizzaShop");

            entity.HasIndex(e => e.Name, "TableStatus_Name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(250);
        });

        modelBuilder.Entity<TaxAndFee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("TaxAndFee_pkey");

            entity.ToTable("TaxAndFee", "PizzaShop");

            entity.HasIndex(e => e.Name, "TaxAndFee_Name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.IsEnabled)
                .IsRequired()
                .HasDefaultValueSql("true");
            entity.Property(e => e.ModifiedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Name).HasMaxLength(250);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Users_pkey");

            entity.ToTable("Users", "PizzaShop");

            entity.HasIndex(e => e.Email, "Users_Email_key").IsUnique();

            entity.HasIndex(e => e.UserName, "Users_UserName_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CityId).HasColumnName("CityID");
            entity.Property(e => e.CountryId).HasColumnName("CountryID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Email).HasMaxLength(250);
            entity.Property(e => e.FirstName).HasMaxLength(250);
            entity.Property(e => e.IsActive).HasDefaultValueSql("true");
            entity.Property(e => e.IsFirstTime)
                .IsRequired()
                .HasDefaultValueSql("true");
            entity.Property(e => e.LastName).HasMaxLength(250);
            entity.Property(e => e.ModifiedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Password).HasMaxLength(250);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.StateId).HasColumnName("StateID");
            entity.Property(e => e.UserName).HasMaxLength(250);
            entity.Property(e => e.UserRoleId).HasColumnName("UserRoleID");
            entity.Property(e => e.ZipCode).HasMaxLength(50);

            entity.HasOne(d => d.City).WithMany(p => p.Users)
                .HasForeignKey(d => d.CityId)
                .HasConstraintName("Users_CityID_fkey");

            entity.HasOne(d => d.Country).WithMany(p => p.Users)
                .HasForeignKey(d => d.CountryId)
                .HasConstraintName("Users_CountryID_fkey");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InverseCreatedByNavigation)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Users_CreatedBy_fkey");

            entity.HasOne(d => d.ModifiedByNavigation).WithMany(p => p.InverseModifiedByNavigation)
                .HasForeignKey(d => d.ModifiedBy)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Users_ModifiedBy_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.Users)
                .HasForeignKey(d => d.StateId)
                .HasConstraintName("Users_StateID_fkey");

            entity.HasOne(d => d.UserRole).WithMany(p => p.Users)
                .HasForeignKey(d => d.UserRoleId)
                .HasConstraintName("Users_UserRoleID_fkey");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("UserRole_pkey");

            entity.ToTable("UserRole", "PizzaShop");

            entity.HasIndex(e => e.Name, "UserRole_Name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Wating>(entity =>
        {
            entity.HasKey(e => e.WatingId).HasName("Wating_pkey");

            entity.ToTable("Wating", "PizzaShop");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.ModifiedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.SectionId).HasColumnName("SectionID");

            entity.HasOne(d => d.Customer).WithMany(p => p.Watings)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Wating_CustomerID_fkey");

            entity.HasOne(d => d.Section).WithMany(p => p.Watings)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Wating_SectionID_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
