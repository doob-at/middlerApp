﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using middlerApp.Auth.Context;

namespace middlerApp.Auth.Sqlite.Migrations
{
    [DbContext(typeof(AuthDbContext))]
    partial class AuthDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.7");

            modelBuilder.Entity("middlerApp.Auth.Entities.AuthApplication", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int?>("AccessTokenLifeTime")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClientId")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("ClientSecret")
                        .HasColumnType("TEXT");

                    b.Property<string>("ConcurrencyToken")
                        .IsConcurrencyToken()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("ConsentType")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("DisplayName")
                        .HasColumnType("TEXT");

                    b.Property<string>("DisplayNames")
                        .HasColumnType("TEXT");

                    b.Property<string>("Permissions")
                        .HasColumnType("TEXT");

                    b.Property<string>("PostLogoutRedirectUris")
                        .HasColumnType("TEXT");

                    b.Property<string>("Properties")
                        .HasColumnType("TEXT");

                    b.Property<string>("RedirectUris")
                        .HasColumnType("TEXT");

                    b.Property<int?>("RefreshTokenLifeTime")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Requirements")
                        .HasColumnType("TEXT");

                    b.Property<string>("Type")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ClientId")
                        .IsUnique();

                    b.ToTable("AuthApplications");
                });

            modelBuilder.Entity("middlerApp.Auth.Entities.AuthAuthorization", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("ApplicationId")
                        .HasColumnType("TEXT");

                    b.Property<string>("ConcurrencyToken")
                        .IsConcurrencyToken()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("CreationDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Properties")
                        .HasColumnType("TEXT");

                    b.Property<string>("Scopes")
                        .HasColumnType("TEXT");

                    b.Property<string>("Status")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("Subject")
                        .HasMaxLength(400)
                        .HasColumnType("TEXT");

                    b.Property<string>("Type")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ApplicationId", "Status", "Subject", "Type");

                    b.ToTable("AuthAuthorizations");
                });

            modelBuilder.Entity("middlerApp.Auth.Entities.AuthScope", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("ConcurrencyToken")
                        .IsConcurrencyToken()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("Descriptions")
                        .HasColumnType("TEXT");

                    b.Property<string>("DisplayName")
                        .HasColumnType("TEXT");

                    b.Property<string>("DisplayNames")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<string>("Properties")
                        .HasColumnType("TEXT");

                    b.Property<string>("Resources")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("AuthScopes");
                });

            modelBuilder.Entity("middlerApp.Auth.Entities.AuthToken", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("ApplicationId")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("AuthorizationId")
                        .HasColumnType("TEXT");

                    b.Property<string>("ConcurrencyToken")
                        .IsConcurrencyToken()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("CreationDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("ExpirationDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Payload")
                        .HasColumnType("TEXT");

                    b.Property<string>("Properties")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("RedemptionDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("ReferenceId")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("Status")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("Subject")
                        .HasMaxLength(400)
                        .HasColumnType("TEXT");

                    b.Property<string>("Type")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("AuthorizationId");

                    b.HasIndex("ReferenceId")
                        .IsUnique();

                    b.HasIndex("ApplicationId", "Status", "Subject", "Type");

                    b.ToTable("AuthTokens");
                });

            modelBuilder.Entity("middlerApp.Auth.Entities.AuthAuthorization", b =>
                {
                    b.HasOne("middlerApp.Auth.Entities.AuthApplication", "Application")
                        .WithMany("Authorizations")
                        .HasForeignKey("ApplicationId");

                    b.Navigation("Application");
                });

            modelBuilder.Entity("middlerApp.Auth.Entities.AuthToken", b =>
                {
                    b.HasOne("middlerApp.Auth.Entities.AuthApplication", "Application")
                        .WithMany("Tokens")
                        .HasForeignKey("ApplicationId");

                    b.HasOne("middlerApp.Auth.Entities.AuthAuthorization", "Authorization")
                        .WithMany("Tokens")
                        .HasForeignKey("AuthorizationId");

                    b.Navigation("Application");

                    b.Navigation("Authorization");
                });

            modelBuilder.Entity("middlerApp.Auth.Entities.AuthApplication", b =>
                {
                    b.Navigation("Authorizations");

                    b.Navigation("Tokens");
                });

            modelBuilder.Entity("middlerApp.Auth.Entities.AuthAuthorization", b =>
                {
                    b.Navigation("Tokens");
                });
#pragma warning restore 612, 618
        }
    }
}
