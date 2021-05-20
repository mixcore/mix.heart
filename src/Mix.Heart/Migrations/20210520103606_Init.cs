using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Mix.Common.Helper;
using Mix.Heart.Constants;
using Mix.Heart.Enums;

namespace Mix.Heart.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var dbProvider = CommonHelper.GetWebEnumConfig<MixDatabaseProvider>(WebConfiguration.MixCacheDbProvider);
            string valueType = dbProvider == MixDatabaseProvider.MSSQL ? "ntext" : "text";
            migrationBuilder.CreateTable(
                name: "mix_cache",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(150)", nullable: false),
                    Value = table.Column<string>(type: valueType, nullable: false),
                    ExpiredDateTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    ModifiedBy = table.Column<string>(type: "varchar(50)", nullable: true),
                    CreatedBy = table.Column<string>(type: "varchar(50)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mix_cache", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "Index_ExpiresAtTime",
                table: "mix_cache",
                column: "ExpiredDateTime");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mix_cache");
        }
    }
}
