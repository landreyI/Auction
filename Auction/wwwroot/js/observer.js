var visibleTimers = new Map(); // Храним запущенные таймеры
var observer; // Объявляем переменную на верхнем уровне

function updateTimers(elements) {
    var now = new Date().getTime();
    elements.each(function () {
        var endTimeStr = $(this).data('end-time');
        var endTime = new Date(endTimeStr).getTime();
        var timeLeft = endTime - now;

        if (timeLeft <= 0) {
            $(this).find('.days').text('0');
            $(this).find('.hrs').text('0');
            $(this).find('.min').text('0');
            $(this).find('.sec').text('0');
        } else {
            var days = Math.floor(timeLeft / (1000 * 60 * 60 * 24));
            var hours = Math.floor((timeLeft % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
            var minutes = Math.floor((timeLeft % (1000 * 60 * 60)) / (1000 * 60));
            var seconds = Math.floor((timeLeft % (1000 * 60)) / 1000);

            $(this).find('.days').text(days);
            $(this).find('.hrs').text(hours);
            $(this).find('.min').text(minutes);
            $(this).find('.sec').text(seconds);
        }
    });
}

// Создаем наблюдателя
observer = new IntersectionObserver(function (entries) {
    entries.forEach(function (entry) {
        var element = $(entry.target);

        if (entry.isIntersecting) {
            // Если элемент стал видимым, запускаем интервал обновления таймера
            if (!visibleTimers.has(entry.target)) {
                var timerId = setInterval(function () {
                    updateTimers(element);
                }, 1000);
                visibleTimers.set(entry.target, timerId);
            }
        } else {
            // Если элемент перестал быть видимым, останавливаем интервал
            if (visibleTimers.has(entry.target)) {
                clearInterval(visibleTimers.get(entry.target));
                visibleTimers.delete(entry.target);
            }
        }
    });
}, {
    root: null, // наблюдаем относительно окна браузера
    rootMargin: '0px',
    threshold: 0.1 // доля элемента
});

// Функция инициализации наблюдателей
function initializeObservers() {
    $('.auction-timer').each(function () {
        observer.observe(this);
    });
}

initializeObservers();