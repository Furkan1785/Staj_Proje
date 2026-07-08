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
