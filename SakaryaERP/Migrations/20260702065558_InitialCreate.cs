using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SakaryaERP.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Aciklama = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BankaHesaplari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HesapAdi = table.Column<string>(type: "text", nullable: false),
                    BankaAdi = table.Column<string>(type: "text", nullable: false),
                    IBAN = table.Column<string>(type: "text", nullable: true),
                    Bakiye = table.Column<decimal>(type: "numeric", nullable: false),
                    ParaBirimi = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankaHesaplari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cariler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CariKodu = table.Column<string>(type: "text", nullable: false),
                    Unvan = table.Column<string>(type: "text", nullable: false),
                    CariTipi = table.Column<int>(type: "integer", nullable: false),
                    VergiNo = table.Column<string>(type: "text", nullable: true),
                    Adres = table.Column<string>(type: "text", nullable: true),
                    Telefon = table.Column<string>(type: "text", nullable: true),
                    EMail = table.Column<string>(type: "text", nullable: true),
                    Bakiye = table.Column<decimal>(type: "numeric", nullable: false),
                    KrediLimiti = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cariler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HesapPlani",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HesapKodu = table.Column<string>(type: "text", nullable: false),
                    HesapAdi = table.Column<string>(type: "text", nullable: false),
                    HesapTipi = table.Column<int>(type: "integer", nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HesapPlani", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HesapPlani_HesapPlani_ParentId",
                        column: x => x.ParentId,
                        principalTable: "HesapPlani",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KasaHesaplari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KasaAdi = table.Column<string>(type: "text", nullable: false),
                    Bakiye = table.Column<decimal>(type: "numeric", nullable: false),
                    ParaBirimi = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KasaHesaplari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MalzemeKategoriler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KategoriAdi = table.Column<string>(type: "text", nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MalzemeKategoriler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MalzemeKategoriler_MalzemeKategoriler_ParentId",
                        column: x => x.ParentId,
                        principalTable: "MalzemeKategoriler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Subeler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubeAdi = table.Column<string>(type: "text", nullable: false),
                    Adres = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subeler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CekSenetler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BelgeTipi = table.Column<int>(type: "integer", nullable: false),
                    BelgeNo = table.Column<string>(type: "text", nullable: false),
                    CariId = table.Column<int>(type: "integer", nullable: false),
                    CiroBilgisi = table.Column<string>(type: "text", nullable: true),
                    VadeTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Tutar = table.Column<decimal>(type: "numeric", nullable: false),
                    BankaAdi = table.Column<string>(type: "text", nullable: true),
                    SubeAdi = table.Column<string>(type: "text", nullable: true),
                    Durum = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CekSenetler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CekSenetler_Cariler_CariId",
                        column: x => x.CariId,
                        principalTable: "Cariler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MusteriTalepleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CariId = table.Column<int>(type: "integer", nullable: false),
                    TalepNo = table.Column<string>(type: "text", nullable: false),
                    Tarih = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Icerik = table.Column<string>(type: "text", nullable: true),
                    Durum = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusteriTalepleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MusteriTalepleri_Cariler_CariId",
                        column: x => x.CariId,
                        principalTable: "Cariler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CariFisleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FisNo = table.Column<string>(type: "text", nullable: false),
                    CariId = table.Column<int>(type: "integer", nullable: false),
                    Tarih = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FisTipi = table.Column<int>(type: "integer", nullable: false),
                    Tutar = table.Column<decimal>(type: "numeric", nullable: false),
                    OdemeYontemi = table.Column<int>(type: "integer", nullable: false),
                    BankaHesabiId = table.Column<int>(type: "integer", nullable: true),
                    KasaHesabiId = table.Column<int>(type: "integer", nullable: true),
                    Aciklama = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CariFisleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CariFisleri_BankaHesaplari_BankaHesabiId",
                        column: x => x.BankaHesabiId,
                        principalTable: "BankaHesaplari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CariFisleri_Cariler_CariId",
                        column: x => x.CariId,
                        principalTable: "Cariler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CariFisleri_KasaHesaplari_KasaHesabiId",
                        column: x => x.KasaHesabiId,
                        principalTable: "KasaHesaplari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Malzemeler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MalzemeKodu = table.Column<string>(type: "text", nullable: false),
                    Barkod = table.Column<string>(type: "text", nullable: true),
                    MalzemeAdi = table.Column<string>(type: "text", nullable: false),
                    Marka = table.Column<string>(type: "text", nullable: true),
                    Kalite = table.Column<string>(type: "text", nullable: true),
                    Tip = table.Column<string>(type: "text", nullable: true),
                    Birim = table.Column<string>(type: "text", nullable: false),
                    TeminTuru = table.Column<int>(type: "integer", nullable: false),
                    StokTipi = table.Column<int>(type: "integer", nullable: false),
                    AlisFiyati = table.Column<decimal>(type: "numeric", nullable: false),
                    SatisFiyati = table.Column<decimal>(type: "numeric", nullable: false),
                    KdvOrani = table.Column<decimal>(type: "numeric", nullable: false),
                    MinStokMiktari = table.Column<decimal>(type: "numeric", nullable: false),
                    MaxStokMiktari = table.Column<decimal>(type: "numeric", nullable: false),
                    RafNo = table.Column<string>(type: "text", nullable: true),
                    Bakiye = table.Column<decimal>(type: "numeric", nullable: false),
                    KategoriId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Malzemeler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Malzemeler_MalzemeKategoriler_KategoriId",
                        column: x => x.KategoriId,
                        principalTable: "MalzemeKategoriler",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AlisSiparisleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CariId = table.Column<int>(type: "integer", nullable: false),
                    SubeId = table.Column<int>(type: "integer", nullable: false),
                    Tarih = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Durum = table.Column<int>(type: "integer", nullable: false),
                    Aciklama = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlisSiparisleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlisSiparisleri_Cariler_CariId",
                        column: x => x.CariId,
                        principalTable: "Cariler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlisSiparisleri_Subeler_SubeId",
                        column: x => x.SubeId,
                        principalTable: "Subeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    AdSoyad = table.Column<string>(type: "text", nullable: false),
                    SubeId = table.Column<int>(type: "integer", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Subeler_SubeId",
                        column: x => x.SubeId,
                        principalTable: "Subeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MalzemeHareketFisleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FisNo = table.Column<string>(type: "text", nullable: false),
                    Tarih = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HareketTipi = table.Column<int>(type: "integer", nullable: false),
                    SubeId = table.Column<int>(type: "integer", nullable: false),
                    Durum = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MalzemeHareketFisleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MalzemeHareketFisleri_Subeler_SubeId",
                        column: x => x.SubeId,
                        principalTable: "Subeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SatisTeklifleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CariId = table.Column<int>(type: "integer", nullable: false),
                    MusteriTalebiId = table.Column<int>(type: "integer", nullable: true),
                    Tarih = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GecerlilikTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Durum = table.Column<int>(type: "integer", nullable: false),
                    Aciklama = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SatisTeklifleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SatisTeklifleri_Cariler_CariId",
                        column: x => x.CariId,
                        principalTable: "Cariler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SatisTeklifleri_MusteriTalepleri_MusteriTalebiId",
                        column: x => x.MusteriTalebiId,
                        principalTable: "MusteriTalepleri",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AlisIrsaliyeleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AlisSiparisiId = table.Column<int>(type: "integer", nullable: true),
                    CariId = table.Column<int>(type: "integer", nullable: false),
                    SubeId = table.Column<int>(type: "integer", nullable: false),
                    Tarih = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Durum = table.Column<int>(type: "integer", nullable: false),
                    Aciklama = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlisIrsaliyeleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlisIrsaliyeleri_AlisSiparisleri_AlisSiparisiId",
                        column: x => x.AlisSiparisiId,
                        principalTable: "AlisSiparisleri",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AlisIrsaliyeleri_Cariler_CariId",
                        column: x => x.CariId,
                        principalTable: "Cariler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlisIrsaliyeleri_Subeler_SubeId",
                        column: x => x.SubeId,
                        principalTable: "Subeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlisSiparisiKalemleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AlisSiparisiId = table.Column<int>(type: "integer", nullable: false),
                    MalzemeId = table.Column<int>(type: "integer", nullable: false),
                    Miktar = table.Column<decimal>(type: "numeric", nullable: false),
                    BirimFiyat = table.Column<decimal>(type: "numeric", nullable: false),
                    KdvOrani = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlisSiparisiKalemleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlisSiparisiKalemleri_AlisSiparisleri_AlisSiparisiId",
                        column: x => x.AlisSiparisiId,
                        principalTable: "AlisSiparisleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlisSiparisiKalemleri_Malzemeler_MalzemeId",
                        column: x => x.MalzemeId,
                        principalTable: "Malzemeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MalzemeHareketFisiKalemleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MalzemeHareketFisiId = table.Column<int>(type: "integer", nullable: false),
                    MalzemeId = table.Column<int>(type: "integer", nullable: false),
                    Miktar = table.Column<decimal>(type: "numeric", nullable: false),
                    Aciklama = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MalzemeHareketFisiKalemleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MalzemeHareketFisiKalemleri_MalzemeHareketFisleri_MalzemeHa~",
                        column: x => x.MalzemeHareketFisiId,
                        principalTable: "MalzemeHareketFisleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MalzemeHareketFisiKalemleri_Malzemeler_MalzemeId",
                        column: x => x.MalzemeId,
                        principalTable: "Malzemeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SatisSiparisleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CariId = table.Column<int>(type: "integer", nullable: false),
                    SatisTeklifiId = table.Column<int>(type: "integer", nullable: true),
                    Tarih = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Durum = table.Column<int>(type: "integer", nullable: false),
                    Aciklama = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SatisSiparisleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SatisSiparisleri_Cariler_CariId",
                        column: x => x.CariId,
                        principalTable: "Cariler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SatisSiparisleri_SatisTeklifleri_SatisTeklifiId",
                        column: x => x.SatisTeklifiId,
                        principalTable: "SatisTeklifleri",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SatisTeklifiKalemleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SatisTeklifiId = table.Column<int>(type: "integer", nullable: false),
                    MalzemeId = table.Column<int>(type: "integer", nullable: false),
                    Miktar = table.Column<decimal>(type: "numeric", nullable: false),
                    BirimFiyat = table.Column<decimal>(type: "numeric", nullable: false),
                    KdvOrani = table.Column<decimal>(type: "numeric", nullable: false),
                    Iskonto = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SatisTeklifiKalemleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SatisTeklifiKalemleri_Malzemeler_MalzemeId",
                        column: x => x.MalzemeId,
                        principalTable: "Malzemeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SatisTeklifiKalemleri_SatisTeklifleri_SatisTeklifiId",
                        column: x => x.SatisTeklifiId,
                        principalTable: "SatisTeklifleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlisFaturalari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CariId = table.Column<int>(type: "integer", nullable: false),
                    AlisSiparisiId = table.Column<int>(type: "integer", nullable: true),
                    AlisIrsaliyesiId = table.Column<int>(type: "integer", nullable: true),
                    Tarih = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Durum = table.Column<int>(type: "integer", nullable: false),
                    Aciklama = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlisFaturalari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlisFaturalari_AlisIrsaliyeleri_AlisIrsaliyesiId",
                        column: x => x.AlisIrsaliyesiId,
                        principalTable: "AlisIrsaliyeleri",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AlisFaturalari_AlisSiparisleri_AlisSiparisiId",
                        column: x => x.AlisSiparisiId,
                        principalTable: "AlisSiparisleri",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AlisFaturalari_Cariler_CariId",
                        column: x => x.CariId,
                        principalTable: "Cariler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlisIrsaliyesiKalemleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AlisIrsaliyesiId = table.Column<int>(type: "integer", nullable: false),
                    MalzemeId = table.Column<int>(type: "integer", nullable: false),
                    Miktar = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlisIrsaliyesiKalemleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlisIrsaliyesiKalemleri_AlisIrsaliyeleri_AlisIrsaliyesiId",
                        column: x => x.AlisIrsaliyesiId,
                        principalTable: "AlisIrsaliyeleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlisIrsaliyesiKalemleri_Malzemeler_MalzemeId",
                        column: x => x.MalzemeId,
                        principalTable: "Malzemeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SatisSiparisiKalemleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SatisSiparisiId = table.Column<int>(type: "integer", nullable: false),
                    MalzemeId = table.Column<int>(type: "integer", nullable: false),
                    Miktar = table.Column<decimal>(type: "numeric", nullable: false),
                    BirimFiyat = table.Column<decimal>(type: "numeric", nullable: false),
                    KdvOrani = table.Column<decimal>(type: "numeric", nullable: false),
                    Iskonto = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SatisSiparisiKalemleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SatisSiparisiKalemleri_Malzemeler_MalzemeId",
                        column: x => x.MalzemeId,
                        principalTable: "Malzemeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SatisSiparisiKalemleri_SatisSiparisleri_SatisSiparisiId",
                        column: x => x.SatisSiparisiId,
                        principalTable: "SatisSiparisleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SevkIrsaliyeleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SatisSiparisiId = table.Column<int>(type: "integer", nullable: false),
                    SubeId = table.Column<int>(type: "integer", nullable: false),
                    Tarih = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SevkAdresi = table.Column<string>(type: "text", nullable: true),
                    AracSofor = table.Column<string>(type: "text", nullable: true),
                    Durum = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SevkIrsaliyeleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SevkIrsaliyeleri_SatisSiparisleri_SatisSiparisiId",
                        column: x => x.SatisSiparisiId,
                        principalTable: "SatisSiparisleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SevkIrsaliyeleri_Subeler_SubeId",
                        column: x => x.SubeId,
                        principalTable: "Subeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlisFaturasiKalemleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AlisFaturasiId = table.Column<int>(type: "integer", nullable: false),
                    MalzemeId = table.Column<int>(type: "integer", nullable: false),
                    Miktar = table.Column<decimal>(type: "numeric", nullable: false),
                    BirimFiyat = table.Column<decimal>(type: "numeric", nullable: false),
                    KdvOrani = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlisFaturasiKalemleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlisFaturasiKalemleri_AlisFaturalari_AlisFaturasiId",
                        column: x => x.AlisFaturasiId,
                        principalTable: "AlisFaturalari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlisFaturasiKalemleri_Malzemeler_MalzemeId",
                        column: x => x.MalzemeId,
                        principalTable: "Malzemeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SatisFaturalari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CariId = table.Column<int>(type: "integer", nullable: false),
                    SevkIrsaliyesiId = table.Column<int>(type: "integer", nullable: true),
                    SatisSiparisiId = table.Column<int>(type: "integer", nullable: true),
                    Tarih = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Durum = table.Column<int>(type: "integer", nullable: false),
                    Aciklama = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SatisFaturalari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SatisFaturalari_Cariler_CariId",
                        column: x => x.CariId,
                        principalTable: "Cariler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SatisFaturalari_SatisSiparisleri_SatisSiparisiId",
                        column: x => x.SatisSiparisiId,
                        principalTable: "SatisSiparisleri",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SatisFaturalari_SevkIrsaliyeleri_SevkIrsaliyesiId",
                        column: x => x.SevkIrsaliyesiId,
                        principalTable: "SevkIrsaliyeleri",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SevkIrsaliyesiKalemleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SevkIrsaliyesiId = table.Column<int>(type: "integer", nullable: false),
                    MalzemeId = table.Column<int>(type: "integer", nullable: false),
                    Miktar = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SevkIrsaliyesiKalemleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SevkIrsaliyesiKalemleri_Malzemeler_MalzemeId",
                        column: x => x.MalzemeId,
                        principalTable: "Malzemeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SevkIrsaliyesiKalemleri_SevkIrsaliyeleri_SevkIrsaliyesiId",
                        column: x => x.SevkIrsaliyesiId,
                        principalTable: "SevkIrsaliyeleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MuhasebeFisleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FisNo = table.Column<string>(type: "text", nullable: false),
                    Tarih = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SatisFaturasiId = table.Column<int>(type: "integer", nullable: true),
                    AlisFaturasiId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuhasebeFisleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MuhasebeFisleri_AlisFaturalari_AlisFaturasiId",
                        column: x => x.AlisFaturasiId,
                        principalTable: "AlisFaturalari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MuhasebeFisleri_SatisFaturalari_SatisFaturasiId",
                        column: x => x.SatisFaturasiId,
                        principalTable: "SatisFaturalari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SatisFaturasiKalemleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SatisFaturasiId = table.Column<int>(type: "integer", nullable: false),
                    MalzemeId = table.Column<int>(type: "integer", nullable: false),
                    Miktar = table.Column<decimal>(type: "numeric", nullable: false),
                    BirimFiyat = table.Column<decimal>(type: "numeric", nullable: false),
                    KdvOrani = table.Column<decimal>(type: "numeric", nullable: false),
                    Iskonto = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SatisFaturasiKalemleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SatisFaturasiKalemleri_Malzemeler_MalzemeId",
                        column: x => x.MalzemeId,
                        principalTable: "Malzemeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SatisFaturasiKalemleri_SatisFaturalari_SatisFaturasiId",
                        column: x => x.SatisFaturasiId,
                        principalTable: "SatisFaturalari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MuhasebeFisiKalemleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MuhasebeFisiId = table.Column<int>(type: "integer", nullable: false),
                    HesapPlaniId = table.Column<int>(type: "integer", nullable: false),
                    Borc = table.Column<decimal>(type: "numeric", nullable: false),
                    Alacak = table.Column<decimal>(type: "numeric", nullable: false),
                    Aciklama = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuhasebeFisiKalemleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MuhasebeFisiKalemleri_HesapPlani_HesapPlaniId",
                        column: x => x.HesapPlaniId,
                        principalTable: "HesapPlani",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MuhasebeFisiKalemleri_MuhasebeFisleri_MuhasebeFisiId",
                        column: x => x.MuhasebeFisiId,
                        principalTable: "MuhasebeFisleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlisFaturalari_AlisIrsaliyesiId",
                table: "AlisFaturalari",
                column: "AlisIrsaliyesiId");

            migrationBuilder.CreateIndex(
                name: "IX_AlisFaturalari_AlisSiparisiId",
                table: "AlisFaturalari",
                column: "AlisSiparisiId");

            migrationBuilder.CreateIndex(
                name: "IX_AlisFaturalari_CariId",
                table: "AlisFaturalari",
                column: "CariId");

            migrationBuilder.CreateIndex(
                name: "IX_AlisFaturasiKalemleri_AlisFaturasiId",
                table: "AlisFaturasiKalemleri",
                column: "AlisFaturasiId");

            migrationBuilder.CreateIndex(
                name: "IX_AlisFaturasiKalemleri_MalzemeId",
                table: "AlisFaturasiKalemleri",
                column: "MalzemeId");

            migrationBuilder.CreateIndex(
                name: "IX_AlisIrsaliyeleri_AlisSiparisiId",
                table: "AlisIrsaliyeleri",
                column: "AlisSiparisiId");

            migrationBuilder.CreateIndex(
                name: "IX_AlisIrsaliyeleri_CariId",
                table: "AlisIrsaliyeleri",
                column: "CariId");

            migrationBuilder.CreateIndex(
                name: "IX_AlisIrsaliyeleri_SubeId",
                table: "AlisIrsaliyeleri",
                column: "SubeId");

            migrationBuilder.CreateIndex(
                name: "IX_AlisIrsaliyesiKalemleri_AlisIrsaliyesiId",
                table: "AlisIrsaliyesiKalemleri",
                column: "AlisIrsaliyesiId");

            migrationBuilder.CreateIndex(
                name: "IX_AlisIrsaliyesiKalemleri_MalzemeId",
                table: "AlisIrsaliyesiKalemleri",
                column: "MalzemeId");

            migrationBuilder.CreateIndex(
                name: "IX_AlisSiparisiKalemleri_AlisSiparisiId",
                table: "AlisSiparisiKalemleri",
                column: "AlisSiparisiId");

            migrationBuilder.CreateIndex(
                name: "IX_AlisSiparisiKalemleri_MalzemeId",
                table: "AlisSiparisiKalemleri",
                column: "MalzemeId");

            migrationBuilder.CreateIndex(
                name: "IX_AlisSiparisleri_CariId",
                table: "AlisSiparisleri",
                column: "CariId");

            migrationBuilder.CreateIndex(
                name: "IX_AlisSiparisleri_SubeId",
                table: "AlisSiparisleri",
                column: "SubeId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_SubeId",
                table: "AspNetUsers",
                column: "SubeId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CariFisleri_BankaHesabiId",
                table: "CariFisleri",
                column: "BankaHesabiId");

            migrationBuilder.CreateIndex(
                name: "IX_CariFisleri_CariId",
                table: "CariFisleri",
                column: "CariId");

            migrationBuilder.CreateIndex(
                name: "IX_CariFisleri_KasaHesabiId",
                table: "CariFisleri",
                column: "KasaHesabiId");

            migrationBuilder.CreateIndex(
                name: "IX_Cariler_CariKodu",
                table: "Cariler",
                column: "CariKodu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CekSenetler_CariId",
                table: "CekSenetler",
                column: "CariId");

            migrationBuilder.CreateIndex(
                name: "IX_HesapPlani_HesapKodu",
                table: "HesapPlani",
                column: "HesapKodu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HesapPlani_ParentId",
                table: "HesapPlani",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_MalzemeHareketFisiKalemleri_MalzemeHareketFisiId",
                table: "MalzemeHareketFisiKalemleri",
                column: "MalzemeHareketFisiId");

            migrationBuilder.CreateIndex(
                name: "IX_MalzemeHareketFisiKalemleri_MalzemeId",
                table: "MalzemeHareketFisiKalemleri",
                column: "MalzemeId");

            migrationBuilder.CreateIndex(
                name: "IX_MalzemeHareketFisleri_SubeId",
                table: "MalzemeHareketFisleri",
                column: "SubeId");

            migrationBuilder.CreateIndex(
                name: "IX_MalzemeKategoriler_ParentId",
                table: "MalzemeKategoriler",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Malzemeler_KategoriId",
                table: "Malzemeler",
                column: "KategoriId");

            migrationBuilder.CreateIndex(
                name: "IX_Malzemeler_MalzemeKodu",
                table: "Malzemeler",
                column: "MalzemeKodu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MuhasebeFisiKalemleri_HesapPlaniId",
                table: "MuhasebeFisiKalemleri",
                column: "HesapPlaniId");

            migrationBuilder.CreateIndex(
                name: "IX_MuhasebeFisiKalemleri_MuhasebeFisiId",
                table: "MuhasebeFisiKalemleri",
                column: "MuhasebeFisiId");

            migrationBuilder.CreateIndex(
                name: "IX_MuhasebeFisleri_AlisFaturasiId",
                table: "MuhasebeFisleri",
                column: "AlisFaturasiId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MuhasebeFisleri_SatisFaturasiId",
                table: "MuhasebeFisleri",
                column: "SatisFaturasiId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MusteriTalepleri_CariId",
                table: "MusteriTalepleri",
                column: "CariId");

            migrationBuilder.CreateIndex(
                name: "IX_SatisFaturalari_CariId",
                table: "SatisFaturalari",
                column: "CariId");

            migrationBuilder.CreateIndex(
                name: "IX_SatisFaturalari_SatisSiparisiId",
                table: "SatisFaturalari",
                column: "SatisSiparisiId");

            migrationBuilder.CreateIndex(
                name: "IX_SatisFaturalari_SevkIrsaliyesiId",
                table: "SatisFaturalari",
                column: "SevkIrsaliyesiId");

            migrationBuilder.CreateIndex(
                name: "IX_SatisFaturasiKalemleri_MalzemeId",
                table: "SatisFaturasiKalemleri",
                column: "MalzemeId");

            migrationBuilder.CreateIndex(
                name: "IX_SatisFaturasiKalemleri_SatisFaturasiId",
                table: "SatisFaturasiKalemleri",
                column: "SatisFaturasiId");

            migrationBuilder.CreateIndex(
                name: "IX_SatisSiparisiKalemleri_MalzemeId",
                table: "SatisSiparisiKalemleri",
                column: "MalzemeId");

            migrationBuilder.CreateIndex(
                name: "IX_SatisSiparisiKalemleri_SatisSiparisiId",
                table: "SatisSiparisiKalemleri",
                column: "SatisSiparisiId");

            migrationBuilder.CreateIndex(
                name: "IX_SatisSiparisleri_CariId",
                table: "SatisSiparisleri",
                column: "CariId");

            migrationBuilder.CreateIndex(
                name: "IX_SatisSiparisleri_SatisTeklifiId",
                table: "SatisSiparisleri",
                column: "SatisTeklifiId");

            migrationBuilder.CreateIndex(
                name: "IX_SatisTeklifiKalemleri_MalzemeId",
                table: "SatisTeklifiKalemleri",
                column: "MalzemeId");

            migrationBuilder.CreateIndex(
                name: "IX_SatisTeklifiKalemleri_SatisTeklifiId",
                table: "SatisTeklifiKalemleri",
                column: "SatisTeklifiId");

            migrationBuilder.CreateIndex(
                name: "IX_SatisTeklifleri_CariId",
                table: "SatisTeklifleri",
                column: "CariId");

            migrationBuilder.CreateIndex(
                name: "IX_SatisTeklifleri_MusteriTalebiId",
                table: "SatisTeklifleri",
                column: "MusteriTalebiId");

            migrationBuilder.CreateIndex(
                name: "IX_SevkIrsaliyeleri_SatisSiparisiId",
                table: "SevkIrsaliyeleri",
                column: "SatisSiparisiId");

            migrationBuilder.CreateIndex(
                name: "IX_SevkIrsaliyeleri_SubeId",
                table: "SevkIrsaliyeleri",
                column: "SubeId");

            migrationBuilder.CreateIndex(
                name: "IX_SevkIrsaliyesiKalemleri_MalzemeId",
                table: "SevkIrsaliyesiKalemleri",
                column: "MalzemeId");

            migrationBuilder.CreateIndex(
                name: "IX_SevkIrsaliyesiKalemleri_SevkIrsaliyesiId",
                table: "SevkIrsaliyesiKalemleri",
                column: "SevkIrsaliyesiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlisFaturasiKalemleri");

            migrationBuilder.DropTable(
                name: "AlisIrsaliyesiKalemleri");

            migrationBuilder.DropTable(
                name: "AlisSiparisiKalemleri");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CariFisleri");

            migrationBuilder.DropTable(
                name: "CekSenetler");

            migrationBuilder.DropTable(
                name: "MalzemeHareketFisiKalemleri");

            migrationBuilder.DropTable(
                name: "MuhasebeFisiKalemleri");

            migrationBuilder.DropTable(
                name: "SatisFaturasiKalemleri");

            migrationBuilder.DropTable(
                name: "SatisSiparisiKalemleri");

            migrationBuilder.DropTable(
                name: "SatisTeklifiKalemleri");

            migrationBuilder.DropTable(
                name: "SevkIrsaliyesiKalemleri");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "BankaHesaplari");

            migrationBuilder.DropTable(
                name: "KasaHesaplari");

            migrationBuilder.DropTable(
                name: "MalzemeHareketFisleri");

            migrationBuilder.DropTable(
                name: "HesapPlani");

            migrationBuilder.DropTable(
                name: "MuhasebeFisleri");

            migrationBuilder.DropTable(
                name: "Malzemeler");

            migrationBuilder.DropTable(
                name: "AlisFaturalari");

            migrationBuilder.DropTable(
                name: "SatisFaturalari");

            migrationBuilder.DropTable(
                name: "MalzemeKategoriler");

            migrationBuilder.DropTable(
                name: "AlisIrsaliyeleri");

            migrationBuilder.DropTable(
                name: "SevkIrsaliyeleri");

            migrationBuilder.DropTable(
                name: "AlisSiparisleri");

            migrationBuilder.DropTable(
                name: "SatisSiparisleri");

            migrationBuilder.DropTable(
                name: "Subeler");

            migrationBuilder.DropTable(
                name: "SatisTeklifleri");

            migrationBuilder.DropTable(
                name: "MusteriTalepleri");

            migrationBuilder.DropTable(
                name: "Cariler");
        }
    }
}
