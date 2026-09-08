(function(window,document){
    'use strict';
    var settings={info:{title:'Thông báo',icon:'i'},warning:{title:'Cảnh báo',icon:'!'},error:{title:'Lỗi',icon:'×'}};
    var confirmResolver=null;
    function normalizeType(type){type=(type||'info').toLowerCase();return settings[type]?type:'info';}
    function ensurePopup(){
        var popup=document.getElementById('claimcar-notification');
        if(popup)return popup;
        popup=document.createElement('div');
        popup.id='claimcar-notification';popup.className='notification-popup';popup.setAttribute('aria-hidden','true');
        popup.innerHTML='<div class="notification-backdrop" data-notification-close></div><div class="notification-dialog" role="alertdialog" aria-modal="true" aria-labelledby="notification-title" aria-describedby="notification-message"><div class="notification-icon" aria-hidden="true"></div><div class="notification-content"><h2 id="notification-title"></h2><p id="notification-message"></p></div><div class="notification-actions"><button class="notification-cancel" type="button" data-notification-close>Đóng</button><button class="notification-confirm" type="button" data-notification-confirm>Tiếp tục</button></div></div>';
        popup.onclick=function(e){if(e.target.getAttribute('data-notification-close')!==null)close(false);if(e.target.getAttribute('data-notification-confirm')!==null)close(true);};
        document.body.appendChild(popup);return popup;
    }
    function show(type,message,title){
        type=normalizeType(type);var popup=ensurePopup();
        popup.className='notification-popup notification-'+type+' open';
        popup.querySelector('.notification-icon').textContent=settings[type].icon;
        popup.querySelector('#notification-title').textContent=title||settings[type].title;
        popup.querySelector('#notification-message').textContent=message||'';
        popup.querySelector('.notification-confirm').style.display='none';
        popup.querySelector('.notification-cancel').textContent='Đóng';
        popup.setAttribute('aria-hidden','false');popup.querySelector('.notification-cancel').focus();
    }
    function confirm(type,message,title,confirmText,cancelText){
        return new Promise(function(resolve){
            show(type,message,title);var popup=ensurePopup();confirmResolver=resolve;
            popup.querySelector('.notification-confirm').textContent=confirmText||'Tiếp tục';
            popup.querySelector('.notification-confirm').style.display='inline-block';
            popup.querySelector('.notification-cancel').textContent=cancelText||'Dừng lại';
            popup.querySelector('.notification-confirm').focus();
        });
    }
    function close(result){var popup=document.getElementById('claimcar-notification');if(!popup)return;popup.className='notification-popup';popup.setAttribute('aria-hidden','true');if(confirmResolver){var resolve=confirmResolver;confirmResolver=null;resolve(result===true);}}
    document.addEventListener('keydown',function(e){if(e.key==='Escape')close();});
    window.ClaimCarNotification={show:show,confirm:confirm,close:close,info:function(message,title){show('info',message,title);},warning:function(message,title){show('warning',message,title);},error:function(message,title){show('error',message,title);}};
})(window,document);
