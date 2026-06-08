using System.ComponentModel.DataAnnotations;

namespace WordGenerator.Api.Application.DTOs
{
    public class CreateTableColumnDto
    {
        public string Header { get; set; } = null!;

        public int Width { get; set; } = 0;

        public int Order { get; set; }
    }
}