using LancasterCreditCardDiversion.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LancasterCreditCardDiversion.ViewModels
{
    public partial class CaseDocumentViewModel: IDocument
    {
        public int DocId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = null!;

        public DateTime DocDate { get; set; }

        public byte[]? Content { get; set; } = null!;

        [Required(ErrorMessage = "Document type is required")]
        public string DocType { get; set; } = null!;

        [Required(ErrorMessage = "Document type domain name is required")]
        public string DocTypeDomainName { get; set; } = null!;

        public int CaseId { get; set; }

        public string? CreatedUser { get; set; }

        public DateTime CreatedDttm { get; set; }

        public string? ModifiedUser { get; set; }

        public DateTime? ModifiedDttm { get; set; }

        [Required(ErrorMessage = "Record status is required")]
        public string RecordStatus { get; set; } = null!;

        [Required(ErrorMessage = "Document upload is required")]
        public IFormFile DocumentUpload { get; set; } = null!;

        public bool IsChecked { get; set; }

        public string? TextContent { get; set; }

        public int? WordCount { get; set; }

        public string? DocumentUri { get; set; }

        public List<DocumentDto> Documents { get; set; } = new List<DocumentDto>(); 
    }

    public class DocumentDto
    {
        public string? DocName { get; set; }
        public DateTime CreatedDttm { get; set; }
        public string? DocType { get; set; }
        public string? RecordStatus { get; set; }
    } 

}
