(function () {
    var parts = [
        ['Toyota', 'Vios', 'RL4BT9F33R0000121', 'Điện - điều hoà', 'PT.DDH.37', 'Camera trước'],
        ['Toyota', 'Vios', 'RL4BT9F33R0000121', 'Điện - điều hoà', 'PT.DDH.7', 'Bình ắc quy'],
        ['Toyota', 'Fortuner', 'MHFJW8GS5R0000248', 'Điện - điều hoà', 'PT.DDH.95', 'Ga điều hòa'],
        ['Ford', 'Ranger', 'MNBUMFF50RW123456', 'Thân vỏ', 'PT.THV', 'Thân vỏ'],
        ['Ford', 'Ranger', 'MNBUMFF50RW123456', 'Động cơ', 'PT.DC', 'Động cơ'],
        ['Honda', 'City', 'MRHGM6620RP001357', 'Động cơ', 'PT.DC.23', 'Bugi'],
        ['Honda', 'City', 'MRHGM6620RP001357', 'Động cơ', 'PT.DC.17', 'Bobin đánh lửa'],
        ['Hyundai', 'Accent', 'KMHCT41DBRU012845', 'Động cơ', 'PT.DC.50', 'Dầu động cơ'],
        ['Hyundai', 'Accent', 'KMHCT41DBRU012845', 'Động cơ', 'PT.DC.67', 'Hộp lọc gió'],
        ['Mazda', 'CX-5', 'JM7KF4WLA00124680', 'Cơ cấu chuyên dùng khác', 'PT.DDH.1', 'Âm ly'],
        ['Mazda', 'CX-5', 'JM7KF4WLA00124680', 'Điện - điều hoà', 'PT.DDH.107', 'Hộp dàn lạnh điều hòa'],
        ['Mazda', 'CX-5', 'JM7KF4WLA00124680', 'Truyền lực - gầm', 'PT.DDH.108', 'Hộp điều khiển truyền lực']
    ];
    var activeTable = null;
    var activeRow = null;
    var selectedPart = null;
    var currentPage = 1;
    var pageSize = 10;
    var damages = [
        ['TH.01', 'Trầy xước'],
        ['TH.02', 'Móp, biến dạng'],
        ['TH.03', 'Nứt'],
        ['TH.04', 'Vỡ'],
        ['TH.05', 'Cong, lệch'],
        ['TH.06', 'Hư hỏng cơ khí'],
        ['TH.07', 'Hư hỏng điện'],
        ['TH.08', 'Mất chức năng hoạt động']
    ];

    function ensureModal() {
        var modal = document.getElementById('QuotePartPickerModal');
        if (modal) return modal;
        modal = document.createElement('div');
        modal.id = 'QuotePartPickerModal';
        modal.className = 'code-picker-modal';
        modal.setAttribute('aria-hidden', 'true');
        modal.innerHTML = '<div class="code-picker-dialog part-picker-dialog" role="dialog" aria-modal="true" aria-labelledby="QuotePartPickerTitle"><div class="code-picker-header"><h2 id="QuotePartPickerTitle">Danh sách thiết bị - phụ tùng xe</h2><button type="button" class="code-picker-close quote-part-picker-close" aria-label="Đóng">×</button></div><div id="PartPickerListView"><div class="part-picker-vehicle"><label>Hãng xe<input id="PartVehicleBrand" class="form-control readonly" readonly></label><label>Phiên bản xe<input id="PartVehicleVersion" class="form-control readonly" readonly></label><label>Số loại<input id="PartVehicleTypeNumber" class="form-control readonly" readonly></label></div><div class="part-picker-search"><select id="PartTypeFilter" class="form-control"><option value="">-- Nhóm phụ tùng --</option><option>Điện - điều hoà</option><option>Cơ cấu chuyên dùng khác</option><option>Thân vỏ</option><option>Động cơ</option><option>Truyền lực - gầm</option></select><select id="PartCodeFilter" class="form-control"><option value="">-- Mã phụ tùng --</option></select><input id="PartNameFilter" class="form-control" placeholder="Tên phụ tùng"><button type="button" id="PartSearchButton" class="btn">Tìm kiếm</button></div><div class="part-picker-body"><table class="grid-table part-picker-table"><thead><tr><th>Mã phụ tùng</th><th>Tên phụ tùng</th></tr></thead><tbody id="QuotePartChoices"></tbody></table><div id="PartPickerEmpty" class="code-picker-empty" style="display:none">Không tìm thấy phụ tùng phù hợp.</div><div id="PartPickerPagination" class="part-picker-pagination"></div></div></div><div id="PartPickerDetailView" class="part-picker-detail" style="display:none"><div class="part-detail-grid"><div class="form-group"><label>Mã phụ tùng</label><input id="SelectedPartCode" class="form-control readonly" readonly></div><div class="form-group"><label>Tên thiết bị</label><input id="SelectedPartName" class="form-control readonly" readonly></div><div class="form-group"><label>Số lượng</label><input id="SelectedPartQuantity" class="form-control right" type="number" min="1" value="1"></div><div class="form-group"><label>Thiệt hại</label><input id="SelectedPartDamage" type="hidden"><button type="button" id="SelectedPartDamageDisplay" class="form-control code-picker-trigger">-- Chọn thiệt hại --</button></div><div class="form-group"><label>Kích thước</label><input id="SelectedPartDimensions" class="form-control"></div><div class="form-group"><label>Phương án</label><select id="SelectedPartProposal" class="form-control"><option value="">-- Chọn phương án --</option><option>Thay thế có thu hồi</option><option>Thay thế không thu hồi</option><option>Thay thế đặc biệt có thu hồi</option><option>Thay thế đặc biệt không thu hồi</option><option>Sửa chữa</option></select></div><div class="form-group"><label>Loại phụ tùng</label><select id="SelectedPartType" class="form-control"><option value="">-- Chọn loại --</option><option>Chính hãng</option><option>Không chính hãng</option><option>Đã qua sử dụng</option></select></div><div class="form-group"><label>Đơn giá</label><input id="SelectedPartPrice" class="form-control right" type="number" min="0" value="0"></div><div class="form-group"><label>Thành tiền</label><input id="SelectedPartAmount" class="form-control readonly right" type="number" readonly value="0"></div><div class="form-group"><label>Sơn</label><input id="SelectedPartPaint" class="form-control right" type="number" min="0" value="0"></div><div class="form-group"><label>Công</label><input id="SelectedPartLabor" class="form-control right" type="number" min="0" value="0"></div></div><div class="part-picker-detail-actions"><button type="button" id="PartDetailBack" class="btn btn-secondary">Quay lại</button><button type="button" id="PartDetailConfirm" class="btn">Thêm phụ tùng</button></div></div><div class="code-picker-footer"><button type="button" class="btn quote-part-picker-close">Đóng</button></div></div>';
        document.body.appendChild(modal);
        document.getElementById('SelectedPartType').innerHTML = '<option value="">-- Chọn loại --</option><option>Chính hãng</option><option>Hàng zin(X.xứ.Hãng)</option><option>Loại 1(Xịn,OEM)</option><option>Loại 2(Liên doanh)</option>';
        document.getElementById('PartSearchButton').onclick = function () { renderParts(true); };
        document.getElementById('PartTypeFilter').onchange = function () { populatePartCodes(); renderParts(true); };
        document.getElementById('PartCodeFilter').onchange = function () { renderParts(true); };
        document.getElementById('PartDetailBack').onclick = showPartList;
        document.getElementById('PartDetailConfirm').onclick = function () {
            if (activeRow) updatePart(activeRow);
            else if (selectedPart) addPart(selectedPart);
        };
        document.getElementById('SelectedPartQuantity').oninput = calculatePartAmount;
        document.getElementById('SelectedPartPrice').oninput = calculatePartAmount;
        document.getElementById('SelectedPartPaint').oninput = calculatePartAmount;
        document.getElementById('SelectedPartLabor').oninput = calculatePartAmount;
        document.getElementById('SelectedPartDamageDisplay').onclick = openDamagePicker;
        var filters = modal.querySelectorAll('.part-picker-search input');
        for (var i = 0; i < filters.length; i++) filters[i].onkeydown = function (event) { if (event.key === 'Enter' || event.keyCode === 13) { event.preventDefault(); renderParts(true); } };
        return modal;
    }

    function closePicker() {
        var modal = document.getElementById('QuotePartPickerModal');
        if (!modal) return;
        modal.className = 'code-picker-modal';
        modal.setAttribute('aria-hidden', 'true');
        activeRow = null;
        selectedPart = null;
    }

    function addPart(part) {
        if (!activeTable) return;
        var body = activeTable.querySelector('.quote-part-rows');
        var codes = body.querySelectorAll('input[name$=".PartCode"]');
        for (var i = 0; i < codes.length; i++) {
            var existingName = codes[i].parentNode.querySelector('input[name$=".PartName"]');
            if (codes[i].value === part[4] && existingName && existingName.value === part[5]) {
                if (window.ClaimCarNotification) ClaimCarNotification.warning('Phụ tùng ' + part[5] + ' đã được chọn.');
                return;
            }
        }
        var index = body.querySelectorAll('tr').length;
        var prefix = 'Items[' + index + ']';
        var row = document.createElement('tr');
        row.innerHTML = '<td><input type="hidden" name="' + prefix + '.Id" value="0"><input type="hidden" name="' + prefix + '.PartCode"><input class="form-control" name="' + prefix + '.PartName" readonly></td>' +
            '<td><input class="form-control right" type="number" min="0" name="' + prefix + '.Quantity"></td>' +
            '<td><input type="hidden" name="' + prefix + '.Damage"><input type="hidden" name="' + prefix + '.Dimensions"><select class="form-control" name="' + prefix + '.Proposal"><option value="">-- Chọn phương án --</option><option>Thay thế có thu hồi</option><option>Thay thế không thu hồi</option><option>Thay thế đặc biệt có thu hồi</option><option>Thay thế đặc biệt không thu hồi</option><option>Sửa chữa</option></select></td>' +
            '<td><select class="form-control" name="' + prefix + '.PartType"><option value="">-- Chọn loại --</option><option>Chính hãng</option><option>Hàng zin(X.xứ.Hãng)</option><option>Loại 1(Xịn,OEM)</option><option>Loại 2(Liên doanh)</option></select></td>' +
            '<td><input class="form-control right" type="number" min="0" name="' + prefix + '.PartPrice" value="0"></td>' +
            '<td><input class="form-control right" type="number" min="0" name="' + prefix + '.PaintCost" value="0"></td>' +
            '<td><input class="form-control right" type="number" min="0" name="' + prefix + '.LaborCost" value="0"></td>' +
            '<td class="num">0</td>' +
            '<td>' + actionMenuMarkup() + '</td>';
        row.querySelector('input[name$=".PartCode"]').value = part[4];
        row.querySelector('input[name$=".PartName"]').value = part[5];
        row.querySelector('input[name$=".Quantity"]').value = document.getElementById('SelectedPartQuantity').value || 1;
        row.querySelector('input[name$=".Damage"]').value = document.getElementById('SelectedPartDamage').value;
        row.querySelector('input[name$=".Dimensions"]').value = document.getElementById('SelectedPartDimensions').value;
        row.querySelector('select[name$=".Proposal"]').value = document.getElementById('SelectedPartProposal').value;
        row.querySelector('select[name$=".PartType"]').value = document.getElementById('SelectedPartType').value;
        row.querySelector('input[name$=".PartPrice"]').value = document.getElementById('SelectedPartPrice').value || 0;
        row.querySelector('input[name$=".PaintCost"]').value = document.getElementById('SelectedPartPaint').value || 0;
        row.querySelector('input[name$=".LaborCost"]').value = document.getElementById('SelectedPartLabor').value || 0;
        calculateRowTotal(row);
        body.appendChild(row);
        lockRow(row);
        recalculatePartCosts(activeTable);
        closePicker();
    }

    function actionMenuMarkup() {
        return '<div class="quote-part-actions"><button type="button" class="quote-part-edit" aria-label="Sửa phụ tùng" title="Sửa">✎</button><button type="button" class="quote-part-delete" aria-label="Xóa phụ tùng" title="Xóa">🗑</button></div>';
    }

    function lockRow(row) {
        var inputs = row.querySelectorAll('input:not([type="hidden"])');
        for (var i = 0; i < inputs.length; i++) {
            inputs[i].readOnly = true;
            inputs[i].className += inputs[i].className.indexOf('readonly') === -1 ? ' readonly' : '';
            inputs[i].tabIndex = -1;
        }
        var selects = row.querySelectorAll('select');
        for (var selectIndex = 0; selectIndex < selects.length; selectIndex++) {
            selects[selectIndex].disabled = true;
            selects[selectIndex].className += selects[selectIndex].className.indexOf('readonly') === -1 ? ' readonly' : '';
        }
    }

    function initializeGrid() {
        var rows = document.querySelectorAll('.quote-parts-table .quote-part-rows tr');
        for (var i = 0; i < rows.length; i++) {
            var oldEdit = rows[i].querySelector('.quote-part-edit');
            if (oldEdit && !rows[i].querySelector('.quote-part-actions')) oldEdit.parentNode.innerHTML = actionMenuMarkup();
            lockRow(rows[i]);
        }
        var tables = document.querySelectorAll('.quote-parts-table');
        for (var tableIndex = 0; tableIndex < tables.length; tableIndex++) recalculatePartCosts(tables[tableIndex]);
    }

    function closeActionMenus(except) {
        var menus = document.querySelectorAll('.quote-part-actions.open');
        for (var i = 0; i < menus.length; i++) {
            if (menus[i] === except) continue;
            menus[i].classList.remove('open');
            menus[i].querySelector('.quote-part-actions-toggle').setAttribute('aria-expanded', 'false');
        }
    }

    function updatePart(row) {
        row.querySelector('input[name$=".PartName"]').value = document.getElementById('SelectedPartName').value;
        row.querySelector('input[name$=".Quantity"]').value = document.getElementById('SelectedPartQuantity').value || 1;
        row.querySelector('input[name$=".Damage"]').value = document.getElementById('SelectedPartDamage').value;
        row.querySelector('input[name$=".Dimensions"]').value = document.getElementById('SelectedPartDimensions').value;
        row.querySelector('select[name$=".Proposal"]').value = document.getElementById('SelectedPartProposal').value;
        row.querySelector('select[name$=".PartType"]').value = document.getElementById('SelectedPartType').value;
        row.querySelector('input[name$=".PartPrice"]').value = document.getElementById('SelectedPartPrice').value || 0;
        row.querySelector('input[name$=".PaintCost"]').value = document.getElementById('SelectedPartPaint').value || 0;
        row.querySelector('input[name$=".LaborCost"]').value = document.getElementById('SelectedPartLabor').value || 0;
        calculateRowTotal(row);
        recalculatePartCosts(row.closest('.quote-parts-table'));
        closePicker();
    }

    function openPartEditor(row) {
        activeRow = row;
        activeTable = row.closest('.quote-parts-table');
        ensureModal();
        document.getElementById('SelectedPartCode').value = row.querySelector('input[name$=".PartCode"]').value;
        document.getElementById('SelectedPartName').value = row.querySelector('input[name$=".PartName"]').value;
        document.getElementById('SelectedPartQuantity').value = row.querySelector('input[name$=".Quantity"]').value;
        document.getElementById('SelectedPartDamage').value = row.querySelector('input[name$=".Damage"]').value;
        document.getElementById('SelectedPartDamageDisplay').textContent = row.querySelector('input[name$=".Damage"]').value || '-- Chọn thiệt hại --';
        document.getElementById('SelectedPartDimensions').value = row.querySelector('input[name$=".Dimensions"]').value;
        document.getElementById('SelectedPartProposal').value = row.querySelector('select[name$=".Proposal"]').value;
        document.getElementById('SelectedPartType').value = row.querySelector('select[name$=".PartType"]').value;
        document.getElementById('SelectedPartPrice').value = row.querySelector('input[name$=".PartPrice"]').value;
        document.getElementById('SelectedPartPaint').value = row.querySelector('input[name$=".PaintCost"]').value;
        document.getElementById('SelectedPartLabor').value = row.querySelector('input[name$=".LaborCost"]').value;
        calculatePartAmount();
        document.getElementById('PartPickerListView').style.display = 'none';
        document.getElementById('PartPickerDetailView').style.display = 'block';
        document.getElementById('QuotePartPickerTitle').textContent = 'Sửa thông tin phụ tùng';
        document.getElementById('PartDetailConfirm').textContent = 'Lưu thay đổi';
        document.getElementById('QuotePartPickerModal').className = 'code-picker-modal open';
        document.getElementById('QuotePartPickerModal').setAttribute('aria-hidden', 'false');
    }

    function reindexRows(table) {
        var rows = table.querySelectorAll('.quote-part-rows tr');
        for (var index = 0; index < rows.length; index++) {
            var fields = rows[index].querySelectorAll('[name^="Items["]');
            for (var fieldIndex = 0; fieldIndex < fields.length; fieldIndex++) {
                fields[fieldIndex].name = fields[fieldIndex].name.replace(/^Items\[\d+\]/, 'Items[' + index + ']');
                if (fields[fieldIndex].id) fields[fieldIndex].id = fields[fieldIndex].id.replace(/^Items_\d+__/, 'Items_' + index + '__');
            }
        }
    }

    function normalized(value) { return (value || '').toLocaleLowerCase('vi').trim(); }

    function calculatePartAmount() {
        var quantity = parseFloat(document.getElementById('SelectedPartQuantity').value) || 0;
        var price = parseFloat(document.getElementById('SelectedPartPrice').value) || 0;
        document.getElementById('SelectedPartAmount').value = quantity * price;
    }

    function calculateRowTotal(row) {
        var quantity = parseFloat(row.querySelector('input[name$=".Quantity"]').value) || 0;
        var price = parseFloat(row.querySelector('input[name$=".PartPrice"]').value) || 0;
        var paint = parseFloat(row.querySelector('input[name$=".PaintCost"]').value) || 0;
        var labor = parseFloat(row.querySelector('input[name$=".LaborCost"]').value) || 0;
        row.querySelector('.num').textContent = ((quantity * price) + paint + labor).toLocaleString('vi-VN');
    }

    function recalculatePartCosts(table) {
        if (!table) return;
        var card = table.closest('.card');
        var totals = { ReplacementTotal: 0, SpecialReplacementTotal: 0, RepairTotal: 0, PaintTotal: 0, LaborTotal: 0 };
        var rows = table.querySelectorAll('.quote-part-rows tr');
        for (var i = 0; i < rows.length; i++) {
            var proposal = rows[i].querySelector('select[name$=".Proposal"]');
            var quantity = parseFloat(rows[i].querySelector('input[name$=".Quantity"]').value) || 0;
            var price = (parseFloat(rows[i].querySelector('input[name$=".PartPrice"]').value) || 0) * quantity;
            var paint = parseFloat(rows[i].querySelector('input[name$=".PaintCost"]').value) || 0;
            var labor = parseFloat(rows[i].querySelector('input[name$=".LaborCost"]').value) || 0;
            if (proposal && (proposal.value === 'Thay thế có thu hồi' || proposal.value === 'Thay thế không thu hồi')) totals.ReplacementTotal += price;
            if (proposal && (proposal.value === 'Thay thế đặc biệt có thu hồi' || proposal.value === 'Thay thế đặc biệt không thu hồi')) totals.SpecialReplacementTotal += price;
            if (proposal && proposal.value === 'Sửa chữa') totals.RepairTotal += price;
            totals.PaintTotal += paint;
            totals.LaborTotal += labor;
        }
        function value(name) { var input = card.querySelector('[name="' + name + '"]'); return input ? (parseFloat(input.value) || 0) : 0; }
        function setTotal(name, total, discountName) {
            var input = card.querySelector('[name="' + name + '"]');
            if (input) input.value = total;
            var amount = input && input.closest('.money-block') ? input.closest('.money-block').querySelector('.money-block-amount') : null;
            var discounted = total * (100 - value(discountName)) / 100;
            if (amount) amount.value = discounted.toLocaleString('vi-VN');
            return discounted;
        }
        var replacementAmount = setTotal('ReplacementTotal', totals.ReplacementTotal, 'ReplacementDiscountPercent');
        var specialReplacementAmount = setTotal('SpecialReplacementTotal', totals.SpecialReplacementTotal, 'SpecialReplacementDiscountPercent');
        var repairAmount = setTotal('RepairTotal', totals.RepairTotal, 'RepairDiscountPercent');
        var paintAmount = setTotal('PaintTotal', totals.PaintTotal, 'PaintDiscountPercent');
        var laborAmount = setTotal('LaborTotal', totals.LaborTotal, 'LaborDiscountPercent');
        var towingAmount = setTotal('TowingTotal', value('TowingTotal'), 'TowingDiscountPercent');
        var repairCost = card.querySelector('#RepairCostTotal');
        var replacementCost = card.querySelector('#ReplacementCostTotal');
        var depreciationCost = card.querySelector('#DepreciationCostTotal');
        var towingCost = card.querySelector('#TowingCostTotal');
        if (repairCost) repairCost.value = (repairAmount + paintAmount + laborAmount).toLocaleString('vi-VN');
        if (replacementCost) replacementCost.value = (replacementAmount + specialReplacementAmount).toLocaleString('vi-VN');
        if (depreciationCost) depreciationCost.value = ((replacementAmount * value('ReplacementDepreciationPercent') / 100) + (specialReplacementAmount * value('SpecialDepreciationPercent') / 100)).toLocaleString('vi-VN');
        if (towingCost) towingCost.value = towingAmount.toLocaleString('vi-VN');
        recalculateCompensation(card, repairAmount + paintAmount + laborAmount, replacementAmount, specialReplacementAmount, towingAmount);
    }

    function recalculateCompensation(card, repairCost, replacementAmount, specialReplacementAmount, towingCost) {
        function value(selector) { var input = card.querySelector(selector); return input ? (parseFloat(input.value) || 0) : 0; }
        var actualValue = value('[name="ActualValue"]');
        var insuredAmount = value('#PolicyInsuredAmount');
        var policyVehicleValue = value('#PolicyVehicleValue');
        var participationValueInput = card.querySelector('[name="ParticipationValuePercent"]');
        if (participationValueInput) participationValueInput.value = policyVehicleValue > 0 ? (insuredAmount * 100 / policyVehicleValue).toFixed(2) : 0;
        var deductible = Math.max(0, value('[name="DeductibleAmount"]'));
        if (actualValue <= 0 || insuredAmount <= 0) {
            var emptyResult = card.querySelector('[name="ApprovedTotal"]');
            if (emptyResult) emptyResult.value = 0;
            return;
        }
        var depreciation = (replacementAmount * value('[name="ReplacementDepreciationPercent"]') / 100)
            + (specialReplacementAmount * value('[name="SpecialDepreciationPercent"]') / 100);
        var coveredAmount = Math.max(0, (repairCost + replacementAmount + specialReplacementAmount - depreciation + towingCost) * insuredAmount / actualValue);
        var afterDeductible = Math.max(0, coveredAmount - deductible);
        var reductionPercentInput = card.querySelector('[name="CompensationReductionPercent"]');
        var reductionAmountInput = card.querySelector('[name="CompensationReductionAmount"]');
        var reductionAmount = value('[name="CompensationReductionAmount"]');
        if (reductionAmount > 0) {
            var convertedPercent = afterDeductible > 0 ? Math.min(100, reductionAmount * 100 / afterDeductible) : 0;
            if (reductionPercentInput) reductionPercentInput.value = convertedPercent.toFixed(2);
            reductionAmount = afterDeductible * convertedPercent / 100;
        } else {
            reductionAmount = afterDeductible * value('[name="CompensationReductionPercent"]') / 100;
            if (reductionAmountInput) reductionAmountInput.value = reductionAmount.toFixed(2);
        }
        var riskSharing = afterDeductible * value('[name="RiskSharingPercent"]') / 100;
        var compensation = Math.max(0, afterDeductible - reductionAmount - riskSharing);
        var participationValueAmount = afterDeductible * (100 - value('[name="ParticipationValuePercent"]')) / 100;
        var participationFeeAmount = afterDeductible * (100 - value('[name="ParticipationFeePercent"]')) / 100;
        var customerPayment = depreciation + participationValueAmount + participationFeeAmount + deductible + reductionAmount + riskSharing;
        var approvedInput = card.querySelector('[name="ApprovedTotal"]');
        var customerInput = card.querySelector('[name="CustomerPaymentTotal"]');
        if (approvedInput) approvedInput.value = compensation.toFixed(2);
        if (customerInput) customerInput.value = Math.max(0, customerPayment).toFixed(2);
    }

    function ensureDamagePicker() {
        var modal = document.getElementById('DamagePickerModal');
        if (modal) return modal;
        modal = document.createElement('div');
        modal.id = 'DamagePickerModal';
        modal.className = 'code-picker-modal damage-picker-modal';
        modal.setAttribute('aria-hidden', 'true');
        modal.innerHTML = '<div class="code-picker-dialog damage-picker-dialog" role="dialog" aria-modal="true" aria-labelledby="DamagePickerTitle"><div class="code-picker-header"><h2 id="DamagePickerTitle">Lựa chọn đối tượng thiệt hại</h2><button type="button" class="code-picker-close damage-picker-close" aria-label="Đóng">×</button></div><div class="damage-picker-search"><input id="DamageCodeFilter" class="form-control" placeholder="Nhập mã tìm kiếm"><input id="DamageNameFilter" class="form-control" placeholder="Nhập tên tìm kiếm"><button type="button" id="DamageSearchButton" class="btn">Tìm kiếm</button></div><div class="damage-picker-body"><table class="grid-table damage-picker-table"><thead><tr><th>Mã</th><th>Tên thiệt hại</th></tr></thead><tbody id="DamageChoices"></tbody></table><div id="DamagePickerEmpty" class="code-picker-empty" style="display:none">Không tìm thấy đối tượng phù hợp.</div></div><div class="code-picker-footer"><button type="button" class="btn damage-picker-close">Đóng</button></div></div>';
        document.body.appendChild(modal);
        document.getElementById('DamageSearchButton').onclick = renderDamages;
        var inputs = modal.querySelectorAll('.damage-picker-search input');
        for (var i = 0; i < inputs.length; i++) inputs[i].onkeydown = function (event) { if (event.key === 'Enter' || event.keyCode === 13) { event.preventDefault(); renderDamages(); } };
        return modal;
    }

    function openDamagePicker() {
        var modal = ensureDamagePicker();
        renderDamages();
        modal.className = 'code-picker-modal damage-picker-modal open';
        modal.setAttribute('aria-hidden', 'false');
    }

    function closeDamagePicker() {
        var modal = document.getElementById('DamagePickerModal');
        if (!modal) return;
        modal.className = 'code-picker-modal damage-picker-modal';
        modal.setAttribute('aria-hidden', 'true');
    }

    function renderDamages() {
        var code = normalized(document.getElementById('DamageCodeFilter').value);
        var name = normalized(document.getElementById('DamageNameFilter').value);
        var body = document.getElementById('DamageChoices');
        var count = 0;
        body.innerHTML = '';
        for (var i = 0; i < damages.length; i++) {
            if (code && normalized(damages[i][0]).indexOf(code) === -1 || name && normalized(damages[i][1]).indexOf(name) === -1) continue;
            count++;
            (function (damage) {
                var row = document.createElement('tr');
                row.tabIndex = 0;
                row.innerHTML = '<td></td><td></td>';
                row.cells[0].textContent = damage[0];
                row.cells[1].textContent = damage[1];
                function choose() {
                    document.getElementById('SelectedPartDamage').value = damage[0] + ' - ' + damage[1];
                    document.getElementById('SelectedPartDamageDisplay').textContent = damage[0] + ' - ' + damage[1];
                    closeDamagePicker();
                }
                row.onclick = choose;
                row.onkeydown = function (event) { if (event.key === 'Enter' || event.key === ' ') { event.preventDefault(); choose(); } };
                body.appendChild(row);
            }(damages[i]));
        }
        document.getElementById('DamagePickerEmpty').style.display = count ? 'none' : 'block';
    }

    function showPartDetails(part) {
        activeRow = null;
        selectedPart = part;
        document.getElementById('SelectedPartCode').value = part[4];
        document.getElementById('SelectedPartName').value = part[5];
        document.getElementById('SelectedPartQuantity').value = 1;
        document.getElementById('SelectedPartDamage').value = '';
        document.getElementById('SelectedPartDamageDisplay').textContent = '-- Chọn thiệt hại --';
        document.getElementById('SelectedPartDimensions').value = '';
        document.getElementById('SelectedPartProposal').value = '';
        document.getElementById('SelectedPartType').value = '';
        document.getElementById('SelectedPartPrice').value = 0;
        document.getElementById('SelectedPartPaint').value = 0;
        document.getElementById('SelectedPartLabor').value = 0;
        calculatePartAmount();
        document.getElementById('PartPickerListView').style.display = 'none';
        document.getElementById('PartPickerDetailView').style.display = 'block';
        document.getElementById('QuotePartPickerTitle').textContent = 'Thông tin phụ tùng';
        document.getElementById('PartDetailConfirm').textContent = 'Thêm phụ tùng';
    }

    function showPartList() {
        selectedPart = null;
        document.getElementById('PartPickerDetailView').style.display = 'none';
        document.getElementById('PartPickerListView').style.display = 'block';
        document.getElementById('QuotePartPickerTitle').textContent = 'Danh sách thiết bị - phụ tùng xe';
    }

    function populatePartCodes() {
        var type = document.getElementById('PartTypeFilter').value;
        var codeSelect = document.getElementById('PartCodeFilter');
        var current = codeSelect.value;
        var seen = {};
        codeSelect.innerHTML = '<option value="">-- Mã phụ tùng --</option>';
        for (var i = 0; i < parts.length; i++) {
            if (type && parts[i][3] !== type || seen[parts[i][4]]) continue;
            seen[parts[i][4]] = true;
            var option = document.createElement('option');
            option.value = parts[i][4];
            option.textContent = parts[i][4];
            codeSelect.appendChild(option);
        }
        if (seen[current]) codeSelect.value = current;
    }

    function renderParts(resetPage) {
        if (resetPage) currentPage = 1;
        var values = ['PartTypeFilter', 'PartCodeFilter', 'PartNameFilter'].map(function (id) { return normalized(document.getElementById(id).value); });
        var body = document.getElementById('QuotePartChoices');
        var matchedParts = [];
        body.innerHTML = '';
        for (var i = 0; i < parts.length; i++) {
            var matched = true;
            for (var column = 0; column < values.length; column++) if (values[column] && normalized(parts[i][column + 3]).indexOf(values[column]) === -1) matched = false;
            if (matched) matchedParts.push(parts[i]);
        }
        var pageCount = Math.max(1, Math.ceil(matchedParts.length / pageSize));
        if (currentPage > pageCount) currentPage = pageCount;
        var pageParts = matchedParts.slice((currentPage - 1) * pageSize, currentPage * pageSize);
        for (var partIndex = 0; partIndex < pageParts.length; partIndex++) {
            (function (part) {
                var row = document.createElement('tr');
                row.tabIndex = 0;
                for (var cell = 4; cell < 6; cell++) { var td = document.createElement('td'); td.textContent = part[cell]; row.appendChild(td); }
                row.onclick = function () { showPartDetails(part); };
                row.onkeydown = function (event) { if (event.key === 'Enter' || event.key === ' ') { event.preventDefault(); showPartDetails(part); } };
                body.appendChild(row);
            }(pageParts[partIndex]));
        }
        document.getElementById('PartPickerEmpty').style.display = matchedParts.length ? 'none' : 'block';
        renderPagination(pageCount, matchedParts.length);
    }

    function renderPagination(pageCount, total) {
        var pagination = document.getElementById('PartPickerPagination');
        pagination.innerHTML = '<span>' + total + ' phụ tùng · 10 mã/trang</span>';
        if (!total) return;
        for (var page = 1; page <= pageCount; page++) {
            (function (pageNumber) {
                var button = document.createElement('button');
                button.type = 'button';
                button.className = 'part-page-button' + (pageNumber === currentPage ? ' active' : '');
                button.textContent = pageNumber;
                button.onclick = function () { currentPage = pageNumber; renderParts(false); };
                pagination.appendChild(button);
            }(page));
        }
    }

    function openPicker(trigger) {
        activeRow = null;
        activeTable = trigger.closest('.card').querySelector('.quote-parts-table');
        var modal = ensureModal();
        showPartList();
        document.getElementById('PartVehicleBrand').value = trigger.getAttribute('data-vehicle-brand') || '';
        document.getElementById('PartVehicleVersion').value = trigger.getAttribute('data-vehicle-version') || '';
        document.getElementById('PartVehicleTypeNumber').value = trigger.getAttribute('data-vehicle-type-number') || '';
        populatePartCodes();
        renderParts(true);
        modal.className = 'code-picker-modal open';
        modal.setAttribute('aria-hidden', 'false');
    }

    document.addEventListener('click', function (event) {
        if (event.target.className.indexOf('damage-picker-close') !== -1 || event.target.id === 'DamagePickerModal') { closeDamagePicker(); return; }
        var editButton = event.target.closest ? event.target.closest('.quote-part-edit') : null;
        if (editButton) { event.preventDefault(); closeActionMenus(); openPartEditor(editButton.closest('tr')); return; }
        var deleteButton = event.target.closest ? event.target.closest('.quote-part-delete') : null;
        if (deleteButton) {
            event.preventDefault();
            if (window.confirm('Bạn có chắc muốn xóa phụ tùng này?')) {
                var table = deleteButton.closest('.quote-parts-table');
                deleteButton.closest('tr').remove();
                reindexRows(table);
                recalculatePartCosts(table);
            }
            return;
        }
        closeActionMenus();
        var trigger = event.target.closest ? event.target.closest('.part-add-trigger') : null;
        if (trigger) { event.preventDefault(); openPicker(trigger); return; }
        if (event.target.className.indexOf('quote-part-picker-close') !== -1 || event.target.id === 'QuotePartPickerModal') closePicker();
    });
    document.addEventListener('input', function (event) {
        if (event.target.id === 'DeductibleCases') {
            var deductibleAmount = document.getElementById('DeductibleAmount');
            var deductibleCases = Math.max(0, parseInt(event.target.value, 10) || 0);
            if (deductibleAmount) deductibleAmount.value = 500000 * deductibleCases;
        }
        if (event.target.id === 'CompensationReductionAmount') {
            var percent = document.getElementById('CompensationReductionPercent');
            if (percent) percent.value = 0;
        } else if (event.target.id === 'CompensationReductionPercent') {
            var amount = document.getElementById('CompensationReductionAmount');
            if (amount) amount.value = 0;
        }
        if (event.target.matches('input[name$=".PartPrice"], input[name$=".PaintCost"], input[name$=".LaborCost"]')) calculateRowTotal(event.target.closest('tr'));
        var card = event.target.closest ? event.target.closest('.card') : null;
        if (card && card.querySelector('.quote-parts-table')) recalculatePartCosts(card.querySelector('.quote-parts-table'));
    });
    document.addEventListener('submit', function (event) {
        var table = event.target.querySelector ? event.target.querySelector('.quote-parts-table') : null;
        if (!table) return;
        var selects = table.querySelectorAll('select:disabled');
        for (var i = 0; i < selects.length; i++) selects[i].disabled = false;
    });
    initializeGrid();
    if (window.MutationObserver) {
        new MutationObserver(function (changes) {
            for (var i = 0; i < changes.length; i++) {
                if (changes[i].addedNodes.length) { initializeGrid(); break; }
            }
        }).observe(document.querySelector('.page') || document.body, { childList: true, subtree: true });
    }
    document.addEventListener('keydown', function (event) { if (event.key === 'Escape' || event.keyCode === 27) { if (document.getElementById('DamagePickerModal') && document.getElementById('DamagePickerModal').className.indexOf('open') !== -1) closeDamagePicker(); else closePicker(); } });
}());
