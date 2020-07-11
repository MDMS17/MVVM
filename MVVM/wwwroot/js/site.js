// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function GotoPageClientClick(pageCurrent, pageTotal) {
    if (!$.isNumeric($("#" + pageCurrent).val()) || $("#" + pageCurrent).val() < 1 || Number($("#" + pageCurrent).val()) > Number($("#" + pageTotal).val())) {
        alert("Invalid page number " + $("#" + pageCurrent).val());
        return false;
    }
    return true;
}

$('.signButton').click(function () {
    //alert(this.id);
    if ($(this).html() === "+") $(this).html("-");
    else $(this).html("+");
    $('.parentMonth_I' + this.id).each(function () {
        $(this).toggle();
    });
    $('.children_I' + this.id).each(function () {
        $(this).toggle();
    });
});

$('#collapsable tr').each(function () {
    if ($(this).attr("class") !== "parentYear") {
        $(this).css("display", "none");
    }
});

function McpdCascadeSelection_Changed(obj, NumberOfItems) {
    var objName = obj.id.substring(0, 4);
    var objText = objName.replace("m", "t");
    var objNo = obj.id.substring(4);
    var objNum = Number(objNo);
    var objNext = objNum + 1;

    if ($(obj).val() === '') {
        var objOptions = $('#' + obj.id + ' > option').clone();
        for (var oi = objNum; oi <= NumberOfItems; oi++) {
            $('#' + objText + oi).css("display", "none");
            $('#' + objText + oi).val('');
            var oj = oi + 1;
            if (oj <= NumberOfItems) {
                $('#' + objName + oj).css("display", "none");
                $('#' + objName + oj).empty();
                $.each(objOptions, function (index, item) {
                    $('#' + objName + oj).append($('<option/>', {
                        value: item.value,
                        text: item.value
                    }));
                });
                $('#' + objName + oj).prop("selectedIndex", 0);
            }
        }
    }
    else {
        for (var oi2 = objNext; oi2 <= NumberOfItems; oi2++) {
            $('#' + objText + oi2).css("display", "none");
            $('#' + objText + oi2).val('');
            $('#' + objName + oi2).css("display", "none");
            $('#' + objName + oi2).find("option:contains(" + $(obj).val() + ")").remove();
            $('#' + objName + oi2).prop("selectedIndex", 0);
        }
        $('#' + objText + objNum).css("display", "block");
        if (objNext <= NumberOfItems) $('#' + objName + objNext).css("display", "block");
    }
}

function FileTableSelectedItems_Changed() {
    $('#SelectedItemsClient').val('');
    var s = '';
    $('.FileTableItem').each(function () {
        s += (this.checked) ? '1,' : '0,';
    });
    $('#SelectedItemsClient').val(s);
    return true;
}
function SelectAllToggle(obj) {
    if (obj.checked) {
        $('.FileTableItem').each(function () {
            $(this).prop("checked", true);
        });
    }
    else {
        $('.FileTableItem').each(function () {
            $(this).prop("checked", false);
        });
    }
}


