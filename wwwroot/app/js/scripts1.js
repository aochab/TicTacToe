var interval;
function EmailConfirmation(email) {
    if (window.WebSocket) {
        alert("WebSockets are active");
        openSocket(email, "Email");
    }
    else {
        alert("WebSockets are not active");
        interval = setInterval(() => {
            CheckEmailConfirmationStatus(email);
        }, 5000);
    }
}

function GameInvitationConfirmation(id) {
    if (window.WebSocket) {
        alert("WebSockets are active");
        openSocket(id, "GameInvitation");
    }
    else {
        alert("WebSockets are not active");
        interval = setInterval(() => {
            CheckGameInvitationStatus(id);
        }, 5000);
    }