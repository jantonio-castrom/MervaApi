using MervaApi.UserExpenses.Models;
using MervaApi.UserIncomes.Models;
using MervaApi.UserPreferences.Models;
using MervaApi.UserTokens.Models;
using Microsoft.EntityFrameworkCore;

namespace MervaApi.Data;

public class MervaDbContext(DbContextOptions<MervaDbContext> options) : DbContext(options)
{
    public DbSet<UserToken> UserTokens => Set<UserToken>();
    public DbSet<UserDevice> UserDevices => Set<UserDevice>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<UserIncome> UserIncomes => Set<UserIncome>();
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserToken>(entity =>
        {
            entity.ToTable("UserTokens");
            entity.HasKey(e => e.TokenId);
            entity.Property(e => e.TokenId).UseIdentityColumn();
            entity.Property(e => e.Token).IsRequired().HasMaxLength(64);
            entity.Property(e => e.EncryptedValueHash).IsRequired().HasColumnType("VARBINARY(32)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasIndex(e => e.Token).IsUnique().HasDatabaseName("UQ_UserTokens_Token");
        });

        modelBuilder.Entity<UserDevice>(entity =>
        {
            entity.ToTable("UserDevices");
            entity.HasKey(e => e.DeviceId);
            entity.Property(e => e.DeviceId).UseIdentityColumn();
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.Browser).HasMaxLength(100);
            entity.Property(e => e.BrowserVersion).HasMaxLength(50);
            entity.Property(e => e.OperatingSystem).HasMaxLength(100);
            entity.Property(e => e.Language).HasMaxLength(20);
            entity.Property(e => e.Timezone).HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.Region).HasMaxLength(100);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Isp).HasMaxLength(200);
            entity.Property(e => e.ConnectionType).HasMaxLength(50);
            entity.Property(e => e.RecordedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(e => e.UserToken)
                  .WithMany(t => t.Devices)
                  .HasForeignKey(e => e.TokenId)
                  .HasConstraintName("FK_UserDevices_UserTokens");
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.ToTable("Expenses");
            entity.HasKey(e => e.ExpenseId);
            entity.Property(e => e.ExpenseId).UseIdentityColumn();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Currency).IsRequired().IsFixedLength().HasMaxLength(3).HasDefaultValue("USD");
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.DeletedAt).IsRequired(false);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasOne(e => e.UserToken)
                  .WithMany(t => t.Expenses)
                  .HasForeignKey(e => e.TokenId)
                  .HasConstraintName("FK_Expenses_UserTokens");
        });

        modelBuilder.Entity<UserIncome>(entity =>
        {
            entity.ToTable("UserIncomes");
            entity.HasKey(e => e.IncomeId);
            entity.Property(e => e.IncomeId).UseIdentityColumn();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Currency).IsRequired().IsFixedLength().HasMaxLength(3).HasDefaultValue("USD");
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.DeletedAt).IsRequired(false);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasOne(e => e.UserToken)
                  .WithMany(t => t.Incomes)
                  .HasForeignKey(e => e.TokenId)
                  .HasConstraintName("FK_UserIncomes_UserTokens");
        });

        modelBuilder.Entity<UserPreference>(entity =>
        {
            entity.ToTable("UserPreferences");
            entity.HasKey(e => e.PreferenceId);
            entity.Property(e => e.PreferenceId).UseIdentityColumn();
            entity.Property(e => e.DefaultCurrency).IsRequired().IsFixedLength().HasMaxLength(3).HasDefaultValue("USD");
            entity.Property(e => e.Theme).IsRequired().HasMaxLength(20).HasDefaultValue("light");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasIndex(e => e.TokenId).IsUnique().HasDatabaseName("UQ_UserPreferences_TokenId");
            entity.HasOne(e => e.UserToken)
                  .WithOne(t => t.Preference)
                  .HasForeignKey<UserPreference>(e => e.TokenId)
                  .HasConstraintName("FK_UserPreferences_UserTokens");
        });
    }
}
