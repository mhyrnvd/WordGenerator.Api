using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordGenerator.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixTableCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoverPageItem_CoverPageTemplate_CoverPageTemplateId",
                table: "CoverPageItem");

            migrationBuilder.DropForeignKey(
                name: "FK_CoverPageTemplate_DocumentTemplates_DocumentTemplateId",
                table: "CoverPageTemplate");

            migrationBuilder.DropForeignKey(
                name: "FK_SectionParagraph_TemplateSection_TemplateSectionId",
                table: "SectionParagraph");

            migrationBuilder.DropForeignKey(
                name: "FK_TemplateSection_DocumentTemplates_TemplateId",
                table: "TemplateSection");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TemplateSection",
                table: "TemplateSection");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SectionParagraph",
                table: "SectionParagraph");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CoverPageTemplate",
                table: "CoverPageTemplate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CoverPageItem",
                table: "CoverPageItem");

            migrationBuilder.RenameTable(
                name: "TemplateSection",
                newName: "TemplateSections");

            migrationBuilder.RenameTable(
                name: "SectionParagraph",
                newName: "SectionParagraphs");

            migrationBuilder.RenameTable(
                name: "CoverPageTemplate",
                newName: "CoverPageTemplates");

            migrationBuilder.RenameTable(
                name: "CoverPageItem",
                newName: "CoverPageItems");

            migrationBuilder.RenameIndex(
                name: "IX_TemplateSection_TemplateId",
                table: "TemplateSections",
                newName: "IX_TemplateSections_TemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_SectionParagraph_TemplateSectionId",
                table: "SectionParagraphs",
                newName: "IX_SectionParagraphs_TemplateSectionId");

            migrationBuilder.RenameIndex(
                name: "IX_CoverPageTemplate_DocumentTemplateId",
                table: "CoverPageTemplates",
                newName: "IX_CoverPageTemplates_DocumentTemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_CoverPageItem_CoverPageTemplateId",
                table: "CoverPageItems",
                newName: "IX_CoverPageItems_CoverPageTemplateId");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "TemplateSections",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TemplateSections",
                table: "TemplateSections",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SectionParagraphs",
                table: "SectionParagraphs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CoverPageTemplates",
                table: "CoverPageTemplates",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CoverPageItems",
                table: "CoverPageItems",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "DynamicTables",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    CoverPageTemplateId = table.Column<long>(type: "bigint", nullable: true),
                    TemplateSectionId = table.Column<long>(type: "bigint", nullable: true),
                    ShowRowNumbers = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RowNumberHeader = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicTables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DynamicTables_CoverPageTemplates_CoverPageTemplateId",
                        column: x => x.CoverPageTemplateId,
                        principalTable: "CoverPageTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DynamicTables_TemplateSections_TemplateSectionId",
                        column: x => x.TemplateSectionId,
                        principalTable: "TemplateSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TableColumns",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DynamicTableId = table.Column<long>(type: "bigint", nullable: false),
                    Header = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Width = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsRowNumberColumn = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableColumns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TableColumns_DynamicTables_DynamicTableId",
                        column: x => x.DynamicTableId,
                        principalTable: "DynamicTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TableRows",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DynamicTableId = table.Column<long>(type: "bigint", nullable: false),
                    RowNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TableRows_DynamicTables_DynamicTableId",
                        column: x => x.DynamicTableId,
                        principalTable: "DynamicTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TableCells",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableDataRowId = table.Column<long>(type: "bigint", nullable: false),
                    TableColumnDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableCells", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TableCells_TableColumns_TableColumnDefinitionId",
                        column: x => x.TableColumnDefinitionId,
                        principalTable: "TableColumns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TableCells_TableRows_TableDataRowId",
                        column: x => x.TableDataRowId,
                        principalTable: "TableRows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DynamicTables_CoverPageTemplateId",
                table: "DynamicTables",
                column: "CoverPageTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicTables_TemplateSectionId",
                table: "DynamicTables",
                column: "TemplateSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_TableCells_TableColumnDefinitionId",
                table: "TableCells",
                column: "TableColumnDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_TableCells_TableDataRowId_TableColumnDefinitionId",
                table: "TableCells",
                columns: new[] { "TableDataRowId", "TableColumnDefinitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TableColumns_DynamicTableId",
                table: "TableColumns",
                column: "DynamicTableId");

            migrationBuilder.CreateIndex(
                name: "IX_TableRows_DynamicTableId",
                table: "TableRows",
                column: "DynamicTableId");

            migrationBuilder.AddForeignKey(
                name: "FK_CoverPageItems_CoverPageTemplates_CoverPageTemplateId",
                table: "CoverPageItems",
                column: "CoverPageTemplateId",
                principalTable: "CoverPageTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CoverPageTemplates_DocumentTemplates_DocumentTemplateId",
                table: "CoverPageTemplates",
                column: "DocumentTemplateId",
                principalTable: "DocumentTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SectionParagraphs_TemplateSections_TemplateSectionId",
                table: "SectionParagraphs",
                column: "TemplateSectionId",
                principalTable: "TemplateSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TemplateSections_DocumentTemplates_TemplateId",
                table: "TemplateSections",
                column: "TemplateId",
                principalTable: "DocumentTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoverPageItems_CoverPageTemplates_CoverPageTemplateId",
                table: "CoverPageItems");

            migrationBuilder.DropForeignKey(
                name: "FK_CoverPageTemplates_DocumentTemplates_DocumentTemplateId",
                table: "CoverPageTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_SectionParagraphs_TemplateSections_TemplateSectionId",
                table: "SectionParagraphs");

            migrationBuilder.DropForeignKey(
                name: "FK_TemplateSections_DocumentTemplates_TemplateId",
                table: "TemplateSections");

            migrationBuilder.DropTable(
                name: "TableCells");

            migrationBuilder.DropTable(
                name: "TableColumns");

            migrationBuilder.DropTable(
                name: "TableRows");

            migrationBuilder.DropTable(
                name: "DynamicTables");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TemplateSections",
                table: "TemplateSections");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SectionParagraphs",
                table: "SectionParagraphs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CoverPageTemplates",
                table: "CoverPageTemplates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CoverPageItems",
                table: "CoverPageItems");

            migrationBuilder.RenameTable(
                name: "TemplateSections",
                newName: "TemplateSection");

            migrationBuilder.RenameTable(
                name: "SectionParagraphs",
                newName: "SectionParagraph");

            migrationBuilder.RenameTable(
                name: "CoverPageTemplates",
                newName: "CoverPageTemplate");

            migrationBuilder.RenameTable(
                name: "CoverPageItems",
                newName: "CoverPageItem");

            migrationBuilder.RenameIndex(
                name: "IX_TemplateSections_TemplateId",
                table: "TemplateSection",
                newName: "IX_TemplateSection_TemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_SectionParagraphs_TemplateSectionId",
                table: "SectionParagraph",
                newName: "IX_SectionParagraph_TemplateSectionId");

            migrationBuilder.RenameIndex(
                name: "IX_CoverPageTemplates_DocumentTemplateId",
                table: "CoverPageTemplate",
                newName: "IX_CoverPageTemplate_DocumentTemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_CoverPageItems_CoverPageTemplateId",
                table: "CoverPageItem",
                newName: "IX_CoverPageItem_CoverPageTemplateId");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "TemplateSection",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TemplateSection",
                table: "TemplateSection",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SectionParagraph",
                table: "SectionParagraph",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CoverPageTemplate",
                table: "CoverPageTemplate",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CoverPageItem",
                table: "CoverPageItem",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CoverPageItem_CoverPageTemplate_CoverPageTemplateId",
                table: "CoverPageItem",
                column: "CoverPageTemplateId",
                principalTable: "CoverPageTemplate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CoverPageTemplate_DocumentTemplates_DocumentTemplateId",
                table: "CoverPageTemplate",
                column: "DocumentTemplateId",
                principalTable: "DocumentTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SectionParagraph_TemplateSection_TemplateSectionId",
                table: "SectionParagraph",
                column: "TemplateSectionId",
                principalTable: "TemplateSection",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TemplateSection_DocumentTemplates_TemplateId",
                table: "TemplateSection",
                column: "TemplateId",
                principalTable: "DocumentTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
