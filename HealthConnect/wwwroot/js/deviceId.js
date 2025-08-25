function setPersistentCookie(name, value, days) {
    const date = new Date();
    date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000)); // `days` parameter controls expiration
    const expires = "expires=" + date.toUTCString();
    document.cookie = name + "=" + value + ";" + expires + ";path=/";
}

function getCookie(name) {
    const nameEQ = name + "=";
    const cookiesArray = document.cookie.split(';');
    for (let i = 0; i < cookiesArray.length; i++) {
        let cookie = cookiesArray[i].trim();
        if (cookie.indexOf(nameEQ) === 0) return cookie.substring(nameEQ.length, cookie.length);
    }
    return null;
}

function getOrCreateDeviceId() {
    let deviceId = getCookie("deviceUniqueId");
    if (!deviceId) {
        deviceId = 'id-' + Date.now() + '-' + Math.random().toString(36).substr(2, 9);
        setPersistentCookie("deviceUniqueId", deviceId, 30); // Set cookie to expire in 30 Days
    }
    //console.log("Device ID:", deviceId);
    return deviceId;
}

function displayDeviceId() {
    const deviceId = getOrCreateDeviceId();
    document.getElementById("deviceIdDisplay").innerText = deviceId;
}

// Call displayDeviceId once the page loads
window.onload = displayDeviceId;
