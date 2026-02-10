/*  
    Functions for Page - Views/EligibilityCheckRequests/Index.cshtml start from here
*/
if (controller === "EligibilityCheckRequests" && action === "ListRequests") {

    function clickHandler(args) {
        const target = args.originalEvent.target.closest('button');

        if (target.id === 'refresh') {
            window.location.reload();
        }
    }

    function showModal(message) {
        document.getElementById('modalMessage').innerHTML = message;
        document.getElementById('showModal').style.display = 'block';
        document.getElementById('overlay').style.display = 'block';
    }

    function closeModal() {
        document.getElementById('overlay').style.display = 'none';
        document.getElementById('showModal').style.display = 'none';
        const queueTab = document.getElementById('queueTab');
        queueTab.classList.remove('open');
    }

    function formatResults(markdown, documentName) {
        const cleaned = markdown
            .replace(/<\/?analysis[^>]*>/g, '')
            .replace(/<\/?decision[^>]*>/g, '');

        const html = marked.parse(cleaned);

        return `
        <div class="analysis-header">Eligibility Analysis</div>
        <div class="doc-name">Documents: ${documentName}</div>
        <div class="analysis-body">${html}</div>
    `;
    }


    document.addEventListener("click", function (event) {
        const modal = document.getElementById("showModal");
        const overlay = document.getElementById("overlay");

        if (modal.style.display === "block" && !modal.contains(event.target) && !overlay.contains(event.target)) {
            closeModal();
        }
    });

}