using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordGenerator.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDuplicateHeaderTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TableCells_TableDataRowId_TableColumnDefinitionId",
                table: "TableCells");

            migrationBuilder.CreateIndex(
                name: "IX_TableCells_TableDataRowId",
                table: "TableCells",
                column: "TableDataRowId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TableCells_TableDataRowId",
                table: "TableCells");

            migrationBuilder.CreateIndex(
                name: "IX_TableCells_TableDataRowId_TableColumnDefinitionId",
                table: "TableCells",
                columns: new[] { "TableDataRowId", "TableColumnDefinitionId" },
                unique: true);
        }
    }
}
