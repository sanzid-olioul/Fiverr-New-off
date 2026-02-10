/*  
    Functions for Page - Views/HearingDates/ListHearingDates.cshtml start from here
*/
if (controller === "HearingDates" && action === "ListHearingDates") {
    function onActionComplete(args) {
        if (args.requestType === 'save') {
            fetch(updateHearingDateTimeUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(args.data)
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        alert('Hearing Date and Time updated successfully');
                        window.location.reload();
                    } else {
                        alert('Failed to update date and time');
                        window.location.reload();
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                    alert('An error occurred while updating.');

                });
        }
        else if (args.requestType === 'delete') {
            const confirmation = window.confirm("Are you sure you want to delete this date?");

            if (confirmation) {
                const hearingId = args.data[0].HearingId;
                fetch(deleteConfirmedUrl + '/' + hearingId, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    }
                })
                    .then(response => {
                        if (response.ok) {
                            alert("Successfully deleted the date");
                            args.grid.refresh();
                        } else {
                            alert('Failed to delete the record.');
                        }
                    })
                    .catch(error => {
                        console.error('Error during delete:', error);
                    });
            }
            else {
                alert('Delete operation was canceled.');
                window.location.reload();
            }
        }
    }

    function toolbarClick(args) {
        if (args.item.id === "Grid_excelexport") {
            const grid = document.getElementById("Grid").ej2_instances[0];
            grid.showSpinner();
            grid.excelExport();
        }
    }

    function excelExportFormat(args) {
        if (args.column.field === "HearingDttm" && args.value) {
            const date = new Date(args.value);
            args.value = date.toLocaleString('en-US', {
                month: 'short', day: '2-digit', year: 'numeric',
                hour: '2-digit', minute: '2-digit', hour12: true
            });

            args.style = { numberFormat: 'mmm dd, yyyy hh:mm AM/PM', width: 30 };
        }
    }

}

/*  
    Functions for Page - Views/HearingDates/SelectHearingDates.cshtml start from here
*/
if (controller === "HearingDates" && action === "SelectHearingDates") {

    document.addEventListener("DOMContentLoaded", function () {
        let grid = document.getElementById('hearingGrid').ej2_instances[0];
        // Initialize actionComplete event Update and Delete
        grid.actionComplete = function (args) {
            if (args.requestType === 'save') {
                const updatedData = args.data;
                const updatedDttm = args.data.HearingDttm;
                const prevDttm = args.previousData.HearingDttm;
              
                if (updatedDttm !== prevDttm) {
                    let dataSource = grid.dataSource;
                    let rowIndex = dataSource.findIndex(item => item.HearingId === updatedData.HearingId);

                    if (rowIndex !== -1) {
                        dataSource[rowIndex] = updatedData;
                        grid.setProperties({ dataSource: dataSource });
                    }
                    else {
                        console.error('Row index is not found...');
                    }
                } else {
                    console.error('Row index is not found.');
                }

                grid.refresh();
            }
            if (args.requestType === 'delete') {
                let deletedData = args.rowData;
            }
        };
    });

    //function onDateChange(args) {
    //    alert(JSON.stringify(args));
    //    selectedDate = args.value.toISOString();
    //    updateDateTimeList();
    //}

    //function updateDateTimeList() {
    //    let grid = document.getElementById('hearingGrid').ej2_instances[0];

    //    let localDate = new Date(selectedDate);  // Ensure selectedDate is a Date object
    //    // Set the default time to 1:30 PM
    //    localDate.setHours(13); // Set to 1 PM
    //    localDate.setMinutes(30); // Set to 30 minutes

    //    let currentData = grid.dataSource || [];

    //    let newHearingId = currentData.length > 0 ? Math.max(...currentData.map(item => item.HearingId)) + 1 : 1;
    //    currentData.push({
    //        HearingId: newHearingId,
    //        HearingDttm: localDate,
    //    });

    //    grid.dataSource = currentData;
    //    grid.refresh();

    //    selectedDate = [];
    //}


    function onDateChange(args) {
        let grid = document.getElementById('hearingGrid').ej2_instances[0];
        let currentData = grid.dataSource || [];

        let selectedValues = args.values.map(value => {
            let date = new Date(value);
            date.setHours(13); // Set to 1 PM
            date.setMinutes(30); // Set to 30 minutes
            return date.getTime(); // Store time as unique identifier
        });

        // Add newly selected dates
        args.values.forEach(value => {
            let date = new Date(value);
            date.setHours(13);
            date.setMinutes(30);

            let isAlreadyAdded = currentData.some(item => item.HearingDttm.getTime() === date.getTime());
            if (!isAlreadyAdded) {
                let newHearingId = currentData.length > 0 ? Math.max(...currentData.map(item => item.HearingId)) + 1 : 1;
                currentData.push({
                    HearingId: newHearingId,
                    HearingDttm: date,
                });
            }
        });

        // Remove deselected dates
        currentData = currentData.filter(item => selectedValues.includes(item.HearingDttm.getTime()));

        // Update grid data source
        grid.dataSource = currentData;
        grid.refresh();

        // Update selectedDates to reflect current selection
        selectedDates = args.values;
    }



    document.getElementById('dateForm').addEventListener('submit', function (event) {
        const hiddenInput = document.getElementById('selectedDatesAndTimes');
        const grid = document.getElementById('hearingGrid').ej2_instances[0];
        const gridData = grid.dataSource;

        if (gridData.length > 0) {
            const dateTime = gridData.map(record => record.HearingDttm);
            hiddenInput.value = JSON.stringify(dateTime);

        } else {
            alert("Please add at least one date to the grid.");
            event.preventDefault();
        }
    });
}