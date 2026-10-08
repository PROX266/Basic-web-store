function money(n) {
    return n.toLocaleString('ru-RU', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

function recalc() {
    var total = 0;
    var rows = document.querySelectorAll('.cart-table tbody tr');
    for (var i = 0; i < rows.length; i++) {
        var price = parseFloat(rows[i].dataset.price);
        var qty = parseInt(rows[i].querySelector('.cart-quantity-input').value);
        if (isNaN(qty) || qty < 0) qty = 0;

        var sum = price * qty;
        rows[i].querySelector('.row-sum').textContent = money(sum) + ' ₽';
        total += sum;
    }
    var totalEl = document.getElementById('cart-total');
    if (totalEl) totalEl.textContent = money(total);
}

var inputs = document.querySelectorAll('.cart-quantity-input');
for (var i = 0; i < inputs.length; i++) {
    inputs[i].addEventListener('input', recalc);

    inputs[i].addEventListener('change', function () {
        var form = this.form;
        var qty = parseInt(this.value);
        if (isNaN(qty)) return;

        fetch(form.action, { method: 'POST', body: new FormData(form) }).then(function () {
            if (qty <= 0) location.reload();
        });
    });
}
