﻿// <auto-generated />
using System;
using Eu.EDelivery.AS4.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Eu.EDelivery.AS4.Migrations
{
    [DbContext(typeof(DatastoreContext))]
    [Migration("20180828113700_ReplaceExceptionBodyWithLocation")]
    partial class ReplaceExceptionBodyWithLocation
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.0-rtm-30799");

            modelBuilder.Entity("Eu.EDelivery.AS4.Entities.InException", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("EbmsRefToMessageId")
                        .HasMaxLength(256);

                    b.Property<string>("Exception");

                    b.Property<DateTimeOffset>("InsertionTime");

                    b.Property<string>("MessageLocation");

                    b.Property<DateTimeOffset>("ModificationTime");

                    b.Property<string>("Operation")
                        .IsRequired()
                        .HasColumnName("Operation")
                        .HasMaxLength(50);

                    b.Property<string>("PMode");

                    b.Property<string>("PModeId")
                        .HasMaxLength(256);

                    b.HasKey("Id")
                        .HasName("PK_InExceptions");

                    b.HasIndex("EbmsRefToMessageId")
                        .HasName("IX_InExceptions_EbmsRefToMessageId");

                    b.HasIndex("Operation")
                        .HasName("IX_InExceptions_Operation");

                    b.ToTable("InExceptions");
                });

            modelBuilder.Entity("Eu.EDelivery.AS4.Entities.InMessage", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Action")
                        .HasMaxLength(255);

                    b.Property<string>("ContentType")
                        .HasMaxLength(256);

                    b.Property<string>("ConversationId")
                        .HasMaxLength(50);

                    b.Property<string>("EbmsMessageId")
                        .HasMaxLength(256);

                    b.Property<string>("EbmsMessageType")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<string>("EbmsRefToMessageId")
                        .HasMaxLength(256);

                    b.Property<string>("FromParty")
                        .HasMaxLength(255);

                    b.Property<DateTimeOffset>("InsertionTime");

                    b.Property<bool>("Intermediary");

                    b.Property<bool>("IsDuplicate");

                    b.Property<bool>("IsTest");

                    b.Property<string>("MEP")
                        .IsRequired()
                        .HasColumnName("MEP")
                        .HasMaxLength(25);

                    b.Property<string>("MessageLocation")
                        .HasMaxLength(512);

                    b.Property<DateTimeOffset>("ModificationTime");

                    b.Property<string>("Mpc")
                        .HasColumnName("MPC")
                        .HasMaxLength(255);

                    b.Property<string>("Operation")
                        .IsRequired()
                        .HasColumnName("Operation")
                        .HasMaxLength(50);

                    b.Property<string>("PMode");

                    b.Property<string>("PModeId")
                        .HasMaxLength(256);

                    b.Property<string>("Service")
                        .HasMaxLength(255);

                    b.Property<string>("SoapEnvelope");

                    b.Property<string>("Status")
                        .HasColumnName("Status")
                        .HasMaxLength(50);

                    b.Property<string>("ToParty")
                        .HasMaxLength(255);

                    b.HasKey("Id")
                        .HasName("PK_InMessages");

                    b.HasIndex("EbmsRefToMessageId")
                        .HasName("IX_InMessages_EbmsRefToMessageId");

                    b.HasIndex("EbmsMessageId", "IsDuplicate")
                        .HasName("IX_InMessages_EbmsMessageId_IsDuplicate");

                    b.HasIndex("Operation", "InsertionTime")
                        .HasName("IX_InMessages_Operation_InsertionTime");

                    b.ToTable("InMessages");
                });

            modelBuilder.Entity("Eu.EDelivery.AS4.Entities.Journal", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("AgentName")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<string>("AgentType")
                        .IsRequired()
                        .HasMaxLength(20);

                    b.Property<string>("EbmsMessageId")
                        .IsRequired()
                        .HasMaxLength(100);

                    b.Property<DateTimeOffset>("InsertionTime");

                    b.Property<DateTimeOffset>("LogDate");

                    b.Property<string>("LogEntry")
                        .IsRequired();

                    b.Property<string>("MessageOperation")
                        .IsRequired()
                        .HasMaxLength(20);

                    b.Property<string>("MessageStatus")
                        .IsRequired()
                        .HasMaxLength(20);

                    b.Property<DateTimeOffset>("ModificationTime");

                    b.Property<long?>("RefToInMessageId");

                    b.Property<long?>("RefToOutMessageId");

                    b.HasKey("Id")
                        .HasName("PK_Journal");

                    b.ToTable("Journal");
                });

            modelBuilder.Entity("Eu.EDelivery.AS4.Entities.OutException", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("EbmsRefToMessageId")
                        .HasMaxLength(256);

                    b.Property<string>("Exception");

                    b.Property<DateTimeOffset>("InsertionTime");

                    b.Property<string>("MessageLocation");

                    b.Property<DateTimeOffset>("ModificationTime");

                    b.Property<string>("Operation")
                        .IsRequired()
                        .HasColumnName("Operation")
                        .HasMaxLength(50);

                    b.Property<string>("PMode");

                    b.Property<string>("PModeId")
                        .HasMaxLength(256);

                    b.HasKey("Id")
                        .HasName("PK_OutExceptions");

                    b.HasIndex("EbmsRefToMessageId")
                        .HasName("IX_OutExceptions_EbmsRefToMessageId");

                    b.HasIndex("Operation")
                        .HasName("IX_OutExceptions_Operation");

                    b.ToTable("OutExceptions");
                });

            modelBuilder.Entity("Eu.EDelivery.AS4.Entities.OutMessage", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Action")
                        .HasMaxLength(255);

                    b.Property<string>("ContentType")
                        .HasMaxLength(256);

                    b.Property<string>("ConversationId")
                        .HasMaxLength(50);

                    b.Property<string>("EbmsMessageId")
                        .HasMaxLength(256);

                    b.Property<string>("EbmsMessageType")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<string>("EbmsRefToMessageId")
                        .HasMaxLength(256);

                    b.Property<string>("FromParty")
                        .HasMaxLength(255);

                    b.Property<DateTimeOffset>("InsertionTime");

                    b.Property<bool>("Intermediary");

                    b.Property<bool>("IsDuplicate");

                    b.Property<bool>("IsTest");

                    b.Property<string>("MEP")
                        .IsRequired()
                        .HasColumnName("MEP")
                        .HasMaxLength(25);

                    b.Property<string>("MessageLocation")
                        .HasMaxLength(512);

                    b.Property<DateTimeOffset>("ModificationTime");

                    b.Property<string>("Mpc")
                        .HasColumnName("MPC")
                        .HasMaxLength(255);

                    b.Property<string>("Operation")
                        .IsRequired()
                        .HasColumnName("Operation")
                        .HasMaxLength(50);

                    b.Property<string>("PMode");

                    b.Property<string>("PModeId")
                        .HasMaxLength(256);

                    b.Property<string>("Service")
                        .HasMaxLength(255);

                    b.Property<string>("SoapEnvelope");

                    b.Property<string>("Status")
                        .HasColumnName("Status")
                        .HasMaxLength(50);

                    b.Property<string>("ToParty")
                        .HasMaxLength(255);

                    b.HasKey("Id")
                        .HasName("PK_OutMessages");

                    b.HasIndex("EbmsMessageId")
                        .HasName("IX_OutMessages_EbmsMessageId");

                    b.HasIndex("EbmsRefToMessageId")
                        .HasName("IX_OutMessages_EbmsRefToMessageId");

                    b.HasIndex("InsertionTime")
                        .HasName("IX_OutMessages_InsertionTime");

                    b.HasIndex("Operation", "MEP", "Mpc", "InsertionTime")
                        .HasName("IX_OutMessages_Operation_MEP_MPC_InsertionTime");

                    b.ToTable("OutMessages");
                });

            modelBuilder.Entity("Eu.EDelivery.AS4.Entities.RetryReliability", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("CurrentRetryCount");

                    b.Property<DateTimeOffset>("InsertionTime");

                    b.Property<DateTimeOffset?>("LastRetryTime");

                    b.Property<int>("MaxRetryCount");

                    b.Property<DateTimeOffset>("ModificationTime");

                    b.Property<long?>("RefToInExceptionId");

                    b.Property<long?>("RefToInMessageId");

                    b.Property<long?>("RefToOutExceptionId");

                    b.Property<long?>("RefToOutMessageId");

                    b.Property<string>("RetryInterval")
                        .IsRequired()
                        .HasConversion(new ValueConverter<string, string>(v => default(string), v => default(string), new ConverterMappingHints(size: 48)))
                        .HasMaxLength(50);

                    b.Property<string>("RetryType")
                        .IsRequired()
                        .HasMaxLength(12);

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(25);

                    b.HasKey("Id")
                        .HasName("PK_RetryReliability");

                    b.ToTable("RetryReliability");
                });

            modelBuilder.Entity("Eu.EDelivery.AS4.Entities.SmpConfiguration", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Action")
                        .HasMaxLength(256);

                    b.Property<string>("EncryptAlgorithm")
                        .HasMaxLength(256);

                    b.Property<int>("EncryptAlgorithmKeySize");

                    b.Property<string>("EncryptKeyDigestAlgorithm")
                        .HasMaxLength(256);

                    b.Property<string>("EncryptKeyMgfAlorithm")
                        .HasMaxLength(256);

                    b.Property<string>("EncryptKeyTransportAlgorithm")
                        .HasMaxLength(256);

                    b.Property<byte[]>("EncryptPublicKeyCertificate");

                    b.Property<string>("EncryptPublicKeyCertificateName");

                    b.Property<bool>("EncryptionEnabled");

                    b.Property<string>("FinalRecipient")
                        .HasMaxLength(256);

                    b.Property<string>("PartyRole")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.Property<string>("PartyType")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.Property<string>("ServiceType")
                        .HasMaxLength(256);

                    b.Property<string>("ServiceValue")
                        .HasMaxLength(256);

                    b.Property<bool>("TlsEnabled")
                        .HasColumnName("TLSEnabled");

                    b.Property<string>("ToPartyId")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.Property<string>("Url")
                        .HasColumnName("URL")
                        .HasMaxLength(2083);

                    b.HasKey("Id")
                        .HasName("PK_SmpConfigurations");

                    b.HasIndex("ToPartyId", "PartyRole", "PartyType")
                        .IsUnique()
                        .HasName("IX_SmpConfigurations_ToPartyId_PartyRole_PartyType");

                    b.ToTable("SmpConfigurations");
                });
#pragma warning restore 612, 618
        }
    }
}
