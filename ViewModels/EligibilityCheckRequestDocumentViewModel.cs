using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace LancasterCreditCardDiversion.ViewModels
{
    public partial class EligibilityCheckRequestDocumentViewModel
    {
        public int CheckRequestDocId { get; set; }

        public int ReqId { get; set; }
        public int DocId { get; set; }
    }
}
