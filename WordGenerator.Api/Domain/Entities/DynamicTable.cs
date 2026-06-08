namespace WordGenerator.Api.Domain.Entities
{
    public class DynamicTable
    {
        public long Id { get; set; }
        public string Title { get; set; } = null!;
        public int Order { get; set; }
        public long? CoverPageTemplateId { get; set; }
        public virtual CoverPageTemplate? CoverPage { get; set; }
        public long? TemplateSectionId { get; set; }
        public virtual TemplateSection? Section { get; set; }
        public bool ShowRowNumbers { get; set; } = false;
        public string? RowNumberHeader { get; set; }

        public virtual ICollection<TableColumnDefinition> Columns { get; set; } = new List<TableColumnDefinition>();
        public virtual ICollection<TableDataRow> Rows { get; set; } = new List<TableDataRow>();
    }

    public class TableColumnDefinition
    {
        public long Id { get; set; }
        public long DynamicTableId { get; set; }
        public virtual DynamicTable Table { get; set; } = null!;
        public string Header { get; set; } = null!;
        public int Width { get; set; } = 0;
        public int Order { get; set; }
        public bool IsRowNumberColumn { get; set; } = false;

        public virtual ICollection<TableDataCell> Cells { get; set; } = new List<TableDataCell>();
    }

    public class TableDataRow
    {
        public long Id { get; set; }
        public long DynamicTableId { get; set; }
        public virtual DynamicTable Table { get; set; } = null!;
        public int RowNumber { get; set; }

        public virtual ICollection<TableDataCell> Cells { get; set; } = new List<TableDataCell>();
    }

    public class TableDataCell
    {
        public long Id { get; set; }

        public long TableDataRowId { get; set; }
        public virtual TableDataRow Row { get; set; } = null!;

        public long TableColumnDefinitionId { get; set; }  // این کلیدی است که ارور می‌دهد
        public virtual TableColumnDefinition Column { get; set; } = null!;

        public string Value { get; set; } = null!;
    }
}