using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Eu.EDelivery.AS4.Common;

namespace Eu.EDelivery.AS4.Migrations
{
    [DbContext(typeof(DatastoreContext))]
    [Migration("20180302141116_RemoveOutMessagesConstraint")]
    partial class RemoveOutMessagesConstraint
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Eu.EDelivery.AS4.Entities.InException", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field)
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("EbmsRefToMessageId")
                        .HasMaxLength(256)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("Exception")
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<DateTimeOffset>("InsertionTime");

                    b.Property<byte[]>("MessageBody")
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<DateTimeOffset>("ModificationTime");

                    b.Property<string>("Operation")
                        .HasColumnName("Operation")
                        .HasMaxLength(50);

                    b.Property<string>("PMode")
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("PModeId")
                        .HasMaxLength(256)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.HasKey("Id");

                    b.HasIndex("EbmsRefToMessageId");

                    b.HasIndex("Operation");

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
                        .HasMaxLength(50)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("EbmsRefToMessageId")
                        .HasMaxLength(256);

                    b.Property<string>("FromParty")
                        .HasMaxLength(255);

                    b.Property<DateTimeOffset>("InsertionTime");

                    b.Property<bool>("Intermediary");

                    b.Property<bool>("IsDuplicate");

                    b.Property<bool>("IsTest");

                    b.Property<string>("MEP")
                        .HasColumnName("MEP")
                        .HasMaxLength(25)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("MessageLocation")
                        .HasMaxLength(512);

                    b.Property<DateTimeOffset>("ModificationTime");

                    b.Property<string>("Mpc")
                        .HasColumnName("MPC")
                        .HasMaxLength(255);

                    b.Property<string>("Operation")
                        .HasColumnName("Operation")
                        .HasMaxLength(50);

                    b.Property<string>("PMode")
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("PModeId")
                        .HasMaxLength(256)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("Service")
                        .HasMaxLength(255);

                    b.Property<string>("SoapEnvelope");

                    b.Property<string>("Status")
                        .HasColumnName("Status")
                        .HasMaxLength(50);

                    b.Property<string>("ToParty")
                        .HasMaxLength(255);

                    b.HasKey("Id");

                    b.HasIndex("EbmsRefToMessageId");

                    b.HasIndex("EbmsMessageId", "IsDuplicate");

                    b.HasIndex("Operation", "InsertionTime");

                    b.ToTable("InMessages");
                });

            modelBuilder.Entity("Eu.EDelivery.AS4.Entities.OutException", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field)
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("EbmsRefToMessageId")
                        .HasMaxLength(256)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("Exception")
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<DateTimeOffset>("InsertionTime");

                    b.Property<byte[]>("MessageBody")
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<DateTimeOffset>("ModificationTime");

                    b.Property<string>("Operation")
                        .HasColumnName("Operation")
                        .HasMaxLength(50);

                    b.Property<string>("PMode")
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("PModeId")
                        .HasMaxLength(256)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.HasKey("Id");

                    b.HasIndex("EbmsRefToMessageId");

                    b.HasIndex("Operation");

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
                        .HasMaxLength(50)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("EbmsRefToMessageId")
                        .HasMaxLength(256);

                    b.Property<string>("FromParty")
                        .HasMaxLength(255);

                    b.Property<DateTimeOffset>("InsertionTime");

                    b.Property<bool>("Intermediary");

                    b.Property<bool>("IsDuplicate");

                    b.Property<bool>("IsTest");

                    b.Property<string>("MEP")
                        .HasColumnName("MEP")
                        .HasMaxLength(25)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("MessageLocation")
                        .HasMaxLength(512);

                    b.Property<DateTimeOffset>("ModificationTime");

                    b.Property<string>("Mpc")
                        .HasColumnName("MPC")
                        .HasMaxLength(255);

                    b.Property<string>("Operation")
                        .HasColumnName("Operation")
                        .HasMaxLength(50);

                    b.Property<string>("PMode")
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("PModeId")
                        .HasMaxLength(256)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("Service")
                        .HasMaxLength(255);

                    b.Property<string>("SoapEnvelope");

                    b.Property<string>("Status")
                        .HasColumnName("Status")
                        .HasMaxLength(50);

                    b.Property<string>("ToParty")
                        .HasMaxLength(255);

                    b.HasKey("Id");

                    b.HasIndex("EbmsMessageId");

                    b.HasIndex("EbmsRefToMessageId");

                    b.HasIndex("InsertionTime");

                    b.HasIndex("Operation", "MEP", "Mpc", "InsertionTime");

                    b.ToTable("OutMessages");
                });

            modelBuilder.Entity("Eu.EDelivery.AS4.Entities.ReceptionAwareness", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("CurrentRetryCount");

                    b.Property<DateTimeOffset>("InsertionTime");

                    b.Property<DateTimeOffset?>("LastSendTime");

                    b.Property<DateTimeOffset>("ModificationTime");

                    b.Property<string>("RefToEbmsMessageId")
                        .HasMaxLength(256);

                    b.Property<long>("RefToOutMessageId");

                    b.Property<string>("RetryInterval")
                        .HasMaxLength(12);

                    b.Property<string>("Status")
                        .HasMaxLength(25);

                    b.Property<int>("TotalRetryCount");

                    b.HasKey("Id");

                    b.HasAlternateKey("RefToOutMessageId");

                    b.HasIndex("Status", "CurrentRetryCount");

                    b.ToTable("ReceptionAwareness");
                });

            modelBuilder.Entity("Eu.EDelivery.AS4.Entities.SmpConfiguration", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field)
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Action")
                        .HasMaxLength(256)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("EncryptAlgorithm")
                        .HasMaxLength(256)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<int>("EncryptAlgorithmKeySize")
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("EncryptKeyDigestAlgorithm")
                        .HasMaxLength(256)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("EncryptKeyMgfAlorithm")
                        .HasMaxLength(256)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("EncryptKeyTransportAlgorithm")
                        .HasMaxLength(256)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<byte[]>("EncryptPublicKeyCertificate")
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<bool>("EncryptionEnabled")
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("FinalRecipient")
                        .HasMaxLength(256)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("PartyRole")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("PartyType")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("ServiceType")
                        .HasMaxLength(256)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("ServiceValue")
                        .HasMaxLength(256)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<bool>("TlsEnabled")
                        .HasColumnName("TLSEnabled")
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("ToPartyId")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.Property<string>("Url")
                        .HasColumnName("URL")
                        .HasMaxLength(2083)
                        .HasAnnotation("PropertyAccessMode", PropertyAccessMode.Field);

                    b.HasKey("Id");

                    b.HasAlternateKey("ToPartyId", "PartyRole", "PartyType");

                    b.ToTable("SmpConfigurations");
                });
        }
    }
}
