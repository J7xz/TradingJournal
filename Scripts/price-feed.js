// @ts-nocheck
$(function () {
    console.log('price-feed.js loaded');

    var connection = $.hubConnection();
    var hub = connection.createHubProxy('priceHub');

    hub.on('priceUpdate', function (data) {
        console.log('Full price data received:', data);

        // Update each symbol card
        $('[data-symbol]').each(function () {
            var card = $(this);
            var symbol = $(this).data('symbol');

            if (data[symbol] !== undefined) {
                var quote = data[symbol];
                var price = quote.c;
                var change = quote.d;
                var changePercent = quote.dp;
                var high = quote.h;
                var low = quote.l;
                var open = quote.o;
                var previousClose = quote.pc;

                // 1. Update Price
                var priceElement = $(this).find('.price-value');
                var currentText = priceElement.text().replace('$', '');
                var currentPrice = parseFloat(currentText) || 0;
                priceElement.text('$' + price.toFixed(2));

                // 2. Price color
                priceElement.removeClass('up down');
                if (currentPrice > 0) {
                    if (price > currentPrice) {
                        priceElement.addClass('up');
                        card.addClass('flash-up').removeClass('flash-down');
                        setTimeout(function () { card.removeClass('flash-up'); }, 500);
                    } else if (price < currentPrice) {
                        priceElement.addClass('down');
                        card.addClass('flash-down').removeClass('flash-up');
                        setTimeout(function () { card.removeClass('flash-down'); }, 500);
                    }
                }

                // 3. Change Badge
                var changeElement = $(this).find('.price-change-badge .badge');
                var changeClass = change >= 0 ? 'positive' : 'negative';
                var arrow = change >= 0 ? '▲' : '▼';
                changeElement.text(arrow + ' ' + Math.abs(changePercent).toFixed(2) + '%');
                changeElement.removeClass('positive negative').addClass(changeClass);

                // 4. Day Range (High/Low)
                var rangeElement = $(this).find('.price-range');
                if (rangeElement.length) {
                    rangeElement.html('H: $' + high.toFixed(2) + ' | L: $' + low.toFixed(2));
                }

                // 5. Open / Previous Close
                var openElement = $(this).find('.price-open');
                if (openElement.length) {
                    openElement.text('Open: $' + open.toFixed(2) + ' | Prev: $' + previousClose.toFixed(2));
                }

                // 6. Volume (we don't get volume from quote endpoint, keep random for demo)
                var volumeElement = $(this).find('.price-volume');
                if (volumeElement.length) {
                    var volume = Math.floor(Math.random() * 10000) + 5000;
                    volumeElement.text('Vol: ' + volume.toLocaleString());
                }

                // 7. Price Change in dollars
                var changeDollarElement = $(this).find('.price-change-dollar');
                if (changeDollarElement.length) {
                    var changeClassDollar = change >= 0 ? 'text-profit' : 'text-loss';
                    changeDollarElement.text((change >= 0 ? '+' : '') + '$' + change.toFixed(2));
                    changeDollarElement.removeClass('text-profit text-loss').addClass(changeClassDollar);
                }

                // 8. Update sparkline (trend)
                var sparklineElement = $(this).find('.price-sparkline');
                if (sparklineElement.length) {
                    var trend = change >= 0 ? '📈' : '📉';
                    var color = change >= 0 ? 'var(--accent-green)' : 'var(--accent-red)';
                    sparklineElement.html('<span style="color:' + color + '; font-size: 1.2rem;">' + trend + '</span>');
                }
            }
        });
    });

    connection.start().done(function () {
        console.log('SignalR connected!');
    }).fail(function (err) {
        console.error('SignalR connection failed:', err);
    });
});