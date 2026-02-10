using System.ComponentModel.DataAnnotations;

namespace LancasterCreditCardDiversion.ViewModels
{
    public class OpenAIConfigViewModel
    {
        public const string ConfigSection = "OpenAI";

        public required string ApiKey { get; set; }
        public required string ModelName { get; set; }
        public required string ApiBaseUrl { get; set; }
    }

}
    