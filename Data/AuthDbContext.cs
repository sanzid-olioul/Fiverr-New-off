using System;
using System.Collections.Generic;
using LancasterCreditCardDiversion.Models;
using LancasterCreditCardDiversion.ViewModels;
using Microsoft.EntityFrameworkCore;
using AppDomain = LancasterCreditCardDiversion.Models.AppDomain;

namespace LancasterCreditCardDiversion.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    public virtual DbSet<AuthenticateUserResult> AuthenticateUserResults { get; set; }

    public virtual DbSet<ResetPasswordResult> ResetPasswordResult { get; set; }
    public virtual DbSet<CreateUserResult> CreateUserResult { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //modelBuilder
        //    .HasDefaultSchema("APPDBA")
        //    .UseCollation("USING_NLS_COMP");

            modelBuilder.Entity<AuthenticateUserResult>().HasNoKey();

            modelBuilder.Entity<ResetPasswordResult>().HasNoKey();

            modelBuilder.Entity<CreateUserResult>().HasNoKey();

    }
}

