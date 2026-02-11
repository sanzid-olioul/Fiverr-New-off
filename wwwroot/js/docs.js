/*  
    Functions for Page - Views/CaseDocs/ListDocuments.cshtml start from here
*/
if (controller === "CaseDocs" && action === "ListDocuments") {

    function clickHandler(args) {
        const target = args.originalEvent.target.closest('button');
        if (target.id === 'check') {
            checkEligibility();
        }
        else if (target.id === 'refresh') {
            window.location.reload();
        }
        else if (target.id === 'queue') {
            window.location.href = requestsListUrl;
        }
        else if (target.id === 'excelexport') {
            const grid = document.getElementById("DocumentGrid").ej2_instances[0];

            if (grid) {
                grid.showSpinner();
                grid.excelExport().then(() => {
                    grid.hideSpinner();
                }).catch(error => {
                    console.error("Excel Export Error:", error);
                    grid.hideSpinner();
                });
            } else {
                console.error("Grid instance not found!");
            }
        }
    }

    function checkEligibility() {
        const grid = document.getElementById('DocumentGrid').ej2_instances[0];
        const selectedRecords = grid.getSelectedRecords();

        if (selectedRecords.length === 0) {
            showAlert('Please select at least one document.');
            return;
        }

        fetch(checkEligibilityUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(selectedRecords)
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    showModal(data.message);
                } else {
                    showModal('Check initialization failed.');
                }
            })
            .catch(error => {
                let errorMessage = error.message || 'An unknown error occurred';
                showModal(`Error: ${errorMessage}`);
            });
    }

    function commandClick(args) {
        try {
            const documentId = (args && args.rowData && (args.rowData.DocId || args.rowData.docId)) ||
                (args && args.data && (args.data.DocId || args.data.docId));

            if (typeof console !== 'undefined') console.log('commandClick args:', args, 'resolved documentId:', documentId);

            const cmdType = (args && args.commandColumn && args.commandColumn.type) || args.commandName || (args && args.command && args.command.type) || '';

            if (cmdType === 'View' || cmdType === 'view') {
                // Open the viewer page (Syncfusion viewer) with the docId so the viewer page initializes and loads the PDF
                openPdfViewer(documentId);
                //const viewerUrl = '/PdfViewer?docId=' + encodeURIComponent(documentId);
                //const opened = window.open(viewerUrl, '_blank');
                //if (!opened) window.location.href = viewerUrl;
                return;
            }
            else if (cmdType === 'Download' || cmdType === 'download') {
                window.location.href = downloadDocumentUrl + '/' + documentId;
            } else if (cmdType === 'Edit' || cmdType === 'edit') {
                window.location.href = editDocumentUrl + '/' + documentId;
            } else if (cmdType === 'Delete' || cmdType === 'delete') {
                if (confirm("Are you sure you want to delete this document?")) {
                    document.getElementById('docId').value = documentId;
                    const form = document.getElementById('deleteForm');
                    form.action = deleteConfirmedUrl + '/' + documentId;
                    form.submit();
                }
            }
        } catch (ex) {
            console.error('commandClick error', ex, args);
        }
    }

    function showModal(message) {
        document.getElementById('modalMessage').innerHTML = message;
        document.getElementById('showModal').style.display = 'block';
        document.getElementById('overlay').style.display = 'block';
    }

    function closeModal() {
        document.getElementById('showModal').style.display = 'none';
        document.getElementById('overlay').style.display = 'none';  
    }

    function excelExportFormat(args) {
        if (args.column.field === "DocDate" && args.value) {
            const date = new Date(args.value); // Convert to JavaScript Date object

            args.value = date.toLocaleString('en-US', {
                month: 'short', day: '2-digit', year: 'numeric',
                hour: '2-digit', minute: '2-digit', hour12: true
            });

            args.style = { numberFormat: 'mmm dd, yyyy hh:mm AM/PM', width: 30 };
        }
    }

}

/*  
    Functions for Page - Views/CaseDocs/UploadDocument.cshtml start from here
*/
if (controller === "CaseDocs" && action === "UploadDocument") {
    document.addEventListener('DOMContentLoaded', function () {
        const form = document.getElementById('uploadForm');
        const documentUploader = document.getElementById('documentUpload');

        form.addEventListener('submit', function (event) {
            if (documentUploader && documentUploader.ej2_instances && documentUploader.ej2_instances.length > 0) {
                const uploaderInstance = documentUploader.ej2_instances[0];

                if (uploaderInstance.getFilesData().length === 0) {
                    event.preventDefault();
                    showAlert('Please upload the Document before submitting the form.');
                    return false;
                }
            }

            form.submit();
        });
    });
}