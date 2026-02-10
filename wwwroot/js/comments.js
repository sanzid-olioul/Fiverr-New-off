/*  
    Functions for Page - Views/CaseComments/ListComments.cshtml start from here
*/
if (controller === "CaseComments" && action === "ListComments") {
    function commandClick(args) {
        const commentid = args.rowData.CommentId;
        if (args.commandColumn.type === 'Delete') {
            if (confirm("Are you sure you want to delete this comment?")) {
                document.getElementById('commentId').value = commentid;
                var form = document.getElementById('deleteForm');
                form.action = deleteConfirmedUrl + '/' + commentid;
                form.submit();
            }
        }
    }
}