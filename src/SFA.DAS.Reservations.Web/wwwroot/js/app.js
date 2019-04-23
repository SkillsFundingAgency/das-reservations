﻿var idSelectField = 'course-search';
var selectEl = document.querySelector('#' + idSelectField);
if (selectEl) {
    accessibleAutocomplete.enhanceSelectElement({
        selectElement: selectEl,
        minLength: 3,
        autoselect: true,
        defaultValue: '',
        displayMenu: 'overlay',
        placeholder: 'Start typing to search apprenticeships',
        onConfirm: function (opt) {
            var txtInput = document.querySelector('#' + idSelectField);
            var searchString = opt || txtInput.value;
            var requestedOption = [].filter.call(this.selectElement.options,
                function (option) {
                    return (option.textContent || option.innerText) === searchString
                }
            )[0];
            if (requestedOption) {
                requestedOption.selected = true;
            } else {
                this.selectElement.selectedIndex = 0;
            }
        }
    });
}

$('form.validate-auto-complete').on('submit', function (e) {

    var canSubmit = true,
        form = this,
        validationMessages = [];

    $('.autocomplete__input').each(function () {
        var that = $(this);
        setTimeout(function () {
            if (!checkField(that)) {
                var fieldId = that.attr('id'),
                    errorMessage = $('#' + fieldId + '-select').data('validation-message');

                validationMessages.push({ id: fieldId, message: errorMessage });
                canSubmit = false
            }
        }, 100);
    });

    setTimeout(function () {
        if (canSubmit) {
            hideErrorSummary();
            form.submit();
        } else {
            showErrorSummary(validationMessages);
        }
    }, 300);

    e.preventDefault();

}).keydown(function (e) {
    if (e.which == 13) {
        e.preventDefault();
        return false;
    }
});

var checkField = function ($field) {
    var textInput = $field,
        selectField = $('#' + textInput.attr('id') + '-select'),
        valueLength = $field.val().length;

    if (valueLength > 0 && selectField[0].selectedIndex === 0) {
        showInlineValidation(selectField);
        return false;

    } else {
        hideInlineValidation(selectField);
    }
    return true;
}

var showInlineValidation = function ($field) {
    var fieldGroup = $field.parent(),
        fieldLabel = fieldGroup.find('label'),
        validationMessageText = $field.data('validation-message'),
        validationMessage = $('<span>').addClass('govuk-error-message').text(validationMessageText),
        alreadyShowingError = fieldGroup.hasClass('govuk-form-group--error');

    if (!alreadyShowingError) {
        fieldGroup.addClass('govuk-form-group--error');
        fieldLabel.after(validationMessage);
    }
}

var hideInlineValidation = function ($field) {
    var fieldGroup = $field.parent();
    var validationMessage = fieldGroup.find('span.govuk-error-message');

    fieldGroup.removeClass('govuk-form-group--error');
    validationMessage.remove();

}

var showErrorSummary = function (validationMessages) {
    var errorSummary = $('.govuk-error-summary'), 
        errorList = $('.govuk-list.govuk-error-summary__list');
    
    if (typeof errorSummary === 'undefined') {
        errorSummary = $('<div>').addClass('govuk-error-summary');
        var errorTitle = $('<h2>').addClass('govuk-error-summary__title').text('There is a problem');
        var errorBody = $('<div>').addClass('govuk-error-summary__body');
        errorList = $('<ul>').addClass('govuk-list govuk-error-summary__list');

        errorBody.html(errorList);
        errorSummary.append(errorTitle, errorBody);

        var pageHeading = $('h1.govuk-heading-xl');
        pageHeading.before(errorSummary);
    }
    
    $.each(validationMessages, function (index, value) {
        if ($('.govuk-list.govuk-error-summary__list li a:contains(' + value.message + ')').length > 0)
            return;
        
        var errorLink = $('<a>').html(value.message).attr('href', '#' + value.id);
        var errorListItem = $('<li>').append(errorLink);
        errorList.append(errorListItem);
    });
}

var hideErrorSummary = function () {
    $('.govuk-error-summary').remove();
}