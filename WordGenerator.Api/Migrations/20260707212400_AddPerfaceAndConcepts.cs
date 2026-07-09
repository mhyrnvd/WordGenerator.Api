using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordGenerator.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPerfaceAndConcepts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ConceptsSectionId",
                table: "ContentElements",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PrefaceSectionId",
                table: "ContentElements",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ConceptsSections",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, defaultValue: "مفاهیم"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DocumentTemplateId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConceptsSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConceptsSections_DocumentTemplates_DocumentTemplateId",
                        column: x => x.DocumentTemplateId,
                        principalTable: "DocumentTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrefaceSections",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, defaultValue: "پیش‌گفتار"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DocumentTemplateId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrefaceSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrefaceSections_DocumentTemplates_DocumentTemplateId",
                        column: x => x.DocumentTemplateId,
                        principalTable: "DocumentTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentElements_ConceptsSectionId",
                table: "ContentElements",
                column: "ConceptsSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentElements_PrefaceSectionId",
                table: "ContentElements",
                column: "PrefaceSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ConceptsSections_DocumentTemplateId",
                table: "ConceptsSections",
                column: "DocumentTemplateId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrefaceSections_DocumentTemplateId",
                table: "PrefaceSections",
                column: "DocumentTemplateId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ContentElements_ConceptsSections_ConceptsSectionId",
                table: "ContentElements",
                column: "ConceptsSectionId",
                principalTable: "ConceptsSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ContentElements_PrefaceSections_PrefaceSectionId",
                table: "ContentElements",
                column: "PrefaceSectionId",
                principalTable: "PrefaceSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContentElements_ConceptsSections_ConceptsSectionId",
                table: "ContentElements");

            migrationBuilder.DropForeignKey(
                name: "FK_ContentElements_PrefaceSections_PrefaceSectionId",
                table: "ContentElements");

            migrationBuilder.DropTable(
                name: "ConceptsSections");

            migrationBuilder.DropTable(
                name: "PrefaceSections");

            migrationBuilder.DropIndex(
                name: "IX_ContentElements_ConceptsSectionId",
                table: "ContentElements");

            migrationBuilder.DropIndex(
                name: "IX_ContentElements_PrefaceSectionId",
                table: "ContentElements");

            migrationBuilder.DropColumn(
                name: "ConceptsSectionId",
                table: "ContentElements");

            migrationBuilder.DropColumn(
                name: "PrefaceSectionId",
                table: "ContentElements");
        }
    }
}
