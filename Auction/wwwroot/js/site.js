function showMessage(message, isSuccess = true) {
    var alertClass = isSuccess ? 'alert-success' : 'alert-danger';
    var alert = $('<div class="alert ' + alertClass + ' position-fixed top-0 end-0 p-3" style="z-index: 1050;">')
        .text(message)
        .appendTo('body');

    setTimeout(function () {
        alert.fadeOut('slow', function () {
            alert.remove();
        });
    }, 5000);
}

$(document).on('submit', '.ajax-form', function (event) {
    event.preventDefault();

    var form = $(this);
    $.ajax({
        type: form.attr('method'),
        url: form.attr('action'),
        data: form.serialize(),
        success: function (response) {
            if (response.success) {
                // Закрытие модальных окон, если они открыты
                form.closest('.modal').modal('hide');
                if (form.attr('id') === 'authorizationForm' || form.attr('id') === 'registrationForm' || form.attr('id') === 'deleteChatForm') {
                    location.reload();
                }
                if (form.attr('id') === 'deleteLot') {
                    window.location.href = '/Home/Home';
                }
                if (form.attr('id') === 'deleteAutoBidForm') {
                    let idLot = form.find('input[name="idLot"]').val();
                    $(`#autoBid-${idLot}`).remove();
                }
                if (form.attr('id') === 'personalChatForm') {
                    document.getElementById("messageInputPersonalChat").value = "";
                    return;
                }

                showMessage("Successful!");

                // Вызов инициализации наблюдателей для новых элементов
                initializeObservers();
            } else {
                var errorMessage = response.errorMessage || 'An unknown error occurred.';
                showMessage(errorMessage, false);
            }
        },
        error: function (xhr, status, error) {
            var errorMessage = xhr.status + ': ' + xhr.statusText;
            showMessage('Error - ' + errorMessage, false);
        }
    });
});


function togglePasswordVisibility() {
    var passwordInput = document.getElementById("formSignupPassword");

    if (passwordInput.type === "password") {
        passwordInput.type = "text";
    } else {
        passwordInput.type = "password";
    }
}

function togglePasswordVisibilityAuth() {
    var passwordInput = document.getElementById("formSignupPasswordAuth");

    if (passwordInput.type === "password") {
        passwordInput.type = "text";
    } else {
        passwordInput.type = "password";
    }
}