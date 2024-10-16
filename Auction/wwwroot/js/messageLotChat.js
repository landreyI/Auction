document.addEventListener("DOMContentLoaded", async function () {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/chathub")
        .build();

    document.getElementById("sendButtonLotChat").disabled = true;

    connection.on("ReceiveMessageGroupLot", function (nickname, message) {
        var messagesList = document.getElementById("messagesListLotChat");
        if (messagesList) {
            var li = document.createElement("li");
            li.classList.add("list-group-item", "d-flex", "justify-content-start", "align-items-center", "border-0");

            var span = document.createElement("span");
            span.classList.add("alert", "alert-primary", "d-inline-block", "mb-4", "me-1");
            span.innerHTML = `<strong>${nickname}:</strong> ${message}`;

            li.appendChild(span);
            messagesList.appendChild(li);
        }
    });

    try {
        await connection.start();
        document.getElementById("sendButtonLotChat").disabled = false;
    } catch (err) {
        console.error(err.toString());
    }

    document.getElementById("sendButtonLotChat").addEventListener("click", async function (event) {
        var lotId = document.getElementById("idLotChat").value;
        var nickname = document.getElementById("userInputLotChat").value;
        var message = document.getElementById("messageInputLotChat").value;

        try {
            await connection.invoke("SendMessageToGroupLot", lotId, nickname, message);
        } catch (err) {
            console.error(err.toString());
        }

        event.preventDefault();
    });
});