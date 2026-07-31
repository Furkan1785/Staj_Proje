// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Grid ekranlarındaki sütun üstü filtre inputlarını/dropdown'larını DataTable'a bağlar.
// Sütun index'i olarak elemanın DOM sırası değil, bulunduğu <th>'nin gerçek cellIndex'i
// kullanılıyor — aralarda filtresiz boş <th></th> hücreler varsa (ör. Bakiye, Tutar)
// DOM sırası ile gerçek sütun index'i kayar, bu da filtrelerin yanlış sütuna uygulanmasına yol açar.
function sutunFiltreleriBagla(tabloElemanId, dataTable) {
    document.querySelectorAll('#' + tabloElemanId + ' thead tr:nth-child(2) .sutun-filtre').forEach(function (eleman) {
        var sutunIndex = eleman.closest('th').cellIndex;
        eleman.addEventListener('keyup', function () { dataTable.column(sutunIndex).search(this.value).draw(); });
        eleman.addEventListener('change', function () { dataTable.column(sutunIndex).search(this.value).draw(); });
    });
}

// Dar ekranlarda sol sidebar'ı aç/kapat (bkz. site.css .app-sidebar / body.sidebar-open kuralları).
document.addEventListener('DOMContentLoaded', function () {
    var toggle = document.getElementById('sidebarToggle');
    var sidebar = document.getElementById('appSidebar');
    var backdrop = document.getElementById('sidebarBackdrop');
    if (!toggle || !sidebar || !backdrop) return;

    function sidebarKapat() {
        sidebar.classList.remove('show');
        document.body.classList.remove('sidebar-open');
    }

    toggle.addEventListener('click', function () {
        sidebar.classList.toggle('show');
        document.body.classList.toggle('sidebar-open');
    });
    backdrop.addEventListener('click', sidebarKapat);
});

// "Kaydet" butonu içeren formlarda değişiklik yapılıp kaydetmeden sayfadan
// ayrılmaya çalışılırsa tarayıcı uyarısı gösterir. Hedef sadece Ekle/Düzenle
// ekranları ve küçük modaller (submit metni "Kaydet") — Login, arama/filtre
// kutuları ve tek hareketli Onayla/İptal formları bu kapsama girmiyor.
(function () {
    var kirliFormlar = new Set();

    function kaydetButonuVarMi(form) {
        return Array.from(form.querySelectorAll('button[type="submit"], input[type="submit"]'))
            .some(function (btn) { return (btn.textContent || btn.value || '').trim() === 'Kaydet'; });
    }

    document.addEventListener('DOMContentLoaded', function () {
        document.querySelectorAll('form').forEach(function (form) {
            if (!kaydetButonuVarMi(form)) return;
            form.addEventListener('input', function () { kirliFormlar.add(form); });
            form.addEventListener('change', function () { kirliFormlar.add(form); });
            form.addEventListener('submit', function () { kirliFormlar.delete(form); });
        });
    });

    window.addEventListener('beforeunload', function (e) {
        if (kirliFormlar.size === 0) return;
        e.preventDefault();
        e.returnValue = '';
    });
})();
