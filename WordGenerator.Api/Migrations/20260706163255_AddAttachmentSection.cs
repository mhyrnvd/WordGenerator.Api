using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordGenerator.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAttachmentSection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AttachmentSectionId",
                table: "ContentElements",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DocumentSectionId",
                table: "ContentElements",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ReferenceSectionId",
                table: "ContentElements",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AttachmentSections",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, defaultValue: "الف) پیوست‌ها"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Order = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    DocumentTemplateId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttachmentSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttachmentSections_DocumentTemplates_DocumentTemplateId",
                        column: x => x.DocumentTemplateId,
                        principalTable: "DocumentTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentSections",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, defaultValue: "پ) مدارک"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Order = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    DocumentTemplateId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentSections_DocumentTemplates_DocumentTemplateId",
                        column: x => x.DocumentTemplateId,
                        principalTable: "DocumentTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReferenceSections",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, defaultValue: "ب) References"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Order = table.Column<int>(type: "int", nullable: false, defaultValue: 2),
                    DocumentTemplateId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferenceSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReferenceSections_DocumentTemplates_DocumentTemplateId",
                        column: x => x.DocumentTemplateId,
                        principalTable: "DocumentTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentElements_AttachmentSectionId",
                table: "ContentElements",
                column: "AttachmentSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentElements_DocumentSectionId",
                table: "ContentElements",
                column: "DocumentSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentElements_ReferenceSectionId",
                table: "ContentElements",
                column: "ReferenceSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_AttachmentSections_DocumentTemplateId",
                table: "AttachmentSections",
                column: "DocumentTemplateId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSections_DocumentTemplateId",
                table: "DocumentSections",
                column: "DocumentTemplateId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReferenceSections_DocumentTemplateId",
                table: "ReferenceSections",
                column: "DocumentTemplateId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ContentElements_AttachmentSections_AttachmentSectionId",
                table: "ContentElements",
                column: "AttachmentSectionId",
                principalTable: "AttachmentSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ContentElements_DocumentSections_DocumentSectionId",
                table: "ContentElements",
                column: "DocumentSectionId",
                principalTable: "DocumentSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ContentElements_ReferenceSections_ReferenceSectionId",
                table: "ContentElements",
                column: "ReferenceSectionId",
                principalTable: "ReferenceSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContentElements_AttachmentSections_AttachmentSectionId",
                table: "ContentElements");

            migrationBuilder.DropForeignKey(
                name: "FK_ContentElements_DocumentSections_DocumentSectionId",
                table: "ContentElements");

            migrationBuilder.DropForeignKey(
                name: "FK_ContentElements_ReferenceSections_ReferenceSectionId",
                table: "ContentElements");

            migrationBuilder.DropTable(
                name: "AttachmentSections");

            migrationBuilder.DropTable(
                name: "DocumentSections");

            migrationBuilder.DropTable(
                name: "ReferenceSections");

            migrationBuilder.DropIndex(
                name: "IX_ContentElements_AttachmentSectionId",
                table: "ContentElements");

            migrationBuilder.DropIndex(
                name: "IX_ContentElements_DocumentSectionId",
                table: "ContentElements");

            migrationBuilder.DropIndex(
                name: "IX_ContentElements_ReferenceSectionId",
                table: "ContentElements");

            migrationBuilder.DropColumn(
                name: "AttachmentSectionId",
                table: "ContentElements");

            migrationBuilder.DropColumn(
                name: "DocumentSectionId",
                table: "ContentElements");

            migrationBuilder.DropColumn(
                name: "ReferenceSectionId",
                table: "ContentElements");
        }
    }
}
