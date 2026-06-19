using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordGenerator.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddImageGroupsModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ImageGroupId",
                table: "Images",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ImageGroups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    ImagesPerRow = table.Column<int>(type: "int", nullable: false, defaultValue: 2),
                    MasterSectionId = table.Column<long>(type: "bigint", nullable: true),
                    SubSectionId = table.Column<long>(type: "bigint", nullable: true),
                    TemplateSectionId = table.Column<long>(type: "bigint", nullable: true),
                    CoverPageId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageGroups_CoverPageTemplates_CoverPageId",
                        column: x => x.CoverPageId,
                        principalTable: "CoverPageTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ImageGroups_MasterSections_MasterSectionId",
                        column: x => x.MasterSectionId,
                        principalTable: "MasterSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ImageGroups_SubSections_SubSectionId",
                        column: x => x.SubSectionId,
                        principalTable: "SubSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ImageGroups_TemplateSections_TemplateSectionId",
                        column: x => x.TemplateSectionId,
                        principalTable: "TemplateSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Images_ImageGroupId",
                table: "Images",
                column: "ImageGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageGroups_CoverPageId",
                table: "ImageGroups",
                column: "CoverPageId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageGroups_MasterSectionId",
                table: "ImageGroups",
                column: "MasterSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageGroups_Order",
                table: "ImageGroups",
                column: "Order");

            migrationBuilder.CreateIndex(
                name: "IX_ImageGroups_SubSectionId",
                table: "ImageGroups",
                column: "SubSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageGroups_TemplateSectionId",
                table: "ImageGroups",
                column: "TemplateSectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_ImageGroups_ImageGroupId",
                table: "Images",
                column: "ImageGroupId",
                principalTable: "ImageGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_ImageGroups_ImageGroupId",
                table: "Images");

            migrationBuilder.DropTable(
                name: "ImageGroups");

            migrationBuilder.DropIndex(
                name: "IX_Images_ImageGroupId",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "ImageGroupId",
                table: "Images");
        }
    }
}
