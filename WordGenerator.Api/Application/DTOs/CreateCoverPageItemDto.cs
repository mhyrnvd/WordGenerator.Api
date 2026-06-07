using WordGenerator.Api.Domain.Entities;

namespace WordGenerator.Api.Application.DTOs
{
    public class CreateCoverPageItemDto
    {
        public string Label { get; set; } = null!;

        public string Value { get; set; } = null!;

        public int Order { get; set; }
    }
}
