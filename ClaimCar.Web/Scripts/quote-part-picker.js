(function () {
    var parts = [
        ['PT.DDH.37', 'Camera trước'],
        ['PT.DDH.7', 'Bình ắc quy'],
        ['PT.DDH.95', 'Ga điều hòa'],
        ['PT.THV', 'Thân vỏ'],
        ['PT.DC', 'Động cơ'],
        ['PT.DC.23', 'Bugi'],
        ['PT.DC.17', 'Bobin đánh lửa'],
        ['PT.DC.50', 'Dầu động cơ'],
        ['PT.DC.67', 'Hộp lọc gió'],
        ['PT.DDH.1', 'Âm ly'],
        ['PT.DDH.107', 'Hộp dàn lạnh điều hòa'],
        ['PT.DDH.108', 'Hộp điều khiển âm thanh']
    ];
    var activeTable = null;

    function ensureModal() {
        var modal = document.getElementById('QuotePartPickerModal');
        if (modal) return modal;
        modal = document.createElement('div');
        modal.id = 'QuotePartPickerModal';
        modal.className = 'code-picker-modal';
        modal.setAttribute('aria-hidden', 'true');
        modal.innerHTML = '<div class="code-picker-dialog" role="dialog" aria-modal="true" aria-labelledby="QuotePartPickerTitle"><div class="code-picker-header"><h2 id="QuotePartPickerTitle">Lựa chọn phụ tùng</h2><button type="button" class="code-picker-close quote-part-picker-close" aria-label="Đóng">×</button></div><div class="part-picker-body"><table class="grid-table part-picker-table"><thead><tr><th>Mã</th><th>Tên phụ tùng</th></tr></thead><tbody id="QuotePartChoices"></tbody></table></div><div class="code-picker-footer"><button type="button" class="btn quote-part-picker-close">Đóng</button></div></div>';
        document.body.appendChild(modal);
        return modal;
    }

    function closePicker() {
        var modal = document.getElementById('QuotePartPickerModal');
        if (!modal) return;
        modal.className = 'code-picker-modal';
        modal.setAttribute('aria-hidden', 'true');
    }

    function addPart(part) {
        if (!activeTable) return;
        var body = activeTable.querySelector('.quote-part-rows');
        var codes = body.querySelectorAll('input[name$=".PartCode"]');
        for (var i = 0; i < codes.length; i++) {
            if (codes[i].value === part[0]) {
                if (window.ClaimCarNotification) ClaimCarNotification.warning('Mã phụ tùng ' + part[0] + ' đã được chọn.');
                return;
            }
        }
        var index = body.querySelectorAll('tr').length;
        var prefix = 'Items[' + index + ']';
        var row = document.createElement('tr');
        row.innerHTML = '<td><input type="hidden" name="' + prefix + '.Id" value="0"><input type="hidden" name="' + prefix + '.PartCode"><input class="form-control" name="' + prefix + '.PartName" readonly></td>' +
            '<td><input class="form-control right" type="number" min="0" name="' + prefix + '.Quantity" value="1"></td>' +
            '<td><input class="form-control" name="' + prefix + '.Proposal"></td>' +
            '<td><input class="form-control" name="' + prefix + '.PartType"></td>' +
            '<td><input class="form-control right" type="number" min="0" name="' + prefix + '.PartPrice" value="0"></td>' +
            '<td><input class="form-control right" type="number" min="0" name="' + prefix + '.PaintCost" value="0"></td>' +
            '<td><input class="form-control right" type="number" min="0" name="' + prefix + '.LaborCost" value="0"></td>' +
            '<td class="num">0</td>';
        row.querySelector('input[name$=".PartCode"]').value = part[0];
        row.querySelector('input[name$=".PartName"]').value = part[1];
        body.appendChild(row);
        closePicker();
    }

    function openPicker(trigger) {
        activeTable = trigger.closest('table');
        var modal = ensureModal();
        var body = document.getElementById('QuotePartChoices');
        body.innerHTML = '';
        for (var i = 0; i < parts.length; i++) {
            (function (part) {
                var row = document.createElement('tr');
                row.tabIndex = 0;
                row.innerHTML = '<td></td><td></td>';
                row.cells[0].textContent = part[0];
                row.cells[1].textContent = part[1];
                row.onclick = function () { addPart(part); };
                row.onkeydown = function (event) { if (event.key === 'Enter' || event.key === ' ') { event.preventDefault(); addPart(part); } };
                body.appendChild(row);
            }(parts[i]));
        }
        modal.className = 'code-picker-modal open';
        modal.setAttribute('aria-hidden', 'false');
    }

    document.addEventListener('click', function (event) {
        var trigger = event.target.closest ? event.target.closest('.part-add-trigger') : null;
        if (trigger) { event.preventDefault(); openPicker(trigger); return; }
        if (event.target.className.indexOf('quote-part-picker-close') !== -1 || event.target.id === 'QuotePartPickerModal') closePicker();
    });
    document.addEventListener('keydown', function (event) { if (event.key === 'Escape' || event.keyCode === 27) closePicker(); });
}());
