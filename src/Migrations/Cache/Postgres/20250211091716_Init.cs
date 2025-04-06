using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mix.Heart.Migrations.Cache.Postgres
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MixCache",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    keyword = table.Column<string>(type: "varchar(400)", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false),
                    expired_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    created_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    status = table.Column<string>(type: "varchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MixCache", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "index_expires_at_time",
                table: "MixCache",
                column: "expired_date_time");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MixCache");
        }
    }
}
