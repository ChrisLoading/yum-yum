﻿//const { type, error } = require("jquery");

// Return button to /fridge/index
$("#btn-return").on("click", function () {
    window.location.href = "/fridge/index";
});

// Dynamically update ingredient tag list by FILTER CANVAS
$('.ingredient-attribute-checkbox').on('change', function () {
    //console.log($(this).prop('checked'));
    $(this).prev().toggleClass('selected');

    // Get all selected ID
    let selectedId = [];
    $('.ingredient-attribute-checkbox:checked').each(function () {
        selectedId.push($(this).val());
    });
    console.log(selectedId);

    // Ajax request to get filtered tags
    $.ajax(
        {
            url: 'FilterIngredients',
            type: 'POST',
            data: JSON.stringify(selectedId),
            contentType: 'application/json',
            success: function (response) {
                // Update tag section dynamically

                // Check selected ID
                console.log(response);

                // Clear section
                $('#ingredient-list').empty();

                // Append updated ingredients
                $.each(response, function (index, item) {
                    $('#ingredient-list').append(
                        `
                        <button class="tag-button d-inline-flex align-items-center btn btn-tags">
                                <img src="${item.IngredientIcon}" alt="" />
                                <span>${item.IngredientName}</span>
                        </button>
                        `
                    );
                });
            },
            error: function (xhr, status, error) {
                console.error('Error: ' + error);
            }
        }
    );
});

// Dynamically update ingredient tag list by USER INPUT
$('#ingredientInput').on('input', function () {
    var keyword = $(this).val();

    $.ajax(
        {
            url: 'SearchIngredients',
            type: 'GET',
            data: { searchKeyword: keyword },
            success: function (data) {
                $('#ingredient-list').html(data);
            },
            error: function () {
                console.log('An error occurred while fetching the data.');
            }
        }
    );
});

// Create new item rows in item list by DEFAULT TAGS
$('#ingredient-list').on('click', '.tag-button', function () {
    let clickedButtonHtml = $(this).prop('outerHTML');
    console.log(clickedButtonHtml);

    let ingredientName = $(this).find('span').text().trim();
    console.log(ingredientName);

    $.ajax({
        url: 'GetUnitsByIngredientName', 
        type: 'GET', 
        data: { ingredientName: ingredientName }, 
        success: function (response) {
            console.log(response);

            let optionsHtml = '';
            response.forEach(function (unit) {
                optionsHtml += `<option value="${unit.UnitId}">${unit.UnitName}</option>`
            })
            console.log(optionsHtml);

            let newRowHtml = `
                <div class="row align-items-center mb-1">
                    <div class="col-md-3 col-4">
                        ${clickedButtonHtml}
                    </div>
                    <div class="col-md-2 col-4 text-center">
                        <input type="number" class="form-control" value="1" min="1">
                    </div>
                    <div class="col-md-2 col-4">
                        <select class="form-select">
                            ${optionsHtml}
                        </select>
                    </div>
                    <div class="col-md-3 col-8 text-center">
                        <input type="date" class="form-control">
                    </div>
                    <div class="col-md-1 col-4 d-flex justify-content-end">
                        <button class="btn d-flex justify-content-center align-items-center ps-1 pe-1 btn-delete">
                            <img src="../img/icon/delete.svg" alt="" />
                        </button>
                    </div>
                </div>
            `;

            $('.item-list').prepend(newRowHtml);
        }, 
        error: function (error) {
            console.log('Error: ' + error);
        }
    });
});

// Create new item rows in item list by ADDing CUSTOMIZED ONEs
$('#food-tag-sec').on('click', '#add-new-ingred', function () {
    $.ajax({
        url: 'GetOtherUnits', 
        type: 'GET',
        dataType: 'json',
        success: function (response) {

            let otherOptionsHtml = '';
            response.forEach(function (unit) {
                otherOptionsHtml += `<option value="${unit.UnitId}">${unit.UnitName}</option>`
            })

            let newRowHtml = `
                <div class="row align-items-center mb-3">
                    <div class="col-md-3 col-4">
                        <input type="text" id="" class="form-control" placeholder="新增食材名稱">
                    </div>
                    <div class="col-md-2 col-4 text-center">
                        <input type="number" class="form-control" value="1" min="1">
                    </div>
                    <div class="col-md-2 col-4">
                        <select class="form-select">
                            ${otherOptionsHtml}
                        </select>
                    </div>
                    <div class="col-md-3 col-8 text-center">
                        <input type="date" class="form-control" value="">
                    </div>
                    <div class="col-md-1 col-4 d-flex justify-content-end">
                        <button class="btn d-flex justify-content-center align-items-center ps-1 pe-1 btn-delete">
                            <img src="../img/icon/delete.svg" alt="" />
                        </button>
                    </div>
                </div>
            `;
            $('.item-list').prepend(newRowHtml);
        }, 
        error: function (error) {
            console.log('Error: ' + error);
        }
    });
});

// Delete item row in item list
$('.item-list').on('click', '.btn-delete', function () {
    $(this).closest('.row').remove();
});