using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordGenerator.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddHierarchicalSections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoverPageTemplates_DocumentTemplates_DocumentTemplateId",
                table: "CoverPageTemplates");

            migrationBuilder.AddColumn<long>(
                name: "MasterSectionId",
                table: "DynamicTables",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SubSectionId",
                table: "DynamicTables",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MasterSections",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    ShowInToc = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DocumentTemplateId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MasterSections_DocumentTemplates_DocumentTemplateId",
                        column: x => x.DocumentTemplateId,
                        principalTable: "DocumentTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MasterSectionParagraphs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Text = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    MasterSectionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterSectionParagraphs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MasterSectionParagraphs_MasterSections_MasterSectionId",
                        column: x => x.MasterSectionId,
                        principalTable: "MasterSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubSections",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    ShowInToc = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    MasterSectionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubSections_MasterSections_MasterSectionId",
                        column: x => x.MasterSectionId,
                        principalTable: "MasterSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubSectionParagraphs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Text = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    SubSectionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubSectionParagraphs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubSectionParagraphs_SubSections_SubSectionId",
                        column: x => x.SubSectionId,
                        principalTable: "SubSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DynamicTables_MasterSectionId",
                table: "DynamicTables",
                column: "MasterSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicTables_SubSectionId",
                table: "DynamicTables",
                column: "SubSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_MasterSectionParagraphs_MasterSectionId",
                table: "MasterSectionParagraphs",
                column: "MasterSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_MasterSections_DocumentTemplateId",
                table: "MasterSections",
                column: "DocumentTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_SubSectionParagraphs_SubSectionId",
                table: "SubSectionParagraphs",
                column: "SubSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubSections_MasterSectionId",
                table: "SubSections",
                column: "MasterSectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_CoverPageTemplates_DocumentTemplates_DocumentTemplateId",
                table: "CoverPageTemplates",
                column: "DocumentTemplateId",
                principalTable: "DocumentTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicTables_MasterSections_MasterSectionId",
                table: "DynamicTables",
                column: "MasterSectionId",
                principalTable: "MasterSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicTables_SubSections_SubSectionId",
                table: "DynamicTables",
                column: "SubSectionId",
                principalTable: "SubSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoverPageTemplates_DocumentTemplates_DocumentTemplateId",
                table: "CoverPageTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicTables_MasterSections_MasterSectionId",
                table: "DynamicTables");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicTables_SubSections_SubSectionId",
                table: "DynamicTables");

            migrationBuilder.DropTable(
                name: "MasterSectionParagraphs");

            migrationBuilder.DropTable(
                name: "SubSectionParagraphs");

            migrationBuilder.DropTable(
                name: "SubSections");

            migrationBuilder.DropTable(
                name: "MasterSections");

            migrationBuilder.DropIndex(
                name: "IX_DynamicTables_MasterSectionId",
                table: "DynamicTables");

            migrationBuilder.DropIndex(
                name: "IX_DynamicTables_SubSectionId",
                table: "DynamicTables");

            migrationBuilder.DropColumn(
                name: "MasterSectionId",
                table: "DynamicTables");

            migrationBuilder.DropColumn(
                name: "SubSectionId",
                table: "DynamicTables");

            migrationBuilder.AddForeignKey(
                name: "FK_CoverPageTemplates_DocumentTemplates_DocumentTemplateId",
                table: "CoverPageTemplates",
                column: "DocumentTemplateId",
                principalTable: "DocumentTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
