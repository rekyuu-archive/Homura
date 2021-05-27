﻿// <auto-generated />
using System;
using HomuraApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HomuraApi.Migrations
{
    [DbContext(typeof(ArtistContext))]
    [Migration("20210527052446_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.6");

            modelBuilder.Entity("HomuraApi.Models.Artist", b =>
                {
                    b.Property<long>("TwitterId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long?>("LastProcessedTweetId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TwitterUsername")
                        .HasColumnType("TEXT");

                    b.HasKey("TwitterId");

                    b.ToTable("Artists");
                });
#pragma warning restore 612, 618
        }
    }
}
