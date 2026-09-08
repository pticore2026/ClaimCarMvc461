(function(window,document){
    'use strict';
    function selectedIds(){var boxes=document.querySelectorAll('.js-claim-select:checked'),ids=[];for(var i=0;i<boxes.length;i++)ids.push(boxes[i].value);return ids;}
    function request(url,ids,continueValid,token,single){
        var body='continueValid='+encodeURIComponent(continueValid?'true':'false')+'&__RequestVerificationToken='+encodeURIComponent(token?token.value:'');
        for(var i=0;i<ids.length;i++)body+='&'+(single?'id':'ids')+'='+encodeURIComponent(ids[i]);
        return new Promise(function(resolve,reject){
            var xhr=new XMLHttpRequest();xhr.open('POST',url,true);xhr.setRequestHeader('Content-Type','application/x-www-form-urlencoded; charset=UTF-8');
            xhr.onload=function(){if(xhr.status>=200&&xhr.status<300){try{resolve(JSON.parse(xhr.responseText));}catch(e){reject(e);}}else reject(new Error('HTTP '+xhr.status));};
            xhr.onerror=function(){reject(new Error('Không thể kết nối máy chủ.'));};xhr.send(body);
        });
    }
    function executeDelete(button,ids,single,token){
        var url=button.getAttribute('data-url');
        return request(url,ids,false,token,single).then(function(result){
            if(result.status==='confirm')return ClaimCarNotification.confirm('warning',result.message,'Xác nhận xoá một phần','Tiếp tục xoá','Dừng lại').then(function(confirmed){if(confirmed)return request(url,ids,true,token,single);return null;});
            return result;
        }).then(function(result){
            if(!result)return;
            if(result.status==='success'){
                removeDeleted(result.deletedIds||[]);ClaimCarNotification.info(result.message,'Hoàn tất xoá');
                var successUrl=button.getAttribute('data-success-url');if(successUrl)setTimeout(function(){window.location.href=successUrl;},1200);
            }else ClaimCarNotification.error(result.message||'Không thể xoá các hồ sơ đã chọn.');
        });
    }
    function removeDeleted(ids){
        for(var i=0;i<ids.length;i++){var box=document.querySelector('.js-claim-select[value="'+ids[i]+'"]');if(box&&box.parentNode&&box.parentNode.parentNode)box.parentNode.parentNode.remove();}
        var selectAll=document.querySelector('.js-claim-select-all');if(selectAll){selectAll.checked=false;selectAll.indeterminate=false;}
    }
    document.addEventListener('click',function(e){
        var button=e.target.closest?e.target.closest('.js-bulk-delete, .js-delete-claim'):null;if(!button)return;e.preventDefault();
        var single=button.classList.contains('js-delete-claim');
        var ids=single?[button.getAttribute('data-id')]:selectedIds();
        if(ids.length===0){ClaimCarNotification.warning('Vui lòng chọn ít nhất một hồ sơ bồi thường để xoá.');return;}
        var token=single?button.closest('form').querySelector('input[name="__RequestVerificationToken"]'):document.querySelector('#bulk-delete-form input[name="__RequestVerificationToken"]');
        ClaimCarNotification.confirm('warning','Bạn có chắc chắn muốn xoá '+(single?'hồ sơ này':ids.length+' hồ sơ đã chọn')+' không?','Xác nhận xoá','Xoá','Huỷ')
            .then(function(confirmed){if(confirmed)return executeDelete(button,ids,single,token);return null;})
            .catch(function(){ClaimCarNotification.error('Có lỗi xảy ra khi xoá hồ sơ. Vui lòng thử lại.');});
    });
})(window,document);
