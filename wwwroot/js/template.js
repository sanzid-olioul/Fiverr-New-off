/*  
    Functions for Page - Views/LetterTemplates/ListTemplates.cshtml start from here
*/
if (controller === "LetterTemplates" && action === "ListTemplates") {

    function commandClick(args) {
        const templateId = args.rowData.LetterTemplateId;
        if (args.commandColumn.type === 'DownloadAsWord') {
            window.location.href = downloadWordTemplateUrl + '/' + templateId;
        } else if (args.commandColumn.type === 'Edit') {
            window.location.href = editTemplateUrl + '/' + templateId;
        } else if (args.commandColumn.type === 'Delete') {
            if (confirm("Are you sure you want to delete this template?")) {
                document.getElementById('tempId').value = templateId;
                const form = document.getElementById('deleteForm');
                form.action = deleteConfirmedUrl + '/' + templateId;
                form.submit();
            }
        }
    }

    function toolbarClick(args) {
        if (args.item.id === "TemplateGrid_excelexport") {
            const grid = document.getElementById("TemplateGrid").ej2_instances[0];
            grid.showSpinner();
            grid.excelExport();
        }
    }

}

/*  
    Functions for Page - Views/LetterTemplates/EditTemplate.cshtml start from here
*/
if (controller === "LetterTemplates" && action === "EditTemplate") {

    let isConvertToPdfChecked = false;

    document.addEventListener('DOMContentLoaded', function () {
        const form = document.getElementById('editTemplate');
        const nameField = document.getElementById('Name');
        const docTypeDropdown = document.getElementById('docTypeDropdown');
        const templateFileInput = document.getElementById('TemplateFile');
        const checkBoxInput = document.getElementById('checkBox');
        const pdfFlagInput = document.getElementById('pdfFlag');

        isConvertToPdfChecked = checkBoxInput.checked;

        form.addEventListener('submit', function (event) {
            pdfFlagInput.value = isConvertToPdfChecked ? 'true' : 'false';

            // Use the browser's built-in validation
            if (!form.checkValidity()) {
                // Prevent form submission if the form is invalid
                event.preventDefault();
            }
        });
    });

    function convertToPdfCalled() {
        isConvertToPdfChecked = !isConvertToPdfChecked;
    }

}

/*  
    Functions for Page - Views/LetterTemplates/MergeTemplate.cshtml start from here
*/
if (controller === "LetterTemplates" && action === "MergeTemplate") {

    document.addEventListener('DOMContentLoaded', function () {
        const form = document.getElementById('mergeTemplateForm');

        form.addEventListener('submit', function (event) {
            setTimeout(function () {
                document.getElementById('mergeTemplateForm').reset();

                document.getElementById('DocType').value = '';
                document.getElementById('DocTypeDomainName').value = '';
                document.getElementById('ConvertToPdf').value = 'N';
                document.getElementById('ConvertToPdf').value = 'N';
            }, 2000);

            form.submit();

        });
    });

    function updateDocType() {
        const dropdown = document.getElementById('letterTemplateDropdown');
        const selectedOption = dropdown.options[dropdown.selectedIndex];

        const docType = selectedOption.getAttribute('data-doc-type');
        const docTypeDomainName = selectedOption.getAttribute('data-doc-type-domain-name');
        const docConvertToPdf = selectedOption.getAttribute('data-convert-to-pdf');

        document.getElementById('DocType').value = docType || '';
        document.getElementById('DocTypeDomainName').value = docTypeDomainName || '';
        document.getElementById('ConvertToPdf').value = docConvertToPdf || '';
    }
}


/*  
    Functions for Page - Views/LetterTemplates/CreateTemplate.cshtml start from here
*/
if (controller === "LetterTemplates" && action === "CreateTemplate") {
    let isConvertToPdfChecked = false;

    document.addEventListener('DOMContentLoaded', function () {
        const form = document.getElementById('createTemplateForm');
        const templateUploader = document.getElementById('templateDocument');

        form.addEventListener('submit', function (event) {
            if (templateUploader && templateUploader.ej2_instances && templateUploader.ej2_instances.length > 0) {
                const uploaderInstance = templateUploader.ej2_instances[0];
                if (uploaderInstance.getFilesData().length === 0) {
                    event.preventDefault();
                    showAlert('Please upload the Template before submitting the form.');
                    return false;
                }
            }

            const convertPdfInput = document.getElementById('pdfFlag');
            convertPdfInput.value = isConvertToPdfChecked ? 'true' : 'false';

            form.submit();
        });
    });

    function convertToPdfCalled() {
        isConvertToPdfChecked = !isConvertToPdfChecked;
    }


}

