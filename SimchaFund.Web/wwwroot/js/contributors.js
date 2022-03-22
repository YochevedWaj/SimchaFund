$(() => {
    $("#add-contributor").on('click', function () {
        $('#first-name').val('');
        $('#last-name').val('');
        $('#number').val('');
        $('#datepicker').val('');
        $('#always-include').prop('checked', false);
        $('#initial-deposit').attr('style', '');
        $('#add-update-contributor').attr('action', '/contributors/new');
        $("#modal-add-contributor").modal();
    })

    $(".modal-deposit").on('click', function () {
        const id = $(this).data("contributorid");
        const name = $(this).closest('td').next('td').html();
        $('#contributor-name').html(name);
        $('#contributor-id-hidden').val(id);
        $("#modal-add-deposit").modal();
    })

    $(".edit-button").on('click', function () {
        $('#first-name').val($(this).data('first-name'));
        $('#last-name').val($(this).data('last-name'));
        $('#number').val($(this).data('number'));
        $('#datepicker').val($(this).data('date'));
        const alwaysInclude = $(this).data('always-include');
        $('#always-include').prop('checked', alwaysInclude === "True");
        $('#initial-deposit').attr('style', 'display:none;');
        const id = $(this).data('contributor-id');
        $('#contributor-id').html(`<input type="hidden" name="ID" class="form-control" value="${id}" />`);
        $('#add-update-contributor').attr('action', '/contributors/updatecontributor');
        $("#modal-add-contributor").modal();
    })
});