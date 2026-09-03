(function () {
    var catalogs = {
        cause: [
            { name: 'Đâm, va', items: [['NNTT.1.1', 'Đâm va vật cố định'], ['NNTT.1.2', 'Đâm va khác']] },
            { name: 'Thuỷ kích', items: [['NNTT.10', 'Thuỷ kích']] }
        ],
        behavior: [
            { name: 'Chạy quá tốc độ cho phép', items: [['HV.1.1', 'Chạy quá tốc độ cho phép 10%'], ['HV.1.2', 'Chạy quá tốc độ cho phép từ 10%-20%'], ['HV.1.3', 'Chạy quá tốc độ cho phép từ 20% đến 30%']] },
            { name: 'Chở quá trọng tải cho phép', items: [['HV.2.1', 'Chở quá trọng tải cho phép'], ['HV.2.2', 'Chở quá trọng tải cho phép 10% đến 20%'], ['HV.2.3', 'Chở quá trọng tải cho phép từ 20% đến 30%']] },
            { name: 'Chở quá số người cho phép', items: [['HV.3.1', 'Chở quá số người cho phép 10%'], ['HV.3.2', 'Chở quá số người cho phép từ 10% đến 20%'], ['HV.3.3', 'Chở quá số người cho phép từ 20% đến 30%']] }
        ],
        area: [
            { name: 'Miền Bắc', items: [['KV.1.1', 'TP Hà Nội'], ['KV.1.2', 'Tỉnh Cao Bằng'], ['KV.1.3', 'Tỉnh Tuyên Quang'], ['KV.1.4', 'Tỉnh Điện Biên'], ['KV.1.5', 'Tỉnh Bắc Ninh'], ['KV.1.6', 'TP Hải Phòng'], ['KV.1.7', 'Tỉnh Quảng Ninh'], ['KV.1.8', 'Tỉnh Phú Thọ']] },
            { name: 'Miền Nam', items: [['KV.2.1', 'TP Hồ Chí Minh'], ['KV.2.2', 'TP Cần Thơ'], ['KV.2.3', 'Tỉnh Đồng Nai'], ['KV.2.4', 'Tỉnh Cà Mau'], ['KV.2.5', 'Tỉnh An Giang']] },
            { name: 'Miền Trung', items: [['KV.3.1', 'TP Đà Nẵng'], ['KV.3.2', 'Tỉnh Thanh Hoá'], ['KV.3.3', 'Tỉnh Nghệ An'], ['KV.3.4', 'Tỉnh Hà Tĩnh'], ['KV.3.5', 'TP Huế'], ['KV.3.6', 'Tỉnh Quảng Ngãi']] },
            { name: 'Nước ngoài', items: [['KV.4.1', 'Châu Á']] }
        ]
    };
    var activePicker = '';
    var activeTrigger = null;
    var garages = [['001079023298', 'Gara ô tô 123'], ['0100108913', 'Công ty cổ phần dịch vụ vận tải ô tô Số 8']];

    function ensureGarageModal() {
        var modal = document.getElementById('GaragePickerModal');
        if (modal) return modal;
        modal = document.createElement('div');
        modal.id = 'GaragePickerModal';
        modal.className = 'code-picker-modal';
        modal.setAttribute('aria-hidden', 'true');
        modal.innerHTML = '<div class="code-picker-dialog garage-picker-dialog" role="dialog" aria-modal="true" aria-labelledby="GaragePickerTitle"><div class="code-picker-header"><h2 id="GaragePickerTitle">Lựa chọn Gara sửa chữa</h2><button type="button" class="code-picker-close garage-picker-close" aria-label="Đóng">×</button></div><div class="garage-picker-body"><table class="grid-table garage-picker-table"><thead><tr><th>Mã</th><th>Tên Gara</th></tr></thead><tbody id="GarageChoices"></tbody></table></div><div class="code-picker-footer"><button type="button" class="btn garage-picker-close">Đóng</button></div></div>';
        document.body.appendChild(modal);
        return modal;
    }

    function closeGaragePicker() {
        var modal = document.getElementById('GaragePickerModal');
        if (!modal) return;
        modal.className = 'code-picker-modal';
        modal.setAttribute('aria-hidden', 'true');
    }

    function openGaragePicker(trigger) {
        var modal = ensureGarageModal();
        var body = document.getElementById('GarageChoices');
        body.innerHTML = '';
        for (var i = 0; i < garages.length; i++) {
            (function (garage) {
                var row = document.createElement('tr');
                row.tabIndex = 0;
                row.innerHTML = '<td>' + garage[0] + '</td><td>' + garage[1] + '</td>';
                row.onclick = function () {
                    var form = trigger.closest('form');
                    form.querySelector('#GarageCode').value = garage[0];
                    form.querySelector('#GarageName').value = garage[1];
                    trigger.textContent = garage[0];
                    closeGaragePicker();
                };
                body.appendChild(row);
            }(garages[i]));
        }
        modal.className = 'code-picker-modal open';
        modal.setAttribute('aria-hidden', 'false');
    }

    function ensureModal() {
        var modal = document.getElementById('CodePickerModal');
        if (modal) return modal;
        modal = document.createElement('div');
        modal.id = 'CodePickerModal';
        modal.className = 'code-picker-modal';
        modal.setAttribute('aria-hidden', 'true');
        modal.innerHTML = '<div class="code-picker-dialog" role="dialog" aria-modal="true" aria-labelledby="CodePickerTitle">' +
            '<div class="code-picker-header"><h2 id="CodePickerTitle">Lựa chọn mã</h2><button type="button" class="code-picker-close" aria-label="Đóng">×</button></div>' +
            '<div class="code-picker-columns">' +
            '<div class="code-picker-column"><h3>Mã cấp 1</h3><div id="LevelOneChoices" class="code-choice-list"></div></div>' +
            '<div class="code-picker-column"><h3>Mã cấp 2</h3><div id="LevelTwoChoices" class="code-choice-list"></div></div>' +
            '</div><div class="code-picker-footer"><button type="button" id="ClearCodeChoice" class="btn btn-secondary">Xoá lựa chọn</button><button type="button" class="btn code-picker-close">Đóng</button></div></div>';
        document.body.appendChild(modal);
        return modal;
    }

    function makeChoice(text, action) {
        var button = document.createElement('button');
        button.type = 'button';
        button.className = 'code-choice';
        button.textContent = text;
        button.onclick = action;
        return button;
    }

    function closePicker() {
        var modal = document.getElementById('CodePickerModal');
        if (!modal) return;
        modal.className = 'code-picker-modal';
        modal.setAttribute('aria-hidden', 'true');
    }

    function selectCode(item) {
        var fieldId = activeTrigger.getAttribute('data-field');
        var form = activeTrigger ? activeTrigger.closest('form') : null;
        var field = form ? form.querySelector('#' + fieldId) : document.getElementById(fieldId);
        if (field) field.value = activePicker === 'area' ? item[1] : item[0];
        if (activeTrigger) activeTrigger.textContent = activePicker === 'area' ? item[1] : item[0] + ' - ' + item[1];
        closePicker();
    }

    function renderLevelTwo(group, selectedButton) {
        var levelOne = document.getElementById('LevelOneChoices');
        var levelTwo = document.getElementById('LevelTwoChoices');
        var buttons = levelOne.getElementsByTagName('button');
        levelTwo.innerHTML = '';
        for (var i = 0; i < buttons.length; i++) buttons[i].className = 'code-choice';
        selectedButton.className = 'code-choice selected';
        for (var j = 0; j < group.items.length; j++) {
            (function (item) {
                var text = activePicker === 'area' ? item[1] : item[0] + ' - ' + item[1];
                levelTwo.appendChild(makeChoice(text, function () { selectCode(item); }));
            }(group.items[j]));
        }
    }

    function openPicker(type) {
        var modal = ensureModal();
        var groups = catalogs[type];
        var levelOne = document.getElementById('LevelOneChoices');
        var levelTwo = document.getElementById('LevelTwoChoices');
        activePicker = type;
        document.getElementById('CodePickerTitle').textContent = activeTrigger.getAttribute('data-title');
        levelOne.innerHTML = '';
        levelTwo.innerHTML = '<div class="code-picker-empty">Vui lòng chọn mã cấp 1</div>';
        for (var i = 0; i < groups.length; i++) {
            (function (group) {
                var button = makeChoice(group.name, function () { renderLevelTwo(group, button); });
                levelOne.appendChild(button);
            }(groups[i]));
        }
        modal.className = 'code-picker-modal open';
        modal.setAttribute('aria-hidden', 'false');
    }

    document.addEventListener('click', function (event) {
        var garageTrigger = event.target.closest ? event.target.closest('.garage-picker-trigger') : null;
        if (garageTrigger) { event.preventDefault(); openGaragePicker(garageTrigger); return; }
        if (event.target.className.indexOf('garage-picker-close') !== -1) { closeGaragePicker(); return; }
        if (event.target.id === 'GaragePickerModal') { closeGaragePicker(); return; }
        var trigger = event.target.closest ? event.target.closest('.code-picker-trigger') : null;
        if (trigger) { event.preventDefault(); activeTrigger = trigger; openPicker(trigger.getAttribute('data-picker')); return; }
        if (event.target.className.indexOf('code-picker-close') !== -1) { closePicker(); return; }
        if (event.target.id === 'ClearCodeChoice') {
            var fieldId = activeTrigger.getAttribute('data-field');
            var form = activeTrigger ? activeTrigger.closest('form') : null;
            var field = form ? form.querySelector('#' + fieldId) : document.getElementById(fieldId);
            var display = activeTrigger;
            if (field) field.value = '';
            if (display) display.textContent = activeTrigger.getAttribute('data-placeholder');
            closePicker();
            return;
        }
        if (event.target.id === 'CodePickerModal') closePicker();
    });
    document.addEventListener('keydown', function (event) { if (event.key === 'Escape' || event.keyCode === 27) { closePicker(); closeGaragePicker(); } });
}());
