// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Grid ekranlarındaki sütun üstü filtre inputlarını/dropdown'larını DataTable'a bağlar.
function sutunFiltreleriBagla(tabloElemanId, dataTable) {
    document.querySelectorAll('#' + tabloElemanId + ' thead tr:nth-child(2) .sutun-filtre').forEach(function (eleman, index) {
        eleman.addEventListener('keyup', function () { dataTable.column(index).search(this.value).draw(); });
        eleman.addEventListener('change', function () { dataTable.column(index).search(this.value).draw(); });
    });
}
