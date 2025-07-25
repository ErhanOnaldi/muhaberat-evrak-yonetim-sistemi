"use strict";

var KTReportList = function () {
    // Private variables
    var datatable;
    var table = document.querySelector('#kt_table_reports');

    // Private functions
    var initReportList = function () {
        // Set date data order
        const tableRows = table.querySelectorAll('tbody tr');

        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatable = $(table).DataTable({
            info: true,
            order: [],
            pageLength: 10,
            lengthChange: false,
            columnDefs: [
                { orderable: false, targets: 5 }, // Disable ordering on column 5 (actions)
            ],
            dom: `<'row'<'col-sm-12'tr>>
            <'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>`,
            // Layout for the datatable
            language: {
                info: "Toplam _TOTAL_ rapor",
                search: "Rapor Ara:",
                lengthMenu: "_MENU_",
                emptyTable: "Rapor bulunmamaktadır",
                infoEmpty: "Rapor bulunamadı",
                zeroRecords: "Eşleşen rapor bulunamadı",
                infoFiltered: "(_MAX_ rapor içerisinden bulunan)",
                paginate: {
                    first: "İlk",
                    last: "Son",
                    next: "Sonraki",
                    previous: "Önceki"
                }
            }
        });

        // Initialize Select2
        $('[data-control="select2"]').select2({
            language: {
                noResults: function () {
                    return "Sonuç bulunamadı";
                }
            }
        });

        // Initialize DateRangePicker
        $("#kt_daterangepicker").daterangepicker({
            locale: {
                format: 'DD.MM.YYYY',
                separator: ' - ',
                applyLabel: 'Uygula',
                cancelLabel: 'İptal',
                fromLabel: 'From',
                toLabel: 'To',
                customRangeLabel: 'Custom',
                weekLabel: 'W',
                daysOfWeek: ['Pz', 'Pt', 'Sa', 'Ça', 'Pe', 'Cu', 'Ct'],
                monthNames: ['Ocak', 'Şubat', 'Mart', 'Nisan', 'Mayıs', 'Haziran', 'Temmuz', 'Ağustos', 'Eylül', 'Ekim', 'Kasım', 'Aralık'],
                firstDay: 1
            },
            startDate: moment('24.04.2024', 'DD.MM.YYYY'),
            endDate: moment('31.12.2024', 'DD.MM.YYYY')
        });

        // Update Select2 style when multiple items are selected
        $('select[multiple]').on('change', function () {
            var selectedCount = $(this).val() ? $(this).val().length : 0;
            if (selectedCount > 0) {
                var text = $(this).val()[selectedCount - 1];
                if (selectedCount > 1) {
                    text += ' +' + (selectedCount - 1);
                }
                $(this).next('.select2').find('.select2-selection__rendered').text(text);
            }
        });
    }

    // Search Datatable
    var handleSearchDatatable = () => {
        const filterSearch = document.querySelector('[data-kt-report-table-filter="search"]');
        filterSearch.addEventListener('keyup', function (e) {
            datatable.search(e.target.value).draw();
        });
    }

    // Handle download buttons
    var handleDownloadButtons = () => {
        const downloadButtons = table.querySelectorAll('[data-kt-report-table-filter="download_row"]');

        downloadButtons.forEach(button => {
            button.addEventListener('click', function (e) {
                e.preventDefault();

                // Show loading indication
                button.setAttribute('data-kt-indicator', 'on');

                // Disable button to avoid multiple click 
                button.disabled = true;

                // Simulating download process
                setTimeout(function () {
                    // Remove loading indication
                    button.removeAttribute('data-kt-indicator');

                    // Enable button
                    button.disabled = false;

                    // Show success message
                    Swal.fire({
                        text: "Rapor başarıyla indirildi!",
                        icon: "success",
                        buttonsStyling: false,
                        confirmButtonText: "Tamam",
                        customClass: {
                            confirmButton: "btn btn-primary"
                        }
                    });
                }, 2000);
            });
        });
    }

    // Public methods
    return {
        init: function () {
            if (!table) {
                return;
            }

            initReportList();
            handleSearchDatatable();
            handleDownloadButtons();
        }
    }
}();

// On document ready
KTUtil.onDOMContentLoaded(function () {
    KTReportList.init();
});