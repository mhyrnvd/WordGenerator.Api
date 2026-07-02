using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordGenerator.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBulletItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "BulletListId",
                table: "ContentElements",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BulletLists",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    MasterSectionId = table.Column<long>(type: "bigint", nullable: true),
                    SubSectionId = table.Column<long>(type: "bigint", nullable: true),
                    TemplateSectionId = table.Column<long>(type: "bigint", nullable: true),
                    CoverPageId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BulletLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BulletLists_CoverPageTemplates_CoverPageId",
                        column: x => x.CoverPageId,
                        principalTable: "CoverPageTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BulletLists_MasterSections_MasterSectionId",
                        column: x => x.MasterSectionId,
                        principalTable: "MasterSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BulletLists_SubSections_SubSectionId",
                        column: x => x.SubSectionId,
                        principalTable: "SubSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BulletLists_TemplateSections_TemplateSectionId",
                        column: x => x.TemplateSectionId,
                        principalTable: "TemplateSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BulletListItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Text = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Level = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    BulletListId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BulletListItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BulletListItems_BulletLists_BulletListId",
                        column: x => x.BulletListId,
                        principalTable: "BulletLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentElements_BulletListId",
                table: "ContentElements",
                column: "BulletListId");

            migrationBuilder.CreateIndex(
                name: "IX_BulletListItems_BulletListId_Order",
                table: "BulletListItems",
                columns: new[] { "BulletListId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_BulletListItems_Level",
                table: "BulletListItems",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_BulletLists_CoverPageId_Order",
                table: "BulletLists",
                columns: new[] { "CoverPageId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_BulletLists_MasterSectionId_Order",
                table: "BulletLists",
                columns: new[] { "MasterSectionId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_BulletLists_SubSectionId_Order",
                table: "BulletLists",
                columns: new[] { "SubSectionId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_BulletLists_TemplateSectionId_Order",
                table: "BulletLists",
                columns: new[] { "TemplateSectionId", "Order" });

            migrationBuilder.AddForeignKey(
                name: "FK_ContentElements_BulletLists_BulletListId",
                table: "ContentElements",
                column: "BulletListId",
                principalTable: "BulletLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContentElements_BulletLists_BulletListId",
                table: "ContentElements");

            migrationBuilder.DropTable(
                name: "BulletListItems");

            migrationBuilder.DropTable(
                name: "BulletLists");

            migrationBuilder.DropIndex(
                name: "IX_ContentElements_BulletListId",
                table: "ContentElements");

            migrationBuilder.DropColumn(
                name: "BulletListId",
                table: "ContentElements");
        }
    }
}
