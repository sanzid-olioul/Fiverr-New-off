using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LancasterCreditCardDiversion.Models;
using Microsoft.AspNetCore.Mvc;

namespace LancasterCreditCardDiversion.ViewModels
{
    public partial class LetterTemplateViewModel : IDocument
    {
        [Required(ErrorMessage = "Template Selection is required")]
        public int LetterTemplateId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = null!;

        public DateTime? PublishedDate { get; set; }

        public byte[]? Content { get; set; }

        public string? ConvertToPdf { get; set; }

        [Required(ErrorMessage = "Document type is required")]
        public string DocType { get; set; } = null!;

        [Required(ErrorMessage = "Document type domain name is required")]
        public string DocTypeDomainName { get; set; } = null!;

        public string? CreatedUser { get; set; }

        public DateTime? CreatedDttm { get; set; }

        public string? ModifiedUser { get; set; }

        public DateTime? ModifiedDttm { get; set; }

        public string? RecordStatus { get; set; }

        public IFormFile? TemplateFile { get; set; }

        public List<LetterTemplateViewModel> TemplatesList { get; set; } = new List<LetterTemplateViewModel>();
        public bool isConvertToPdf { get; set; } = false;
    }
}
