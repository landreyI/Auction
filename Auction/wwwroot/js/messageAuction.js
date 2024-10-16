document.addEventListener("DOMContentLoaded", function () {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/chathub")
        .withAutomaticReconnect()
        .build();

    // Auction message
    connection.on("ReceiveSystemAlert", function (message) {
        let notification = document.getElementById("notificationSystemAlert");
        if (notification) {
            notification.innerHTML = `<strong>${message}</strong>`;
            notification.style.display = "block";
            setTimeout(() => {
                notification.style.display = "none";
            }, 5000);
        }
    });

    // Этот код срабатывает при загрузке страницы после перезагрузки
    window.onload = function () {
        // Проверяем, есть ли данные в sessionStorage
        let message = sessionStorage.getItem("message");
        let isBlackout = sessionStorage.getItem("isBlackout") === 'true';

        // Если есть сообщение, показываем его
        if (message) {
            showMessage(message, isBlackout);

            // Очищаем sessionStorage, чтобы сообщение не появлялось повторно
            sessionStorage.removeItem("message");
            sessionStorage.removeItem("isBlackout");
        }
    };

    connection.on("ReceiveNewBidMessage", function (idLot, user, bid) {
        let currentBidElementLot = document.querySelector(`.lot[data-idLot="${idLot}"] .currentBid`);

        if (currentBidElementLot) {
            currentBidElementLot.textContent = `$${bid},00`;
        }

        let containerInfoLot = document.querySelector(`#containerInfoLot[data-idLot="${idLot}"]`);

        let currentBidElement = containerInfoLot.querySelector('#currentBid');
        let totalBidsElement = containerInfoLot.querySelector('#totalBids');
        let notification = containerInfoLot.querySelector('#notificationNewBid');

        // Проверяем, существует ли notification
        if (notification) {
            notification.innerHTML = `<strong>${user} made a new bid: $${bid},00</strong>`;
            notification.style.display = "block";

            // Инкрементируем количество ставок
            let totalBidsText = totalBidsElement.textContent;

            // Используем регулярное выражение для извлечения числа
            let totalBidsMatch = totalBidsText.match(/\d+/);
            let totalBids = totalBidsMatch ? parseInt(totalBidsMatch[0], 10) : 0; // Преобразуем текст в число или присваиваем 0, если ничего не найдено

            // Увеличиваем количество ставок
            totalBidsElement.textContent = `Total Bids: ${totalBids + 1}`;
            currentBidElement.textContent = `$${bid},00`;

            setTimeout(() => {
                notification.style.display = "none"; // Скрываем уведомление через 5 секунд
            }, 5000);
        }
    });


    console.log("Starting SignalR connection...");
    connection.start()
        .then(function () {
            console.log("SignalR connected successfully.");
        })
        .catch(function (err) {
            console.error("SignalR connection failed:", err);
        });

    connection.onreconnecting(function (err) {
        console.warn("SignalR reconnecting:", err);
    });

    connection.onreconnected(function () {
        console.log("SignalR reconnected successfully.");
    });

    connection.onclose(function (err) {
        console.error("SignalR connection closed:", err);
    });

});
