using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordGenerator.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPageHeader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PageHeaders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HeaderText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DocumentTemplateId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageHeaders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PageHeaders_DocumentTemplates_DocumentTemplateId",
                        column: x => x.DocumentTemplateId,
                        principalTable: "DocumentTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HeaderLogos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ImageData = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Width = table.Column<double>(type: "float", nullable: false, defaultValue: 60.0),
                    Height = table.Column<double>(type: "float", nullable: false, defaultValue: 40.0),
                    PageHeaderId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeaderLogos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HeaderLogos_PageHeaders_PageHeaderId",
                        column: x => x.PageHeaderId,
                        principalTable: "PageHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HeaderLogos_PageHeaderId_Order",
                table: "HeaderLogos",
                columns: new[] { "PageHeaderId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_PageHeaders_DocumentTemplateId",
                table: "PageHeaders",
                column: "DocumentTemplateId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HeaderLogos");

            migrationBuilder.DropTable(
                name: "PageHeaders");
        }
    }
}
