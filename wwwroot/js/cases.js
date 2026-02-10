/*
    Functions for Page - Views/Cases/Index.cshtml start from here
*/
if (typeof controller !== "undefined" && controller === "Cases" && action === "Index") {
    function recordClick(args) {
        const caseid = args.rowData.CaseId;
        let clickedColumn = args.column.field;
        if (clickedColumn === "Actions") {
            return;
        }
        window.location.href = viewCaseUrl + '/' + caseid;
    }

    function commandClick(args) {
        const caseid = args.rowData.CaseId;

        if (args.commandColumn.type === 'ViewDocs') {
            //getDocumentsForCase(caseid);
            window.location.href = `/Cases/SetCase/${caseid}`;
        }

        if (args.commandColumn.type === 'Comment') {
            getCommentsForCase(caseid);
        }

        if (args.commandColumn.type === 'Delete') {
            if (confirm("Are you sure you want to delete this case?")) {
                document.getElementById('caseId').value = caseid;
                const form = document.getElementById('deleteForm');
                form.action = deleteUrl + '/' + caseid;
                form.submit();
            }
        }
    }

    function toolbarClick(args) {
        // Check if the 'ExcelExport' toolbar button was clicked
        if (args.item.id === "Grid_excelexport") {
            const grid = document.getElementById("Grid").ej2_instances[0];
            grid.showSpinner();
            grid.excelExport();
        }
    }

    async function getDocumentsForCase(caseId) {
        try {
            const response = await fetch(getDocumentsUrl + '?caseId=' + caseId);
            if (response.ok) {
                const data = await response.json();
                if (data.success) {
                    displayDocuments(data.documents);
                    const modal = document.getElementById('documentsModal');
                    modal.style.display = 'block';
                    document.body.style.overflow = 'hidden'; // Prevent background scrolling
                } else {
                    console.error("Failed to fetch documents");
                }
            } else {
                console.error("Failed to fetch documents:", response.statusText);
            }
        } catch (error) {
            console.error("Error:", error);
        }
    }

    function displayDocuments(documents) {
        const tableBody = document.getElementById('documentsTableBody');
        const noDocumentsMessage = document.getElementById('noDocumentsMessage');

        tableBody.innerHTML = '';

        if (documents.length === 0) {
            noDocumentsMessage.style.display = 'block';
            return;
        } else {
            noDocumentsMessage.style.display = 'none';
        }

        documents.forEach(doc => {
            const row = document.createElement('tr');
            const isPdf = doc.name && doc.name.toLowerCase().endsWith('.pdf');

            row.innerHTML = `
                <td style="font-size: 14px;">${doc.name || 'N/A'}</td>
                <td style="font-size: 14px;">${doc.docType || 'N/A'}</td>
                <td style="font-size: 14px;">${new Date(doc.docDate).toLocaleString()}</td>
                <td style="font-size: 14px;">
                    <div style="display:flex;gap:5px;align-items:center;">
                        ${isPdf ? `<button onclick="openPdfViewerFromModal(${doc.docId})" class="doc-action-btn" title="View PDF">
                            <i class="e-icons e-eye"></i>
                        </button>` : ''}
                        <button onclick="downloadDocument(${doc.docId})" class="doc-action-btn download" title="Download">
                            <i class="e-icons e-download"></i>
                        </button>
                    </div>
                </td>
            `;
            tableBody.appendChild(row);
        });
    }

    function closeDocumentsModal() {
        const modal = document.getElementById('documentsModal');
        modal.style.display = 'none';
        document.body.style.overflow = ''; // Restore scrolling
    }

    function openPdfViewerFromModal(docId) {
        const pdfModal = document.getElementById('pdfViewerModal');
        const pdfFrame = document.getElementById('pdfViewerFrame');
        const url = viewDocumentUrl + '/' + docId;

        // Hide documents modal
        document.getElementById('documentsModal').style.display = 'none';

        // Show PDF viewer with iframe
        pdfFrame.src = url;
        pdfModal.style.display = 'block';
        document.body.style.overflow = 'hidden';
    }

    function closePdfViewer() {
        const modal = document.getElementById('pdfViewerModal');
        const frame = document.getElementById('pdfViewerFrame');

        // Clear iframe and hide modal
        modal.style.display = 'none';
        frame.src = '';

        // Show documents modal again
        const documentsModal = document.getElementById('documentsModal');
        documentsModal.style.display = 'block';
        document.body.style.overflow = 'hidden'; // Keep overflow hidden for documents modal
    }

    function downloadDocument(docId) {
        window.location.href = downloadDocumentUrl + '/' + docId;
    }

    // Support ESC key to close modals
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            const pdfModal = document.getElementById('pdfViewerModal');
            const documentsModal = document.getElementById('documentsModal');
            const commentsModal = document.getElementById('commentsModal');

            if (pdfModal && pdfModal.style.display === 'block') {
                closePdfViewer();
            } else if (documentsModal && documentsModal.style.display === 'block') {
                closeDocumentsModal();
            } else if (commentsModal && commentsModal.style.display === 'block') {
                closeModal();
            }
        }
    });

    // Click outside modal to close
    window.addEventListener('click', function (event) {
        const documentsModal = document.getElementById('documentsModal');
        const commentsModal = document.getElementById('commentsModal');

        if (event.target === documentsModal) {
            closeDocumentsModal();
        } else if (event.target === commentsModal) {
            closeModal();
        }
    });

    async function getCommentsForCase(caseId) {
        try {
            const response = await fetch(caseCommentsUrl + '/' + caseId);
            if (response.ok) {
                const comments = await response.json();
                displayComments(comments);
                const modal = document.getElementById('commentsModal');
                modal.style.display = 'block';
                document.body.style.overflow = 'hidden';

                const viewAllButton = document.getElementById('viewAllCommentsButton');
                if (viewAllButton) {
                    viewAllButton.onclick = function () { goToCommentsPage(caseId); };
                }
            } else {
                console.error("Failed to fetch comments:", response.statusText);
            }
        } catch (error) {
            console.error("Error:", error);
        }
    }

    function displayComments(comments) {
        const tableBody = document.getElementById('commentsTableBody');
        const noCommentsMessage = document.getElementById('noCommentsMessage');

        tableBody.innerHTML = ''; // Clear existing content

        if (comments.length === 0) {
            noCommentsMessage.style.display = 'block';
            return;
        } else {
            noCommentsMessage.style.display = 'none';
        }

        comments.forEach(comment => {
            const row = document.createElement('tr');
            row.innerHTML = `
                    <td style="font-size: 14px;">${comment.createdUser}</td>
                    <td style="font-size: 14px;">${comment.commentText}</td>
                    <td style="font-size: 14px;">${new Date(comment.createdDttm).toLocaleString()}</td>
                `;
            tableBody.appendChild(row);
        });
    }

    function closeModal() {
        const modal = document.getElementById('commentsModal');
        modal.style.display = 'none';
        document.body.style.overflow = ''; // Restore scrolling
    }
}
/*
    Functions for Page - Views/Cases/ViewCase.cshtml start from here
*/

if (typeof controller !== "undefined" && controller === "Cases" && action === "ViewCase") {
    document.getElementById('submitButton').onclick = function () {
        window.location.href = editCaseUrl + '/' + caseId;
    };
}

/*
    Functions for Page - Views/Cases/EditCase.cshtml start from here
*/

if (typeof controller !== "undefined" && controller === "Cases" && action === "EditCase") {
    let isFormDirty = false; // Flag to track form changes

    // Event listener to trigger when the user tries to leave the page or close the browser
    window.addEventListener('beforeunload', function (event) {
        if (isFormDirty) {
            let message = "You have unsaved changes. Are you sure you want to leave this page?";
            event.returnValue = message;
            return message; // For some browsers, you need to return the message explicitly
        }
    });

    document.addEventListener('DOMContentLoaded', function () {
        // Listen for changes in ejs-datepicker, ejs-textbox, and ejs-dropdownlist
        let filingDatePicker = document.getElementById('filingDate')?.ej2_instances?.[0];
        let courtCaseNumberTextbox = document.getElementById('courtCaseNumber')?.ej2_instances?.[0];
        let plaintiffNameTextbox = document.getElementById('plaintiffName')?.ej2_instances?.[0];
        let plaintiffRepNameTextbox = document.getElementById('plaintiffRepName')?.ej2_instances?.[0];
        let plaintiffRep2NameTextbox = document.getElementById('plaintiffRep2Name')?.ej2_instances?.[0];
        let defendantNameTextbox = document.getElementById('defendantName')?.ej2_instances?.[0];
        let defendantRepNameTextbox = document.getElementById('defendantRepName')?.ej2_instances?.[0];
        let defendantRep2NameTextbox = document.getElementById('defendantRep2Name')?.ej2_instances?.[0];
        let caseStatusDropdown = document.getElementById('caseStatusDropdown')?.ej2_instances?.[0];
        let hearingDateDropdown = document.getElementById('hearingDateDropdown')?.ej2_instances?.[0];
        let caseCommentTextbox = document.getElementById('caseComment')?.ej2_instances?.[0];

        // Set the form as dirty when any of the inputs change
        if (filingDatePicker) {
            filingDatePicker.addEventListener('change', function () {
                isFormDirty = true;
            });
        }

        if (courtCaseNumberTextbox) {
            courtCaseNumberTextbox.addEventListener('change', function () {
                isFormDirty = true;
            });
        }

        if (plaintiffNameTextbox) {
            plaintiffNameTextbox.addEventListener('change', function () {
                isFormDirty = true;
            });
        }

        if (plaintiffRepNameTextbox) {
            plaintiffRepNameTextbox.addEventListener('change', function () {
                isFormDirty = true;
            });
        }

        if (plaintiffRep2NameTextbox) {
            plaintiffRep2NameTextbox.addEventListener('change', function () {
                isFormDirty = true;
            });
        }

        if (defendantNameTextbox) {
            defendantNameTextbox.addEventListener('change', function () {
                isFormDirty = true;
            });
        }

        if (defendantRepNameTextbox) {
            defendantRepNameTextbox.addEventListener('change', function () {
                isFormDirty = true;
            });
        }

        if (defendantRep2NameTextbox) {
            defendantRep2NameTextbox.addEventListener('change', function () {
                isFormDirty = true;
            });
        }

        if (caseStatusDropdown) {
            caseStatusDropdown.addEventListener('change', function () {
                isFormDirty = true;
            });
        }

        if (hearingDateDropdown) {
            hearingDateDropdown.addEventListener('change', function () {
                isFormDirty = true;
            });
        }

        if (caseCommentTextbox) {
            caseCommentTextbox.addEventListener('change', function () {
                isFormDirty = true;
            });
        }

        // Reset the dirty flag once the form is submitted
        const form = document.getElementById('editForm');
        form.addEventListener('submit', function () {
            isFormDirty = false;
        });

        // Initialize the filing date change listener
        if (filingDatePicker) {
            filingDatePicker.addEventListener('change', onFilingDateChange);
        }

        // Initialize hidden input for hearing date dropdown
        initializeHiddenInput("hearingDateDropdown", "HearingId");
    });

    function onCaseStatusChangeCheckHearingDate(args) {
        let caseStatusValue = args.value;
        initialCaseStatusValue = caseStatusValue;

        if ((caseStatusValue === 'A' || caseStatusValue === 'C') && (initialHearingIdValue === null || initialHearingIdValue.trim() === '')) {
            showAlert('Please select a Hearing Date for Active and Continued Cases');
        }
    }

    function onHearingDateChangeCheck(args) {
        const hearingDateValue = args.value;
        initialHearingIdValue = hearingDateValue;
        const caseStatusDropdownValue = document.getElementById('caseStatusDropdown')?.ej2_instances?.[0];

        if (hearingDateValue !== null && hearingDateValue.trim() !== '' && initialCaseStatusValue === 'N') {
            caseStatusDropdownValue.value = 'A';
            initialCaseStatusValue = 'A';
        } else if (hearingDateValue === null || hearingDateValue.trim() === '') {
            caseStatusDropdownValue.value = 'N';
            initialCaseStatusValue = 'N';
        }
    }

    async function onFilingDateChange() {
        try {
            const filingDateInput = document.getElementById('filingDate').value || new Date().toISOString().split('T')[0];
            const response = await fetch('hearingDateUrl', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(filingDateInput)
            });

            if (!response.ok) throw new Error("Failed to load hearing dates");

            const hearingDates = await response.json();

            const hearingDateDropdown = document.getElementById('hearingDateDropdown')?.ej2_instances?.[0];
            if (hearingDateDropdown) {
                hearingDateDropdown.dataSource = hearingDates.map(hd => ({
                    Value: hd.value,
                    Text: new Date(hd.text).toLocaleString("en-US", { dateStyle: 'medium', timeStyle: 'short' })
                }));

                hearingDateDropdown.value = hearingDates.length > 0 ? hearingDates[0].Value : '';
                hearingDateDropdown.refresh();
            }
        } catch (error) {
            console.error("Error fetching hearing dates:", error);
        }
    }

    function initializeHiddenInput(dropdownId, hiddenInputName) {
        // Get the dropdown instance and form
        const dropdown = document.getElementById(dropdownId)?.ej2_instances?.[0];
        const form = document.getElementById("createForm");

        if (dropdown && form) {
            let hiddenInput = document.createElement("input");
            hiddenInput.type = "hidden";
            hiddenInput.name = hiddenInputName;
            hiddenInput.value = dropdown.value;

            form.appendChild(hiddenInput);

            // Update the hidden input value whenever the dropdown changes
            dropdown.addEventListener('change', function () {
                hiddenInput.value = dropdown.value;
            });
        }
    }
}

/*
    Functions for Page - Views/Cases/CreateCase.cshtml start from here
*/

if (typeof controller !== "undefined" && controller === "Cases" && action === "CreateCase") {
    let isEligibilityChecked = false;

    document.addEventListener('DOMContentLoaded', function () {
        const form = document.getElementById('createForm');
        const documentsUploader = document.getElementById('uploadedDocuments');

        let savedFilesData = sessionStorage.getItem('uploadedFiles');
        if (savedFilesData) {
            const savedFiles = JSON.parse(savedFilesData);
            if (documentsUploader && documentsUploader.ej2_instances && documentsUploader.ej2_instances.length > 0) {
                const uploaderInstance = documentsUploader.ej2_instances[0];
                if (typeof uploaderInstance.addFiles === "function") {
                    uploaderInstance.addFiles(savedFiles);
                    sessionStorage.removeItem('uploadedFiles');
                }
            }
        }

        form.addEventListener('submit', function (event) {
            const checkbox = document.getElementById('checkBox');
            const isEligibilityChecked = checkbox.checked;

            if ((initialCaseStatusValue === 'A' || initialCaseStatusValue === 'C') && (initialHearingIdValue === null || initialHearingIdValue.trim() === '')) {
                event.preventDefault();
                showAlert('Please select a Hearing Date for Active and Continued Cases');
                return false;
            }

            const eligibilityFlagInput = document.getElementById('eligibilityFlag');
            eligibilityFlagInput.value = isEligibilityChecked ? 'true' : 'false';
            if (isEligibilityChecked) {
                showAlertSuccess('Eligibility AI check will start in the background once the case is Created');
            }

            form.submit();
        });

        const filingDatePicker = document.getElementById('filingDate')?.ej2_instances?.[0];
        if (filingDatePicker) {
            filingDatePicker.addEventListener('change', onFilingDateChange);
        }

        if (filingDatePicker && filingDatePicker.value) {
            onFilingDateChange();
        }

        initializeHiddenInput("hearingDateDropdown", "HearingId");
    });

    function onremove() {
        const checkbox = document.getElementById('checkBox').ej2_instances[0];
        checkbox.checked = false;
        isEligibilityChecked = false;
        return;
    }

    function onCaseStatusChangeCheckHearingDate(args) {
        const caseStatusValue = args.value;
        initialCaseStatusValue = caseStatusValue;

        if ((caseStatusValue === 'A' || caseStatusValue === 'C') && (initialHearingIdValue === null || initialHearingIdValue.trim() === '')) {
            showAlert('Please select a Hearing Date for Active and Continued Cases');
        }
    }

    function onHearingDateChangeCheck(args) {
        const hearingDateValue = args.value;
        initialHearingIdValue = hearingDateValue;
        let caseStatusDropdownValue = document.getElementById('caseStatusDropdown')?.ej2_instances?.[0];

        if (hearingDateValue !== null && hearingDateValue.trim() !== '' && initialCaseStatusValue != 'A') {
            caseStatusDropdownValue.value = 'A';
            initialCaseStatusValue = 'A';
        } else if (hearingDateValue === null || hearingDateValue.trim() === '') {
            caseStatusDropdownValue.value = 'N';
            initialCaseStatusValue = 'N';
        }
    }

    async function onFilingDateChange() {
        try {
            const filingDateInput = document.getElementById('filingDate').value || new Date().toISOString().split('T')[0];
            const response = await fetch(hearingDatesUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(filingDateInput)
            });

            if (!response.ok) throw new Error("Failed to load hearing dates");

            const hearingDates = await response.json();

            const hearingDateDropdown = document.getElementById('hearingDateDropdown')?.ej2_instances?.[0];
            if (hearingDateDropdown) {
                hearingDateDropdown.dataSource = hearingDates.map(hd => ({
                    Value: hd.value,
                    Text: hd.text
                }));

                hearingDateDropdown.value = hearingDates.length > 0 ? hearingDates[0].Value : '';
                hearingDateDropdown.refresh();
            }
        } catch (error) {
            console.error("Error fetching hearing dates:", error);
        }
    }

    function initializeHiddenInput(dropdownId, hiddenInputName) {
        const dropdown = document.getElementById(dropdownId)?.ej2_instances?.[0];
        const form = document.getElementById("createForm");

        if (dropdown && form) {
            let hiddenInput = document.createElement("input");
            hiddenInput.type = "hidden";
            hiddenInput.name = hiddenInputName;
            hiddenInput.value = dropdown.value;

            form.appendChild(hiddenInput);
            dropdown.addEventListener('change', function () {
                hiddenInput.value = dropdown.value;
            });
        }
    }

    function checkEligibility() {
        const uploader = document.getElementById('uploadedDocuments').ej2_instances[0];
        const files = uploader.getFilesData();
        const checkbox = document.getElementById('checkBox').ej2_instances[0];

        if (files.length === 0) {
            alert('Please upload a document to check eligibility.');
            checkbox.checked = false;
            isEligibilityChecked = false;
            return;
        }

        if (!checkbox.checked) {
            checkbox.checked = false;
            isEligibilityChecked = false;
        } else {
            checkbox.checked = true;
            isEligibilityChecked = true;
        }
    }

    // Listen for changes in the Plaintiff and Defendant representative fields
    document.getElementById('plaintiffRepName').addEventListener('change', function (e) {
        handleChange(e, 'plaintiff');
    });

    document.getElementById('plaintiffRep2Name').addEventListener('change', function (e) {
        handleChange(e, 'plaintiff');
    });

    document.getElementById('defendantRepName').addEventListener('change', function (e) {
        handleChange(e, 'defendant');
    });

    document.getElementById('defendantRep2Name').addEventListener('change', function (e) {
        handleChange(e, 'defendant');
    });

    function handleChange(event, type) {
        // Get the two representative fields for the given type
        let rep1, rep2;
        if (type === 'plaintiff') {
            rep1 = document.getElementById('plaintiffRepName');
            rep2 = document.getElementById('plaintiffRep2Name');
        } else if (type === 'defendant') {
            rep1 = document.getElementById('defendantRepName');
            rep2 = document.getElementById('defendantRep2Name');
        }

        // If both fields are filled, swap their values
        if (rep1.value.trim() && rep2.value.trim()) {
            // Swap the values of the two fields
            const temp = rep1.value;
            rep1.value = rep2.value;
            rep2.value = temp;
        }
        else {
            if (!rep1.value.trim() || rep1.value == 'No Attorney') {
                rep1.value = '';
            }
            if (!rep2.value.trim()) {
                rep2.value = '';
            }
        }
    }
}

/*
    Functions for Page - Views/Cases/ConciliationManagement.cshtml start from here
*/

if (typeof controller !== "undefined" && controller === "Cases" && action === "ConciliationManagement") {
    let textEditor;
    let elemContent;
    let elemContentCaseStatus;
    let caseStatusDropdown;
    let elemContentHearingDttmStatus;
    let hearingDttmStatusDropdown;
    let elemContentRecordStatus;
    let recordStatusDropdown;
    let isRowColoringEnabled = true;

    document.addEventListener("DOMContentLoaded", function () {
        let grid = document.getElementById("Grid").ej2_instances[0];

        // Apply initial toggle state
        const toggleSwitch = document.getElementById("rowColorToggle");
        if (toggleSwitch) {
            toggleSwitch.checked = isRowColoringEnabled;
        }

        // Set initial row coloring
        grid.refresh();

        let selectedHearingId = sessionStorage.getItem('selectedHearingId');
        if (selectedHearingId) {
            grid.filterByColumn("HearingDttm", "equal", parseFloat(selectedHearingId));
        }
    });

    // Function to handle the Syncfusion Switch toggle
    function onToggleRowColoring(args) {
        isRowColoringEnabled = args.checked;
        sessionStorage.setItem("isRowColoringEnabled", isRowColoringEnabled);

        const grid = document.getElementById("Grid").ej2_instances[0];
        const rows = grid.getRows();

        //Remove the colors of rows
        rows.forEach(row => {
            Object.values(caseStatusColors).forEach(status => {
                row.classList.remove(status.ClassName);
            });
        });

        if (isRowColoringEnabled) {
            grid.refresh();
        }
    }

    function rowDataBound(args) {
        if (!args.row) return;

        if (!isRowColoringEnabled) {
            // If coloring is disabled, skip adding colors
            return;
        }
        const caseStatus = args.data.CaseStatusCode || "Default";

        // Access the status information
        const statusInfo = caseStatusColors[caseStatus] || caseStatusColors["Default"];
        const cssClass = statusInfo.ClassName;
        const backgroundColor = statusInfo.BackgroundColor;
        const textColor = statusInfo.TextColor;

        args.row.classList.add(cssClass);

        let styleElement = document.getElementById("dynamic-styles");
        if (!styleElement) {
            styleElement = document.createElement('style');
            styleElement.id = "dynamic-styles";
            document.head.appendChild(styleElement);
        }

        // Add dynamic background and text color rules
        let backgroundRule = `.e-grid .e-row.${cssClass} .e-rowcell { background-color: ${backgroundColor}; }`;
        if (!styleElement.innerHTML.includes(backgroundRule)) {
            styleElement.innerHTML += backgroundRule;
        }

        let textRule = `.e-grid .e-row.${cssClass} .e-rowcell { color: ${textColor}; }`;
        if (!styleElement.innerHTML.includes(textRule)) {
            styleElement.innerHTML += textRule;
        }
    }

    function create(args) {
        elemContent = document.createElement('textarea');
        return elemContent;
    }

    let previousComment = "";
    function write(args) {
        previousComment = args.rowData[args.column.field];
        textEditor = new ej.inputs.TextBox({
            multiline: true,
            value: "",
            floatLabelType: 'Auto'
        });
        textEditor.appendTo(elemContent);
    }

    function destroy() {
        textEditor.destroy();
    }

    function read(args) {
        return textEditor.value === "" ? previousComment : textEditor.value;
    }

    function created(args) {
        document.getElementById('Grid').ej2_instances[0].keyConfigs.enter = '';
    }

    function onFilterChange(args) {
        let selectedValue = args.value;
        const grid = document.getElementById("Grid").ej2_instances[0];

        sessionStorage.setItem('selectedHearingId', selectedValue);

        if (selectedValue) {
            selectedValue = parseFloat(selectedValue);
            grid.filterByColumn("HearingDttm", "equal", selectedValue);
        } else {
            grid.clearFiltering();
        }
    }
    function toolbarClick(args) {
        if (args.item.id === "Grid_excelexport") {
            const grid = document.getElementById("Grid").ej2_instances[0];
            grid.showSpinner();
            let gridData = grid.dataSource;

            let promises = gridData.map(async function (row) {
                const caseId = row.CaseId;

                let allComments = await fetchAllComments(caseId);

                let updatedComment = allComments.join("\r\n");

                row.CaseComment = updatedComment;
            });

            Promise.all(promises)
                .then(() => {
                    grid.excelExport();
                    grid.hideSpinner();
                })
                .catch(error => {
                    console.error("Error in fetching or updating comments:", error);
                    grid.hideSpinner();
                });
        }

        if (args.item.id === "Grid_wordexport") {
            const grid = document.getElementById("Grid").ej2_instances[0];
            grid.showSpinner();
            grid.wordExport();
        }
    }

    function fetchAllComments(caseId) {
        return fetch(`/Cases/GetAllCommentsByCaseId?caseId=${caseId}`)
            .then(response => response.json())
            .catch(error => {
                console.error("Error fetching old comments:", error);
                return [];
            });
    }

    function onActionComplete(args) {
        if (args.requestType === 'save') {
            console.log("Saved row data:", args.data);

            // Workaround as HearingId is null from frontend

            HearingIdInt = args.data.HearingDttm;

            const payload = {
                CaseId: args.data.CaseId,
                CourtCaseNumber: args.data.CourtCaseNumber,
                FilingDate: args.data.FilingDate ?? null,
                HearingId: HearingIdInt ?? null,
                CaseStatus: args.data.CaseStatus,
                RecordStatus: args.data.RecordStatus,
                CaseComment: args.data.CaseComment,
                PlaintiffName: args.data.PlaintiffName ?? null,
                PlaintiffRepLawfirmName: args.data.PlaintiffRepLawfirmName ?? null,
                PlaintiffRepName: args.data.PlaintiffRepName ?? null,
                PlaintiffRep2Name: args.data.PlaintiffRep2Name ?? null,
                DefendantName: args.data.DefendantName ?? null,
                DefendantRepLawfirmName: args.data.DefendantRepLawfirmName ?? null,
                DefendantRepName: args.data.DefendantRepName ?? null,
                DefendantRep2Name: args.data.DefendantRep2Name ?? null,
                RecordStatus: args.data.RecordStatus ?? null
            };


            fetch(updateHearingConfInlineUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            })
                .then(r => r.json())
                .then(data => {
                    showAlert(data.success ? 'Case updated successfully' : 'Failed to update case');
                    window.location.reload();
                });
        }
    }

    function createCaseStatus(args) {
        elemContentCaseStatus = document.createElement('input');
        return elemContentCaseStatus;
    }

    function writeCaseStatus(args) {
        caseStatusDropdown = new ej.dropdowns.DropDownList({
            dataSource: caseStatusData,
            fields: { value: 'Value', text: 'Text' },
            value: args.rowData.CaseStatusCode || "N",
            floatLabelType: 'Auto',
            allowFiltering: true
        });
        caseStatusDropdown.appendTo(elemContentCaseStatus);
    }

    function readCaseStatus(args) {
        return caseStatusDropdown.value;
    }

    function createHearingDttmStatus(args) {
        elemContentHearingDttmStatus = document.createElement('input');
        return elemContentHearingDttmStatus;
    }

    function writeHearingDttmStatus(args) {
        hearingDttmStatusDropdown = new ej.dropdowns.DropDownList({
            dataSource: hearingDatesData,
            fields: { value: 'Value', text: 'Text' },
            value: args.rowData.HearingId,
            floatLabelType: 'Auto',
            allowFiltering: true
        });
        hearingDttmStatusDropdown.appendTo(elemContentHearingDttmStatus);
    }


    function readHearingDttmStatus(args) {
        return hearingDttmStatusDropdown.value
            ? parseInt(hearingDttmStatusDropdown.value)
            : null;
    }


    function createRecordStatus(args) {
        elemContentRecordStatus = document.createElement('input');
        return elemContentRecordStatus;
    }

    function writeRecordStatus(args) {
        recordStatusDropdown = new ej.dropdowns.DropDownList({
            dataSource: recordStatusData,
            fields: { value: 'Value', text: 'Text' },
            value: args.rowData.RecordStatusCode || "A",
            floatLabelType: 'Auto',
            allowFiltering: true
        });
        recordStatusDropdown.appendTo(elemContentRecordStatus);
    }

    function readRecordStatus(args) {
        return recordStatusDropdown.value;
    }

    function commandClick(args) {
        let caseid = args.rowData.CaseId;
        let grid = document.getElementById("Grid").ej2_instances[0];
        if (args.commandColumn.type === 'Edit') {
            if (!args.rowData.RecordStatus) {
                args.rowData.RecordStatus = 'A';
            }
            grid.beginEdit();
        }
    }

    function onExcelQueryCellInfo(args) {
        if (args.column && args.column.field === 'CaseStatus') {
            const status = args.data.CaseStatusCode || "Default";
            const colorInfo = caseStatusColors[status] || caseStatusColors["Default"];

            args.style = {
                backColor: colorInfo.BackgroundColor,
                fontColor: colorInfo.TextColor
            };
        }
    }

    function excelExportComplete(args) {
        this.hideSpinner();
    }
}

/*
    Functions for Page - Views/Cases/AllCasesSearch.cshtml start from here
*/

if (typeof controller !== "undefined" && controller === "Cases" && action === "AllCasesSearch") {
    function handleKeyPress(event) {
        if (event.key === 'Enter') {
            onSearchClick();
        }
    }

    function onSearchClick() {
        const statusDropdownObj = document.getElementById("caseStatusDropdown").ej2_instances[0];
        const hearingDropdownObj = document.getElementById("hearingIdDropdown").ej2_instances[0];

        const selectedRecords = {
            CourtCaseNumber: document.getElementById("courtCaseNo").value,
            FilingDateSearchValue: document.getElementById("filingDate").value,
            CaseStatus: statusDropdownObj.value,
            PlaintiffName: document.getElementById("plaintiffName").value,
            PlaintiffRepName: document.getElementById("repName").value,
            DefendantName: document.getElementById("defendantName").value,
            DefendantRepName: document.getElementById("defRepName").value,
            HearingDateRange: hearingDropdownObj.value,
        };

        fetch(searchCasesUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(selectedRecords),
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP error! Status: ${response.status}`);
                }
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    const transformedResults = data.results.map(item => ({
                        CaseId: item.caseId,
                        CourtCaseNumber: item.courtCaseNumber,
                        HearingId: item.hearingId,
                        HearingDttm: item.hearingDttm,
                        CaseStatus: item.caseStatus,
                        CaseComment: item.caseComment,
                        FilingDateSearchValue: item.filingDateSearchValue,
                        PlaintiffName: item.plaintiffName,
                        PlaintiffRepName: item.plaintiffRepName,
                        PlaintiffRep2Name: item.plaintiffRep2Name,
                        DefendantName: item.defendantName,
                        DefendantRepName: item.defendantRepName,
                        DefendantRep2Name: item.defendantRep2Name,
                        RecordStatus: item.recordStatus,
                    }));

                    const grid = document.getElementById('Grid').ej2_instances[0];
                    grid.dataSource = transformedResults;
                    grid.refresh();
                } else {
                    console.error("Failed to load data");
                }
            })
            .catch(error => {
                let errorMessage = error.message || 'An unknown error occurred';
                alert(`Error: ${errorMessage}`);
            });
    }

    function onClearClick() {
        const statusDropdownObj = document.getElementById("caseStatusDropdown").ej2_instances[0];
        const hearingDropdownObj = document.getElementById("hearingIdDropdown").ej2_instances[0];

        document.getElementById("courtCaseNo").value = '';
        document.getElementById("plaintiffName").value = '';
        document.getElementById("filingDate").value = '';
        document.getElementById("repName").value = '';
        document.getElementById("defendantName").value = '';
        document.getElementById("defRepName").value = '';
        document.getElementById("caseStatusDropdown").value = '';
        document.getElementById("hearingIdDropdown").value = '';
        statusDropdownObj.value = null;
        hearingDropdownObj.value = null;

        const grid = document.getElementById("Grid").ej2_instances[0];
        grid.clearFiltering();
    }

    function toolbarClick(args) {
        if (args.item.id === "Grid_excelexport") {
            const grid = document.getElementById("Grid").ej2_instances[0];
            grid.showSpinner();
            grid.excelExport();
        }
    }
}