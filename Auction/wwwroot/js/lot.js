function createLotCard(lotData) {
    let lotCardHtml = `
            <div class="card-container">
                <a href="/Auction/InfoLot/${lotData.lotId}" class="text-decoration-none lot" data-idlot="${lotData.lotId}">
                    <div class="card border border-secondary m-2">
                        <div class="position-relative">
                            <img src="${lotData.imgPath}" class="card-img-top rounded" alt="Auction Item" style="height: 200px; object-fit: cover;">
                            <div class="position-absolute top-0 end-0 p-2 bg-warning text-dark rounded-start">
                                <h5 class="card-title mb-0">${lotData.title}</h5>
                            </div>
                        </div>
                        <div class="card-body bg-light">
                            <p class="card-text text-dark">
                                <strong>Current Bid:</strong> <span class="fs-4 currentBid">${lotData.currentPrice}$</span><br>
                            </p>
                            <div class="auction-timer d-flex justify-content-around text-dark" data-idlot="${lotData.lotId}" data-end-time="${lotData.endTime}">
                                <div class="bg-warning p-1 rounded text-center flex-fill mx-1">
                                    <span class="fs-5 d-block days"></span>
                                    days
                                </div>
                                <div class="bg-warning p-2 rounded text-center flex-fill mx-1">
                                    <span class="fs-5 d-block hrs"></span>
                                    hrs
                                </div>
                                <div class="bg-warning p-2 rounded text-center flex-fill mx-1">
                                    <span class="fs-5 d-block min"></span>
                                    min
                                </div>
                                <div class="bg-warning p-2 rounded text-center flex-fill mx-1">
                                    <span class="fs-5 d-block sec"></span>
                                    sec
                                </div>
                            </div>
                            <p></p>
                        </div>
                        <div class="card-footer bg-dark">
                            <p class="card-text text-white">
                                <strong>Location:</strong> ${lotData.location}
                            </p>
                        </div>
                    </div>
                </a>
            </div>
        `;
    return lotCardHtml;
}

//SCROLL PHOTO
var currentMargin = 0;
var imgWidth = 350;
var visibleImgCount = 3; // Количество видимых изображений в контейнере
var totalImgCount = $('#additionalImg > div').length; // Общее количество изображений

$('#scrollLeft').click(function () {
    if (currentMargin < 0) {
        currentMargin += imgWidth;
        $('#additionalImg').css('margin-left', currentMargin + 'px');
    }
});

$('#scrollRight').click(function () {
    if (Math.abs(currentMargin) < imgWidth * (totalImgCount - visibleImgCount)) {
        currentMargin -= imgWidth;
        $('#additionalImg').css('margin-left', currentMargin + 'px');
    }
});


$(document).on('submit', '.lotForm', function (event) {
    event.preventDefault();

    var formData = new FormData(this);
    var form = $(this);
    $.ajax({
        url: $(this).attr('action'),
        type: $(this).attr('method'),
        data: formData,
        processData: false, // Не обрабатываем данные (так как передаем файлы)
        contentType: false, // Отключаем установку типа содержимого
        success: function (response) {
            if (response.success) {
                if (form.attr('id') === 'updateLotForm') {
                    window.location.href = '/Auction/InfoLot?idLot=' + response.idLot;
                }
                if (form.attr('id') === 'lotCreateForm') {
                    window.location.href = '/Home/Home';
                }
                showMessage("Successful!");
            }
            else {
                var errorMessage = response.errorMessage;
                showMessage(errorMessage, false);
            }
        },
        error: function (xhr, status, error) {
            var errorMessage = xhr.status + ': ' + xhr.statusText;
            showMessage('Error - ' + errorMessage, false);
        }
    });
});

$(document).ready(function () {
    $(document).on('change', '.inputFileImg', function (event) {
        const file = event.target.files[0];

        if (file) {
            const reader = new FileReader();
            const input = $(this);

            reader.onload = function (e) {
                const img = input.closest('.img').find('img');
                img.attr('src', e.target.result); // Устанавливаем src для img
            };

            reader.readAsDataURL(file); // Читаем файл как DataURL (для отображения изображения)

            input.closest('.img').find('input.imgPath').remove();
        }
    });
});

function getMyLots(callback) {
    $.ajax({
        type: 'POST',
        url: '/Auction/GetMyLots',
        success: function (response) {
            if (response.success) {
                let myLots = response.myLots;
                callback(myLots);  // Тут вызывается callback
            }
        },
        error: function (xhr, status, error) {
            var errorMessage = xhr.status + ': ' + xhr.statusText;
            showMessage('Error - ' + errorMessage, false);
            callback(null);  // Обработка ошибки
        }
    });
}

$('.getMyLots').click(function () {
    getMyLots(function (myLots) {
        if (myLots) {
            $('#myLotsList').empty();

            myLots.forEach(function (lot) {
                $('#myLotsList').append(` 
                                    <li class="list-group-item d-flex justify-content-between align-items-center bg-light mb-2 rounded">
                                        <div>
                                            <strong>Title:</strong> ${lot.title} <br />
                                            <strong>Price:</strong> ${lot.currentBid}$ <br />
                                        </div>
                                        <a class="btn btn-warning btn-sm" href="/Auction/InfoLot?idLot=${lot.lotId}">Show</a>
                                    </li>`);
            });

            // Отобразить модальное окно
            var modal = new bootstrap.Modal(document.getElementById('myLotsModal'));
            modal.show();
        } else {
            console.log('Error receiving lots');
        }
    });
});