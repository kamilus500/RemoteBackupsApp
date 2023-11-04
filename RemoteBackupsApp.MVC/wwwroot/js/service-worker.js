if ('serviceWorker' in navigator) {
    navigator.serviceWorker.register('/service-worker.js')
        .then(function (registration) {
            console.log('Service Worker zarejestrowany z sukcesem:', registration);
        })
        .catch(function (error) {
            console.log('Błąd rejestracji Service Workera:', error);
        });
}