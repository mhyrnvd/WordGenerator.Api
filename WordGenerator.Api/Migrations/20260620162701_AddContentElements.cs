using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordGenerator.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddContentElements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoverPageTemplates_DocumentTemplates_DocumentTemplateId",
                table: "CoverPageTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicTables_CoverPageTemplates_CoverPageTemplateId",
                table: "DynamicTables");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicTables_MasterSections_MasterSectionId",
                table: "DynamicTables");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicTables_SubSections_SubSectionId",
                table: "DynamicTables");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicTables_TemplateSections_TemplateSectionId",
                table: "DynamicTables");

            migrationBuilder.DropForeignKey(
                name: "FK_ImageGroups_CoverPageTemplates_CoverPageId",
                table: "ImageGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_ImageGroups_MasterSections_MasterSectionId",
                table: "ImageGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_ImageGroups_SubSections_SubSectionId",
                table: "ImageGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_ImageGroups_TemplateSections_TemplateSectionId",
                table: "ImageGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_ImageGroups_ImageGroupId",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_MasterSections_DocumentTemplates_DocumentTemplateId",
                table: "MasterSections");

            migrationBuilder.DropTable(
                name: "MasterSectionParagraphs");

            migrationBuilder.DropTable(
                name: "SectionParagraphs");

            migrationBuilder.DropTable(
                name: "SubSectionParagraphs");

            migrationBuilder.DropIndex(
                name: "IX_TemplateSections_TemplateId",
                table: "TemplateSections");

            migrationBuilder.DropIndex(
                name: "IX_TableRows_DynamicTableId",
                table: "TableRows");

            migrationBuilder.DropIndex(
                name: "IX_TableColumns_DynamicTableId",
                table: "TableColumns");

            migrationBuilder.DropIndex(
                name: "IX_SubSections_MasterSectionId",
                table: "SubSections");

            migrationBuilder.DropIndex(
                name: "IX_MasterSections_DocumentTemplateId",
                table: "MasterSections");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ImageGroups",
                table: "ImageGroups");

            migrationBuilder.DropIndex(
                name: "IX_ImageGroups_Order",
                table: "ImageGroups");

            migrationBuilder.RenameTable(
                name: "ImageGroups",
                newName: "ImageGroup");

            migrationBuilder.RenameColumn(
                name: "DocumentTemplateId",
                table: "MasterSections",
                newName: "TemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_ImageGroups_TemplateSectionId",
                table: "ImageGroup",
                newName: "IX_ImageGroup_TemplateSectionId");

            migrationBuilder.RenameIndex(
                name: "IX_ImageGroups_SubSectionId",
                table: "ImageGroup",
                newName: "IX_ImageGroup_SubSectionId");

            migrationBuilder.RenameIndex(
                name: "IX_ImageGroups_MasterSectionId",
                table: "ImageGroup",
                newName: "IX_ImageGroup_MasterSectionId");

            migrationBuilder.RenameIndex(
                name: "IX_ImageGroups_CoverPageId",
                table: "ImageGroup",
                newName: "IX_ImageGroup_CoverPageId");

            migrationBuilder.AlterColumn<int>(
                name: "Order",
                table: "Images",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "Images",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Caption",
                table: "Images",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "DocumentTemplates",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "CoverPageTemplates",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "ImageGroup",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Order",
                table: "ImageGroup",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "ImagesPerRow",
                table: "ImageGroup",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 2);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ImageGroup",
                table: "ImageGroup",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ContentElements",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    ParagraphText = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ImageId = table.Column<long>(type: "bigint", nullable: true),
                    TableId = table.Column<long>(type: "bigint", nullable: true),
                    MasterSectionId = table.Column<long>(type: "bigint", nullable: true),
                    SubSectionId = table.Column<long>(type: "bigint", nullable: true),
                    TemplateSectionId = table.Column<long>(type: "bigint", nullable: true),
                    CoverPageId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentElements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentElements_CoverPageTemplates_CoverPageId",
                        column: x => x.CoverPageId,
                        principalTable: "CoverPageTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContentElements_DynamicTables_TableId",
                        column: x => x.TableId,
                        principalTable: "DynamicTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContentElements_Images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContentElements_MasterSections_MasterSectionId",
                        column: x => x.MasterSectionId,
                        principalTable: "MasterSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContentElements_SubSections_SubSectionId",
                        column: x => x.SubSectionId,
                        principalTable: "SubSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContentElements_TemplateSections_TemplateSectionId",
                        column: x => x.TemplateSectionId,
                        principalTable: "TemplateSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TemplateSections_Order",
                table: "TemplateSections",
                column: "Order");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateSections_TemplateId_Order",
                table: "TemplateSections",
                columns: new[] { "TemplateId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_TableRows_DynamicTableId_RowNumber",
                table: "TableRows",
                columns: new[] { "DynamicTableId", "RowNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_TableColumns_DynamicTableId_Order",
                table: "TableColumns",
                columns: new[] { "DynamicTableId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_TableColumns_Order",
                table: "TableColumns",
                column: "Order");

            migrationBuilder.CreateIndex(
                name: "IX_SubSections_MasterSectionId_Order",
                table: "SubSections",
                columns: new[] { "MasterSectionId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_SubSections_Order",
                table: "SubSections",
                column: "Order");

            migrationBuilder.CreateIndex(
                name: "IX_MasterSections_Order",
                table: "MasterSections",
                column: "Order");

            migrationBuilder.CreateIndex(
                name: "IX_MasterSections_TemplateId_Order",
                table: "MasterSections",
                columns: new[] { "TemplateId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_Images_Order",
                table: "Images",
                column: "Order");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicTables_Order",
                table: "DynamicTables",
                column: "Order");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTemplates_Name",
                table: "DocumentTemplates",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ContentElements_CoverPageId",
                table: "ContentElements",
                column: "CoverPageId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentElements_CoverPageId_Order",
                table: "ContentElements",
                columns: new[] { "CoverPageId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_ContentElements_ImageId",
                table: "ContentElements",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentElements_MasterSectionId",
                table: "ContentElements",
                column: "MasterSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentElements_MasterSectionId_Order",
                table: "ContentElements",
                columns: new[] { "MasterSectionId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_ContentElements_Order",
                table: "ContentElements",
                column: "Order");

            migrationBuilder.CreateIndex(
                name: "IX_ContentElements_SubSectionId",
                table: "ContentElements",
                column: "SubSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentElements_SubSectionId_Order",
                table: "ContentElements",
                columns: new[] { "SubSectionId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_ContentElements_TableId",
                table: "ContentElements",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentElements_TemplateSectionId",
                table: "ContentElements",
                column: "TemplateSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentElements_TemplateSectionId_Order",
                table: "ContentElements",
                columns: new[] { "TemplateSectionId", "Order" });

            migrationBuilder.AddForeignKey(
                name: "FK_CoverPageTemplates_DocumentTemplates_DocumentTemplateId",
                table: "CoverPageTemplates",
                column: "DocumentTemplateId",
                principalTable: "DocumentTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicTables_CoverPageTemplates_CoverPageTemplateId",
                table: "DynamicTables",
                column: "CoverPageTemplateId",
                principalTable: "CoverPageTemplates",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicTables_MasterSections_MasterSectionId",
                table: "DynamicTables",
                column: "MasterSectionId",
                principalTable: "MasterSections",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicTables_SubSections_SubSectionId",
                table: "DynamicTables",
                column: "SubSectionId",
                principalTable: "SubSections",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicTables_TemplateSections_TemplateSectionId",
                table: "DynamicTables",
                column: "TemplateSectionId",
                principalTable: "TemplateSections",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ImageGroup_CoverPageTemplates_CoverPageId",
                table: "ImageGroup",
                column: "CoverPageId",
                principalTable: "CoverPageTemplates",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ImageGroup_MasterSections_MasterSectionId",
                table: "ImageGroup",
                column: "MasterSectionId",
                principalTable: "MasterSections",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ImageGroup_SubSections_SubSectionId",
                table: "ImageGroup",
                column: "SubSectionId",
                principalTable: "SubSections",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ImageGroup_TemplateSections_TemplateSectionId",
                table: "ImageGroup",
                column: "TemplateSectionId",
                principalTable: "TemplateSections",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_ImageGroup_ImageGroupId",
                table: "Images",
                column: "ImageGroupId",
                principalTable: "ImageGroup",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MasterSections_DocumentTemplates_TemplateId",
                table: "MasterSections",
                column: "TemplateId",
                principalTable: "DocumentTemplates",
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
                name: "FK_DynamicTables_CoverPageTemplates_CoverPageTemplateId",
                table: "DynamicTables");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicTables_MasterSections_MasterSectionId",
                table: "DynamicTables");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicTables_SubSections_SubSectionId",
                table: "DynamicTables");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicTables_TemplateSections_TemplateSectionId",
                table: "DynamicTables");

            migrationBuilder.DropForeignKey(
                name: "FK_ImageGroup_CoverPageTemplates_CoverPageId",
                table: "ImageGroup");

            migrationBuilder.DropForeignKey(
                name: "FK_ImageGroup_MasterSections_MasterSectionId",
                table: "ImageGroup");

            migrationBuilder.DropForeignKey(
                name: "FK_ImageGroup_SubSections_SubSectionId",
                table: "ImageGroup");

            migrationBuilder.DropForeignKey(
                name: "FK_ImageGroup_TemplateSections_TemplateSectionId",
                table: "ImageGroup");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_ImageGroup_ImageGroupId",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_MasterSections_DocumentTemplates_TemplateId",
                table: "MasterSections");

            migrationBuilder.DropTable(
                name: "ContentElements");

            migrationBuilder.DropIndex(
                name: "IX_TemplateSections_Order",
                table: "TemplateSections");

            migrationBuilder.DropIndex(
                name: "IX_TemplateSections_TemplateId_Order",
                table: "TemplateSections");

            migrationBuilder.DropIndex(
                name: "IX_TableRows_DynamicTableId_RowNumber",
                table: "TableRows");

            migrationBuilder.DropIndex(
                name: "IX_TableColumns_DynamicTableId_Order",
                table: "TableColumns");

            migrationBuilder.DropIndex(
                name: "IX_TableColumns_Order",
                table: "TableColumns");

            migrationBuilder.DropIndex(
                name: "IX_SubSections_MasterSectionId_Order",
                table: "SubSections");

            migrationBuilder.DropIndex(
                name: "IX_SubSections_Order",
                table: "SubSections");

            migrationBuilder.DropIndex(
                name: "IX_MasterSections_Order",
                table: "MasterSections");

            migrationBuilder.DropIndex(
                name: "IX_MasterSections_TemplateId_Order",
                table: "MasterSections");

            migrationBuilder.DropIndex(
                name: "IX_Images_Order",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_DynamicTables_Order",
                table: "DynamicTables");

            migrationBuilder.DropIndex(
                name: "IX_DocumentTemplates_Name",
                table: "DocumentTemplates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ImageGroup",
                table: "ImageGroup");

            migrationBuilder.RenameTable(
                name: "ImageGroup",
                newName: "ImageGroups");

            migrationBuilder.RenameColumn(
                name: "TemplateId",
                table: "MasterSections",
                newName: "DocumentTemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_ImageGroup_TemplateSectionId",
                table: "ImageGroups",
                newName: "IX_ImageGroups_TemplateSectionId");

            migrationBuilder.RenameIndex(
                name: "IX_ImageGroup_SubSectionId",
                table: "ImageGroups",
                newName: "IX_ImageGroups_SubSectionId");

            migrationBuilder.RenameIndex(
                name: "IX_ImageGroup_MasterSectionId",
                table: "ImageGroups",
                newName: "IX_ImageGroups_MasterSectionId");

            migrationBuilder.RenameIndex(
                name: "IX_ImageGroup_CoverPageId",
                table: "ImageGroups",
                newName: "IX_ImageGroups_CoverPageId");

            migrationBuilder.AlterColumn<int>(
                name: "Order",
                table: "Images",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "Images",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Caption",
                table: "Images",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "DocumentTemplates",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "CoverPageTemplates",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "ImageGroups",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Order",
                table: "ImageGroups",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "ImagesPerRow",
                table: "ImageGroups",
                type: "int",
                nullable: false,
                defaultValue: 2,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ImageGroups",
                table: "ImageGroups",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "MasterSectionParagraphs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MasterSectionId = table.Column<long>(type: "bigint", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false)
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
                name: "SectionParagraphs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateSectionId = table.Column<long>(type: "bigint", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectionParagraphs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SectionParagraphs_TemplateSections_TemplateSectionId",
                        column: x => x.TemplateSectionId,
                        principalTable: "TemplateSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubSectionParagraphs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubSectionId = table.Column<long>(type: "bigint", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false)
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
                name: "IX_TemplateSections_TemplateId",
                table: "TemplateSections",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TableRows_DynamicTableId",
                table: "TableRows",
                column: "DynamicTableId");

            migrationBuilder.CreateIndex(
                name: "IX_TableColumns_DynamicTableId",
                table: "TableColumns",
                column: "DynamicTableId");

            migrationBuilder.CreateIndex(
                name: "IX_SubSections_MasterSectionId",
                table: "SubSections",
                column: "MasterSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_MasterSections_DocumentTemplateId",
                table: "MasterSections",
                column: "DocumentTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageGroups_Order",
                table: "ImageGroups",
                column: "Order");

            migrationBuilder.CreateIndex(
                name: "IX_MasterSectionParagraphs_MasterSectionId",
                table: "MasterSectionParagraphs",
                column: "MasterSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_SectionParagraphs_TemplateSectionId",
                table: "SectionParagraphs",
                column: "TemplateSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubSectionParagraphs_SubSectionId",
                table: "SubSectionParagraphs",
                column: "SubSectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_CoverPageTemplates_DocumentTemplates_DocumentTemplateId",
                table: "CoverPageTemplates",
                column: "DocumentTemplateId",
                principalTable: "DocumentTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicTables_CoverPageTemplates_CoverPageTemplateId",
                table: "DynamicTables",
                column: "CoverPageTemplateId",
                principalTable: "CoverPageTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicTables_TemplateSections_TemplateSectionId",
                table: "DynamicTables",
                column: "TemplateSectionId",
                principalTable: "TemplateSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ImageGroups_CoverPageTemplates_CoverPageId",
                table: "ImageGroups",
                column: "CoverPageId",
                principalTable: "CoverPageTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ImageGroups_MasterSections_MasterSectionId",
                table: "ImageGroups",
                column: "MasterSectionId",
                principalTable: "MasterSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ImageGroups_SubSections_SubSectionId",
                table: "ImageGroups",
                column: "SubSectionId",
                principalTable: "SubSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ImageGroups_TemplateSections_TemplateSectionId",
                table: "ImageGroups",
                column: "TemplateSectionId",
                principalTable: "TemplateSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Images_ImageGroups_ImageGroupId",
                table: "Images",
                column: "ImageGroupId",
                principalTable: "ImageGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MasterSections_DocumentTemplates_DocumentTemplateId",
                table: "MasterSections",
                column: "DocumentTemplateId",
                principalTable: "DocumentTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
