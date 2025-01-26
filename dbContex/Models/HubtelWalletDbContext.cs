using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace dbContex.Models;

public partial class HubtelWalletDbContext : DbContext
{
    private readonly IConfiguration _configuration;

    public HubtelWalletDbContext(DbContextOptions<HubtelWalletDbContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }

    public virtual DbSet<TCardType> TCardTypes { get; set; }

    public virtual DbSet<TSimcardType> TSimcardTypes { get; set; }

    public virtual DbSet<TType> TTypes { get; set; }

    public virtual DbSet<TUserAccess> TUserAccesses { get; set; }

    public virtual DbSet<TUserProfile> TUserProfiles { get; set; }

    public virtual DbSet<TWalletAccountDetail> TWalletAccountDetails { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(_configuration.GetConnectionString("HubtelWalletDbContext"));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TCardType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__t_CardTy__3214EC27B50F2D27");

            entity.ToTable("t_CardType");

            entity.HasIndex(e => e.Name, "UQ__t_CardTy__737584F6BFA9578B").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(11)
                .IsUnicode(false)
                .IsFixedLength();
        });

        modelBuilder.Entity<TSimcardType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__t_Simcar__3214EC278CC244D1");

            entity.ToTable("t_SimcardType");

            entity.HasIndex(e => e.Name, "UQ__t_Simcar__737584F64EE1284E").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
        });

        modelBuilder.Entity<TType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__t_Type__3214EC27A5B38D6E");

            entity.ToTable("t_Type");

            entity.HasIndex(e => e.Name, "UQ__t_Type__737584F6C0C25D4B").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(4);
        });

        modelBuilder.Entity<TUserAccess>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__t_UserAc__3214EC2719B92381");

            entity.ToTable("t_UserAccess");

            entity.HasIndex(e => e.EmailPhoneNumber, "UQ__t_UserAc__CBD1C1351F76A058").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EmailPhoneNumber)
                .HasMaxLength(120)
                .IsUnicode(false)
                .HasColumnName("Email_PhoneNumber");
            entity.Property(e => e.UserSecret)
                .HasMaxLength(500)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TUserProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__t_UserPr__3214EC2713FAFE97");

            entity.ToTable("t_UserProfile");

            entity.HasIndex(e => e.LegalName, "UQ__t_UserPr__07D0C9F8B4F0ACE3").IsUnique();

            entity.HasIndex(e => e.IdentityCardNumber, "UQ__t_UserPr__59CD512129CEC45C").IsUnique();

            entity.HasIndex(e => e.EmailPhone, "UQ__t_UserPr__7213B70D7056D35E").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EmailPhone)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.IdentityCardNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.LegalName)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.UserAccessId).HasColumnName("UserAccessID");

            entity.HasOne(d => d.UserAccess).WithMany(p => p.TUserProfiles)
                .HasForeignKey(d => d.UserAccessId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_t_UserAccess_And_t_UserProfile");
        });

        modelBuilder.Entity<TWalletAccountDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__t_Wallet__3214EC2725044853");

            entity.ToTable("t_WalletAccountDetails");

            entity.HasIndex(e => e.AccountNumber, "UQ__t_Wallet__BE2ACD6F74114AB8").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ID");
            entity.Property(e => e.AccountNumber)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.CardTypeId).HasColumnName("CardTypeID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SimCardTypeId).HasColumnName("SimCardTypeID");
            entity.Property(e => e.UserAccessId).HasColumnName("UserAccessID");
            entity.Property(e => e.UserProfileId).HasColumnName("UserProfileID");

            entity.HasOne(d => d.AccountType).WithMany(p => p.TWalletAccountDetails)
                .HasForeignKey(d => d.AccountTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_t_Card_AccountDetails_And_t_Type");

            entity.HasOne(d => d.CardType).WithMany(p => p.TWalletAccountDetails)
                .HasForeignKey(d => d.CardTypeId)
                .HasConstraintName("fk_t_Card_AccountDetails_And_t_CardType");

            entity.HasOne(d => d.SimCardType).WithMany(p => p.TWalletAccountDetails)
                .HasForeignKey(d => d.SimCardTypeId)
                .HasConstraintName("fk_t_Card_AccountDetails_And_t_SimcardType");

            entity.HasOne(d => d.UserAccess).WithMany(p => p.TWalletAccountDetails)
                .HasForeignKey(d => d.UserAccessId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_t_Card_AccountDetails_And_t_UserAccess");

            entity.HasOne(d => d.UserProfile).WithMany(p => p.TWalletAccountDetails)
                .HasForeignKey(d => d.UserProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_t_Card_AccountDetails_And_t_UserProfile");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
