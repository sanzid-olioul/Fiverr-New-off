namespace LancasterCreditCardDiversion.Services
{
    public class CaseStatusClass
    {
        private const string yellow = "#FFCC00";
        private const string white = "#FFFFFF";
        private const string red = "#FF0000";
        private const string green= "#00FF00";
        private const string gray = "#969696";
        private const string black = "#000000";

        /// <summary>
        /// Returns a dictionary containing mappings of case statuses to their style class names, background colors, and text colors.
        /// This is used to represent case statuses visually in the application. It is passed as ViewBag to case.js for Conciliation Management page.
        /// </summary>
        public Dictionary<string, (string ClassName, string BackgroundColor, string TextColor)> GetCaseStatusColors()
        {
            return new Dictionary<string, (string, string, string)>
            {
                { "N", ("new", white, black) },                               // New
                { "A", ("active", white, black) },                            // Active
                { "NOA", ("notice-of-appeal", white, red) },                  // Notice of Appeal
                { "CMO", ("cmo-issued", white, black) },                      // CMO Issued
                { "RR", ("rule-returnable", yellow, black) },                 // Rule Returnable Issued
                { "D", ("dismissed", red, white) },                           // Dismissed
                { "S", ("settled", red, white) },                             // Settled
                { "CDC", ("case-dismissed-by-court", red, white) },           // Case Dismissed by Court
                { "CF", ("complaint-filed", white, black) },                  // Complaint Filed
                { "CR", ("complied-with-rule", green, black) },               // Complied With Rule, May Proceed
                { "CO", ("continuance-order", yellow, black) },               // Continuance Order
                { "CGPS", ("continued-pending-settlement", yellow, black) },  // Continued Generally Pending Settlement
                { "DBP", ("discontinued-by-plaintiff", red, white) },         // Discontinued by Plaintiff
                { "DBPWOP", ("discontinued-without-prejudice", red, white) }, // Discontinued by Plaintiff Without Prejudice
                { "DMD", ("not-domiciled", red, white) },                     // Defendant May Not Be Domiciled
                { "DS", ("docs-sufficient", green, black) },                  // Docs Sufficient
                { "EFM", ("error-in-filing", yellow, black) },                // Error in Filing Made
                { "IFP", ("ineligible-for-program", red, white) },            // Ineligible for Program
                { "INCO", ("issued-nco", green, black) },                     // Issued NCO
                { "JAC", ("judgment-by-agreement", red, white) },             // Judgment by Agreement/Consent
                { "NC", ("non-compliant", yellow, black) },                   // Non-Compliant
                { "NS", ("no-service", yellow, black) },                      // No Service
                { "NSF", ("no-sol-or-docs-filed", yellow, black) },           // No SOL or Docs Filed
                { "RFP", ("removed-from-program", red, white) },              // Removed From Program
                { "SM", ("service-made", green, black) },                     // Service Made
                { "SMO", ("service-by-mail", green, black) },                 // Service By Mail Okay
                { "SPC", ("settled-prior-to-conference", red, white) },       // Settled Prior to Conference
                { "SLJ", ("stipulation-in-lieu", red, white) },               // Stipulation in Lieu of Judgment
                { "Default", ("default-status", gray, black) }                // Default

                //Can add more as needed
            };
        }

        /// <summary>
        /// Returns a dictionary mapping document types to case statuses.
        /// This is used to update the case status when a specific document is uploaded or merged. This is passed as ViewBag in LetterTemplates and CaseDocs
        /// </summary>
        public Dictionary<string, string> CaseStatusUpdateOnMerge()
        {
            return new Dictionary<string, string>
            {
                // {DocTypeCode, CaseStatusCode}
                { "CDOC", "CF" },  // Complaint (Initial Document) -> Updates to "Complaint Filed."
                { "CMO", "CMO" },  // Case Management Order -> Updates to "CMO Issued."
                { "CPS", "CGPS" },  // Continuance 90 Days Pending Settlement -> Updates to "Continued Generally Pending Settlement."
                { "CNS", "NS" },  // Continuance No Service -> Updates to "No Service."
                { "RTSCCO", "RR" },  // Rule to Show Cause Rule Returnable with Continuance -> Updates to "Rule Returnable Issued."
                { "RTSCCA", "CR" },  // Rule to Show Cause Cancelled for Compliance -> Updates to "Complied With Rule."
                { "NCO", "INCO" },  // Non-Compliance Order -> Updates to "Issued NCO."
                { "CDNCO", "D" }  // Case Dismissed Non-Compliance with Rule to Show Cause Order -> Updates to "Dismissed."

                // Add more mappings as needed
            };
        }

    }
}
