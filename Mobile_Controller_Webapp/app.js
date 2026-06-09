let socket;
const status = document.createElement('div');
status.style.color = "white";
document.body.appendChild(status);

// Connect to the TV's Local IP (This will be provided by your Game Server)
function connect() {
    const url = `ws://${window.location.hostname}:8080`;
    socket = new WebSocket(url);

    socket.onopen = () => { status.innerText = "Connected to Aero Alpha!"; };
    socket.onclose = () => { setTimeout(connect, 2000); }; // Auto-reconnect
}

// Simple data packet sent 30 times per second
function sendInput(x, y, isBoost) {
    if (socket && socket.readyState === WebSocket.OPEN) {
        const payload = JSON.stringify({ x, y, boost: isBoost });
        socket.send(payload);
    }
}

// Initializing connection
connect();
