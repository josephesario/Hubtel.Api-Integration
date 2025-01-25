using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace dbContex.Models;

public partial class HubtelWalletDbContextExtended : DbContext
{

    private readonly IConfiguration _configuration;

    public HubtelWalletDbContextExtended(DbContextOptions<HubtelWalletDbContextExtended> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }

    public virtual DbSet<TCardAccountDetail>? TCardAccountDetails { get; set; }
    public virtual DbSet<TCardType>? TCardTypes { get; set; }
    public virtual DbSet<TPhoneAccountDetail>? TPhoneAccountDetails { get; set; }
    public virtual DbSet<TSimcardType>? TSimcardTypes { get; set; }
    public virtual DbSet<TUserAccess>? TUserAccesses { get; set; }
    public virtual DbSet<TUserProfile>? TUserProfiles { get; set; }
    public virtual DbSet<TUserType>? TUserTypes { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(_configuration.GetConnectionString("HubtelWalletDbContext"));
        }
    }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TCardAccountDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__t_Card_A__3214EC27F932A28C");

            entity.ToTable("t_Card_AccountDetails");

            entity.HasIndex(e => e.CardNumber, "UQ__t_Card_A__A4E9FFE92328DED2").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ID");
            entity.Property(e => e.CardNumber)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.CardTypeId).HasColumnName("CardTypeID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UserAccessId).HasColumnName("UserAccessID");
            entity.Property(e => e.UserProfileId).HasColumnName("UserProfileID");

            entity.HasOne(d => d.CardType).WithMany(p => p.TCardAccountDetails)
                .HasForeignKey(d => d.CardTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_t_Card_AccountDetails_And_t_CardType");

            entity.HasOne(d => d.UserAccess).WithMany(p => p.TCardAccountDetails)
                .HasForeignKey(d => d.UserAccessId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_t_Card_AccountDetails_And_t_UserAccess");

            entity.HasOne(d => d.UserProfile).WithMany(p => p.TCardAccountDetails)
                .HasForeignKey(d => d.UserProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_t_Card_AccountDetails_And_t_UserProfile");
        });

        modelBuilder.Entity<TCardType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__t_CardTy__3214EC279B346BA0");

            entity.ToTable("t_CardType");

            entity.HasIndex(e => e.Name, "UQ__t_CardTy__737584F69B119427").IsUnique();

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

        modelBuilder.Entity<TPhoneAccountDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__t_Phone___3214EC27C48322A1");

            entity.ToTable("t_Phone_AccountDetails");

            entity.HasIndex(e => e.PhoneNumber, "UQ__t_Phone___85FB4E382F5D5121").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.SimCardTypeId).HasColumnName("SimCardTypeID");
            entity.Property(e => e.UserAccessId).HasColumnName("UserAccessID");
            entity.Property(e => e.UserProfileId).HasColumnName("UserProfileID");

            entity.HasOne(d => d.SimCardType).WithMany(p => p.TPhoneAccountDetails)
                .HasForeignKey(d => d.SimCardTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_t_Phone_AccountDetails_And_t_SimcardType");

            entity.HasOne(d => d.UserAccess).WithMany(p => p.TPhoneAccountDetails)
                .HasForeignKey(d => d.UserAccessId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_t_Phone_AccountDetails_And_t_UserAccess");

            entity.HasOne(d => d.UserProfile).WithMany(p => p.TPhoneAccountDetails)
                .HasForeignKey(d => d.UserProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_t_Phone_AccountDetails_And_t_UserProfile");
        });

        modelBuilder.Entity<TSimcardType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__t_Simcar__3214EC2755309AF0");

            entity.ToTable("t_SimcardType");

            entity.HasIndex(e => e.Name, "UQ__t_Simcar__737584F6A1D2F9C3").IsUnique();

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

        modelBuilder.Entity<TUserAccess>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__t_UserAc__3214EC27786A1ADC");

            entity.ToTable("t_UserAccess");

            entity.HasIndex(e => e.EmailPhoneNumber, "UQ__t_UserAc__CBD1C135CDA26228").IsUnique();

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
            entity.Property(e => e.UserTypeId).HasColumnName("UserTypeID");

            entity.HasOne(d => d.UserType).WithMany(p => p.TUserAccesses)
                .HasForeignKey(d => d.UserTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_t_UserAccess_And_t_UserType");
        });

        modelBuilder.Entity<TUserProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__t_UserPr__3214EC2749914830");

            entity.ToTable("t_UserProfile");

            entity.HasIndex(e => e.LegalName, "UQ__t_UserPr__07D0C9F885E31C87").IsUnique();

            entity.HasIndex(e => e.IdentityCardNumber, "UQ__t_UserPr__59CD512115EEBE64").IsUnique();

            entity.HasIndex(e => e.EmailPhone, "UQ__t_UserPr__85FB4E38C02B8134").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IdentityCardNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.LegalName)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.EmailPhone)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.UserAccessId).HasColumnName("UserAccessID");

            entity.HasOne(d => d.UserAccess).WithMany(p => p.TUserProfiles)
                .HasForeignKey(d => d.UserAccessId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_t_UserAccess_And_t_UserProfile");
        });

        modelBuilder.Entity<TUserType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__t_UserTy__3214EC27F2EB6C55");

            entity.ToTable("t_UserType");

            entity.HasIndex(e => e.Name, "UQ__t_UserTy__737584F628CC73B2").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(4);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
