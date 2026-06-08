namespace WordGenerator.Api.Application.Requests
{
    public class GenerateDocumentRequest
    {
        public long TemplateId { get; set; }

        public List<long> SelectedSectionIds { get; set; } = new();
        public bool IncludeCoverPage { get; set; } = true;
        public bool IncludeTableOfContents { get; set; } = true;
    }
}
