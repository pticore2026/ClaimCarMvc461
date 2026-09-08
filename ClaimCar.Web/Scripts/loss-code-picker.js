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
    var activeBeneficiaryTable = null;
    var coverages = [
        ['XO.1.1.1', 'Bảo hiểm bắt buộc TNDS của chủ xe đối với NT3 về người'],
        ['XO.1.1.2', 'Bảo hiểm bắt buộc TNDS của chủ xe với NT3 về tài sản'],
        ['XO.1.1.3', 'Bảo hiểm bắt buộc TNDS của chủ xe đối với hành khách trên xe'],
        ['XO.3.1', 'Bảo hiểm tai nạn lái xe'],
        ['XO.3.2', 'Bảo hiểm tai nạn phụ xe'],
        ['XO.3.4', 'Bảo hiểm người ngồi trên xe'],
        ['XO.4.1.1', 'Bảo hiểm vật chất toàn bộ xe ô tô - phí cơ bản']
    ];
    var activeCoverageTable = null;

    function ensureBeneficiaryModal() {
        var modal = document.getElementById('BeneficiaryPickerModal');
        if (modal) return modal;
        modal = document.createElement('div');
        modal.id = 'BeneficiaryPickerModal';
        modal.className = 'code-picker-modal';
        modal.setAttribute('aria-hidden', 'true');
        modal.innerHTML = '<div class="code-picker-dialog" role="dialog" aria-modal="true" aria-labelledby="BeneficiaryPickerTitle"><div class="code-picker-header"><h2 id="BeneficiaryPickerTitle">Lựa chọn đối tượng hưởng khác</h2><button type="button" class="code-picker-close beneficiary-picker-close" aria-label="Đóng">×</button></div><div class="coverage-picker-body"><table class="grid-table beneficiary-picker-table"><thead><tr><th>Mã</th><th>Tên</th></tr></thead><tbody id="BeneficiaryChoices"></tbody></table></div><div class="code-picker-footer"><button type="button" class="btn beneficiary-picker-close">Đóng</button></div></div>';
        document.body.appendChild(modal);
        return modal;
    }

    function closeBeneficiaryPicker() {
        var modal = document.getElementById('BeneficiaryPickerModal');
        if (!modal) return;
        modal.className = 'code-picker-modal';
        modal.setAttribute('aria-hidden', 'true');
    }

    function addBeneficiary(garage) {
        if (!activeBeneficiaryTable) return;
        var body = activeBeneficiaryTable.querySelector('.beneficiary-rows');
        var codes = body.querySelectorAll('input[name$=".Code"]');
        for (var i = 0; i < codes.length; i++) {
            if (codes[i].value === garage[0]) {
                if (window.ClaimCarNotification) ClaimCarNotification.warning('Mã Gara ' + garage[0] + ' đã được chọn.');
                return;
            }
        }
        var index = body.querySelectorAll('tr').length;
        var prefix = 'OtherBeneficiaries[' + index + ']';
        var row = document.createElement('tr');
        row.innerHTML = '<td><input type="hidden" name="' + prefix + '.Id" value="0"><input class="form-control" name="' + prefix + '.Code" readonly></td><td><input class="form-control" name="' + prefix + '.Name" readonly></td><td><input class="form-control" name="' + prefix + '.Currency" value="VND"></td><td><input class="form-control right" type="number" min="0" name="' + prefix + '.Amount" value="0"></td>';
        row.querySelector('input[name$=".Code"]').value = garage[0];
        row.querySelector('input[name$=".Name"]').value = garage[1];
        body.appendChild(row);
        closeBeneficiaryPicker();
    }

    function addEmptyBeneficiary(trigger) {
        var table = trigger.closest('table');
        if (!table) return;
        var body = table.querySelector('.beneficiary-rows');
        var emptyRow = body.querySelector('.beneficiary-empty');
        if (emptyRow) body.removeChild(emptyRow);
        var index = body.querySelectorAll('tr').length;
        var prefix = 'OtherBeneficiaries[' + index + ']';
        var row = document.createElement('tr');
        row.innerHTML = '<td class="beneficiary-action-column"><button type="button" class="beneficiary-delete" aria-label="Xóa đối tượng hưởng khác" title="Xóa">🗑</button></td><td><input type="hidden" name="' + prefix + '.Id" value="0"><input class="form-control" name="' + prefix + '.Code"></td><td><input class="form-control" name="' + prefix + '.Name"></td><td><input class="form-control" name="' + prefix + '.Currency" value="VND"></td><td><input class="form-control right" type="number" min="0" name="' + prefix + '.Amount" value="0"></td>';
        body.appendChild(row);
        row.querySelector('input[name$=".Code"]').focus();
    }

    function deleteBeneficiary(button) {
        var body = button.closest('.beneficiary-rows');
        body.removeChild(button.closest('tr'));
        var rows = body.querySelectorAll('tr:not(.beneficiary-empty)');
        for (var i = 0; i < rows.length; i++) {
            var fields = rows[i].querySelectorAll('[name]');
            for (var j = 0; j < fields.length; j++) fields[j].name = fields[j].name.replace(/^OtherBeneficiaries\[\d+\]/, 'OtherBeneficiaries[' + i + ']');
        }
        if (!rows.length) body.innerHTML = '<tr class="beneficiary-empty"><td colspan="5" class="muted center">Chưa có dữ liệu</td></tr>';
    }

    function addEmptyThirdParty(trigger) {
        var table = trigger.closest('table');
        if (!table) return;
        var body = table.querySelector('.third-party-rows');
        var emptyRow = body.querySelector('.third-party-empty');
        if (emptyRow) body.removeChild(emptyRow);
        var index = body.querySelectorAll('tr').length;
        var prefix = 'ThirdParties[' + index + ']';
        var row = document.createElement('tr');
        row.innerHTML = '<td class="third-party-add-column"><button type="button" class="third-party-delete" aria-label="Xóa người thứ ba" title="Xóa">🗑</button></td><td><input class="form-control" name="' + prefix + '.Name"></td><td><input class="form-control" name="' + prefix + '.Currency" value="VND"></td><td><input class="form-control right" type="number" min="0" name="' + prefix + '.Amount" value="0"></td>';
        body.appendChild(row);
        row.querySelector('input[name$=".Name"]').focus();
    }

    function deleteThirdParty(button) {
        var body = button.closest('.third-party-rows');
        body.removeChild(button.closest('tr'));
        var rows = body.querySelectorAll('tr:not(.third-party-empty)');
        for (var i = 0; i < rows.length; i++) {
            var fields = rows[i].querySelectorAll('[name]');
            for (var j = 0; j < fields.length; j++) fields[j].name = fields[j].name.replace(/^ThirdParties\[\d+\]/, 'ThirdParties[' + i + ']');
        }
        if (!rows.length) body.innerHTML = '<tr class="third-party-empty"><td colspan="4" class="muted center">Chưa có dữ liệu</td></tr>';
    }

    function openBeneficiaryPicker(trigger) {
        activeBeneficiaryTable = trigger.closest('table');
        var modal = ensureBeneficiaryModal();
        var body = document.getElementById('BeneficiaryChoices');
        body.innerHTML = '';
        for (var i = 0; i < garages.length; i++) {
            (function (garage) {
                var row = document.createElement('tr');
                row.tabIndex = 0;
                row.innerHTML = '<td></td><td></td>';
                row.cells[0].textContent = garage[0];
                row.cells[1].textContent = garage[1];
                row.onclick = function () { addBeneficiary(garage); };
                row.onkeydown = function (event) { if (event.key === 'Enter' || event.key === ' ') { event.preventDefault(); addBeneficiary(garage); } };
                body.appendChild(row);
            }(garages[i]));
        }
        modal.className = 'code-picker-modal open';
        modal.setAttribute('aria-hidden', 'false');
    }

    function ensureCoverageModal() {
        var modal = document.getElementById('CoveragePickerModal');
        if (modal) return modal;
        modal = document.createElement('div');
        modal.id = 'CoveragePickerModal';
        modal.className = 'code-picker-modal';
        modal.setAttribute('aria-hidden', 'true');
        modal.innerHTML = '<div class="code-picker-dialog coverage-picker-dialog" role="dialog" aria-modal="true" aria-labelledby="CoveragePickerTitle"><div class="code-picker-header"><h2 id="CoveragePickerTitle">Lựa chọn loại hình</h2><button type="button" class="code-picker-close coverage-picker-close" aria-label="Đóng">×</button></div><div class="coverage-picker-body"><table class="grid-table coverage-picker-table"><thead><tr><th>Mã</th><th>Tên</th></tr></thead><tbody id="CoverageChoices"></tbody></table></div><div class="code-picker-footer"><button type="button" class="btn coverage-picker-close">Đóng</button></div></div>';
        document.body.appendChild(modal);
        return modal;
    }

    function closeCoveragePicker() {
        var modal = document.getElementById('CoveragePickerModal');
        if (!modal) return;
        modal.className = 'code-picker-modal';
        modal.setAttribute('aria-hidden', 'true');
    }

    function addCoverage(code) {
        if (!activeCoverageTable) return;
        var body = activeCoverageTable.querySelector('.coverage-rows');
        var existing = body.querySelectorAll('input[name$=".CoverageCode"]');
        for (var i = 0; i < existing.length; i++) {
            if (existing[i].value === code) {
                if (window.ClaimCarNotification) ClaimCarNotification.warning('Mã loại hình ' + code + ' đã được chọn.');
                return;
            }
        }
        var index = body.querySelectorAll('tr').length;
        var prefix = 'Coverages[' + index + ']';
        var row = document.createElement('tr');
        row.innerHTML = '<td><input type="hidden" name="' + prefix + '.Id" value="0"><span class="coverage-code"></span><input type="hidden" name="' + prefix + '.CoverageCode"></td>' +
            '<td><input class="form-control coverage-line-input" name="' + prefix + '.Currency" value="VND" readonly></td>' +
            '<td><input class="form-control right coverage-line-input" type="number" name="' + prefix + '.InsuranceAmount" value="0" readonly></td>' +
            '<td><input class="form-control right coverage-line-input" type="number" step="0.01" min="0" max="100" name="' + prefix + '.LossPercent" value="0" readonly></td>' +
            '<td><input class="form-control right coverage-line-input" type="number" name="' + prefix + '.LossAmount" value="0" readonly></td>' +
            '<td><input class="form-control right coverage-line-input" type="number" name="' + prefix + '.Deductible" value="0" readonly></td>' +
            '<td><input class="form-control right coverage-line-input" type="number" name="' + prefix + '.CompensationAmount" value="0" readonly></td>' +
            '<td><input class="form-control right coverage-line-input" type="number" name="' + prefix + '.TaxAmount" value="0" readonly></td>' +
            '<td class="coverage-actions"><button type="button" class="coverage-edit" aria-label="Chỉnh sửa" title="Chỉnh sửa">✎</button><button type="button" class="coverage-delete" aria-label="Xóa" title="Xóa">🗑</button></td>';
        row.querySelector('.coverage-code').textContent = code;
        row.querySelector('input[name$=".CoverageCode"]').value = code;
        body.appendChild(row);
        closeCoveragePicker();
    }

    function reindexCoverages(body) {
        var rows = body.querySelectorAll('tr');
        for (var i = 0; i < rows.length; i++) {
            var fields = rows[i].querySelectorAll('[name]');
            for (var j = 0; j < fields.length; j++) fields[j].name = fields[j].name.replace(/^Coverages\[\d+\]/, 'Coverages[' + i + ']');
        }
    }

    function toggleCoverageEdit(button) {
        var row = button.closest('tr');
        var fields = row.querySelectorAll('.coverage-line-input');
        var editing = row.className.indexOf('coverage-editing') !== -1;
        for (var i = 0; i < fields.length; i++) fields[i].readOnly = editing;
        row.classList.toggle('coverage-editing', !editing);
        button.textContent = editing ? '✎' : '✓';
        button.title = editing ? 'Chỉnh sửa' : 'Hoàn tất chỉnh sửa';
        button.setAttribute('aria-label', button.title);
        if (!editing && fields.length) fields[0].focus();
    }

    function deleteCoverage(button) {
        var row = button.closest('tr');
        var code = row.querySelector('.coverage-code').textContent;
        if (!window.confirm('Bạn có chắc muốn xóa mã loại hình ' + code + '?')) return;
        var body = row.parentNode;
        body.removeChild(row);
        reindexCoverages(body);
    }

    function openCoveragePicker(trigger) {
        activeCoverageTable = trigger.closest('table');
        var modal = ensureCoverageModal();
        var body = document.getElementById('CoverageChoices');
        body.innerHTML = '';
        for (var i = 0; i < coverages.length; i++) {
            (function (coverage) {
                var row = document.createElement('tr');
                row.tabIndex = 0;
                row.innerHTML = '<td></td><td></td>';
                row.cells[0].textContent = coverage[0];
                row.cells[1].textContent = coverage[1];
                row.onclick = function () { addCoverage(coverage[0]); };
                row.onkeydown = function (event) { if (event.key === 'Enter' || event.key === ' ') { event.preventDefault(); addCoverage(coverage[0]); } };
                body.appendChild(row);
            }(coverages[i]));
        }
        modal.className = 'code-picker-modal open';
        modal.setAttribute('aria-hidden', 'false');
    }

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
        var beneficiaryTrigger = event.target.closest ? event.target.closest('.beneficiary-add-trigger') : null;
        if (beneficiaryTrigger) { event.preventDefault(); addEmptyBeneficiary(beneficiaryTrigger); return; }
        var beneficiaryDelete = event.target.closest ? event.target.closest('.beneficiary-delete') : null;
        if (beneficiaryDelete) { event.preventDefault(); deleteBeneficiary(beneficiaryDelete); return; }
        var thirdPartyTrigger = event.target.closest ? event.target.closest('.third-party-add-trigger') : null;
        if (thirdPartyTrigger) { event.preventDefault(); addEmptyThirdParty(thirdPartyTrigger); return; }
        var thirdPartyDelete = event.target.closest ? event.target.closest('.third-party-delete') : null;
        if (thirdPartyDelete) { event.preventDefault(); deleteThirdParty(thirdPartyDelete); return; }
        if (event.target.className.indexOf('beneficiary-picker-close') !== -1) { closeBeneficiaryPicker(); return; }
        if (event.target.id === 'BeneficiaryPickerModal') { closeBeneficiaryPicker(); return; }
        var editCoverage = event.target.closest ? event.target.closest('.coverage-edit') : null;
        if (editCoverage) { event.preventDefault(); toggleCoverageEdit(editCoverage); return; }
        var deleteCoverageButton = event.target.closest ? event.target.closest('.coverage-delete') : null;
        if (deleteCoverageButton) { event.preventDefault(); deleteCoverage(deleteCoverageButton); return; }
        var coverageTrigger = event.target.closest ? event.target.closest('.coverage-add-trigger') : null;
        if (coverageTrigger) { event.preventDefault(); openCoveragePicker(coverageTrigger); return; }
        if (event.target.className.indexOf('coverage-picker-close') !== -1) { closeCoveragePicker(); return; }
        if (event.target.id === 'CoveragePickerModal') { closeCoveragePicker(); return; }
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
    document.addEventListener('keydown', function (event) { if (event.key === 'Escape' || event.keyCode === 27) { closePicker(); closeGaragePicker(); closeCoveragePicker(); closeBeneficiaryPicker(); } });
}());
