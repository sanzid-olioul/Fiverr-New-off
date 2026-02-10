
function showAlert(message) {
    document.getElementById('alertMessage').innerText = message;
    document.getElementById('customAlert').style.display = 'block';
   // document.getElementById('overlay').style.display = 'block';
}
function showAlertSuccess(message) {
    document.getElementById('alertMessageSuccess').innerText = message;
    document.getElementById('customAlertSuccess').style.display = 'block';

    setTimeout(() => {
        document.getElementById('customAlertSuccess').style.display = 'none';

    }, 3000);
}

function closeAlert() {
    document.getElementById('customAlert').style.display = 'none';
   // document.getElementById('overlay').style.display = 'none';
}

// 
window.addEventListener('DOMContentLoaded', function () {
    const popup = document.querySelector('.popup-alert');
    const tabMenu = document.querySelector('.tab-menu-custom');

    // Check if popup has a message to display
    const isCases = controller === "Cases" && (action === "ViewCase" || action === "EditCase" || action === "ListDocuments" || action === "ListComments");
    if (!popup || (isCases && popup.getAttribute('data-has-message') !== "true")) {
        console.warn("No message to display in popup. Skipping popup-related logic.");
        return;
    }

    // When popup has content and is shown
    if (popup.innerHTML) {
        popup.style.display = 'block';
    }
    if (tabMenu) {
        tabMenu.classList.add('lower-z-index');
    }

    // Attach event to close button
    const closeButton = popup.querySelector('.close-btn');
    if (closeButton) {
        closeButton.addEventListener('click', function () {
            popup.style.display = 'none'; // Hide popup
            if (tabMenu) {
                tabMenu.classList.remove('lower-z-index'); // Restore z-index
            }
        });
    } else {
        console.warn("No close button found in the popup-alert.");
    }

    setTimeout(() => {
        popup.style.display = 'none';
        if (tabMenu) {
            tabMenu.classList.remove('lower-z-index'); // Restore z-index
        }
    }, 3000);
});

