// IDocument.cs
namespace LancasterCreditCardDiversion.Models
{
    /// <summary>
    /// Defines a document interface with common properties for all document types.
    /// </summary>
    public interface IDocument
    {
        public byte[]? Content { get; set; }
        public string Name { get; set; }
    }
}