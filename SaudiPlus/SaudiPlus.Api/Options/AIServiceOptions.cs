using System.ComponentModel.DataAnnotations;

namespace SaudiPlus.Api.Options
{
    public sealed class AIServiceOptions
    {
        public const string SectionName = "AIService";

        [Required]
        [Url]
        public string BaseUrl { get; set; } = string.Empty;

        [Required]
        public string Model { get; set; } = string.Empty;

        [Range(1, 300)]
        public int TimeoutSeconds { get; set; }

        [Range(0, 10)]
        public int MaxRetries { get; set; }
    }
}
