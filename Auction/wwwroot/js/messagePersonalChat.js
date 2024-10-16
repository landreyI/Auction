document.addEventListener("DOMContentLoaded", function () {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/chathub")
        .withAutomaticReconnect()
        .build();


    connection.on("DeletePersonalChat", function (idChat) {
        if (window.location.pathname === '/User/PersonalChat') {
            window.location.replace('/Home/Home');
        }
        else if (window.location.pathname === '/User/MyMessage') {
            $(`.personalChat[data-idChat="${idChat}"]`).remove();
        }
    });

    connection.on("ReceiveMessageFromUserChat", function (userSenderNick, message, fontColor) {
        let messagesList = document.getElementById("messagesLisChat");
        if (messagesList) {
            let li = document.createElement("li");
            li.classList.add("list-group-item", "d-flex", "justify-content-start", "align-items-center", "border", "border-0");

            let span = document.createElement("span");
            span.classList.add("alert", `alert-${fontColor}`, "d-inline-block", "mb-4", "me-1");

            // Получение текущего времени
            let now = new Date();
            let options = {
                day: '2-digit', // Двузначный день месяца
                month: 'long', // Полное название месяца
                hour: '2-digit', // Двузначный час
                minute: '2-digit', // Двузначные минуты
                hour12: false // 24-часовой формат времени
            };
            let formattedTime = now.toLocaleString('en-US', options);
            formattedTime = formattedTime.replace(',', '');

            // Формирование содержимого сообщения
            span.innerHTML = `<strong>${userSenderNick}:</strong> ${message} <small class="text-muted ms-2">(${formattedTime})</small>`;

            li.appendChild(span);
            messagesList.appendChild(li);
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
