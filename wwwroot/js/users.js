/*  
    Functions for Page - Views/Users/ListUsers.cshtml start from here
*/
if (controller === "Users" && action === "ListUsers") {
    function commandClick(args) {
        const username = args.rowData.UserName;
        if (args.commandColumn.type === 'Edit') {
            // var editUrl = '@Url.Action("EditUser", "Users")/' + username;
            // window.location.href = editUrl;
            alert("Does not work yet");
        }

        if (args.commandColumn.type === 'Delete') {
            if (confirm("Are you sure you want to delete this case?")) {
                document.getElementById('user').value = username;
                const form = document.getElementById('deleteForm');
                form.action = deleteConfirmedUrl + '/' + username;
                form.submit();
            }
        }
    }

    function toolbarClick(args) {
        alert(args.item.id);
        if (args.item.id === "UsersGrid_excelexport") {
            const grid = document.getElementById("UsersGrid").ej2_instances[0];
            grid.showSpinner();
            grid.excelExport();
        }
    }
}

/*  
    Functions for Page - Views/Users/CreateUser.cshtml start from here
*/

if (controller === "Users" && action === "CreateUser") {
    const form = document.getElementById('createUserForm');
    form.addEventListener('submit', function (event) {
        const password = document.getElementById('Password').value;
        const confirmPassword = document.getElementById('ConfirmPassword').value;

        if (password !== confirmPassword) {
            event.preventDefault();
            alert('Passwords do not match!');
            return;
        }
        form.submit();
    });
}

