using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordGenerator.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddImageModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ImageData = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    Caption = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Width = table.Column<double>(type: "float", nullable: false, defaultValue: 500.0),
                    Height = table.Column<double>(type: "float", nullable: false, defaultValue: 0.0),
                    TemplateSectionId = table.Column<long>(type: "bigint", nullable: true),
                    MasterSectionId = table.Column<long>(type: "bigint", nullable: true),
                    SubSectionId = table.Column<long>(type: "bigint", nullable: true),
                    CoverPageId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Images_CoverPageTemplates_CoverPageId",
                        column: x => x.CoverPageId,
                        principalTable: "CoverPageTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Images_MasterSections_MasterSectionId",
                        column: x => x.MasterSectionId,
                        principalTable: "MasterSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Images_SubSections_SubSectionId",
                        column: x => x.SubSectionId,
                        principalTable: "SubSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Images_TemplateSections_TemplateSectionId",
                        column: x => x.TemplateSectionId,
                        principalTable: "TemplateSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Images_CoverPageId",
                table: "Images",
                column: "CoverPageId");

            migrationBuilder.CreateIndex(
                name: "IX_Images_MasterSectionId",
                table: "Images",
                column: "MasterSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Images_SubSectionId",
                table: "Images",
                column: "SubSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Images_TemplateSectionId",
                table: "Images",
                column: "TemplateSectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Images");
        }
    }
}
