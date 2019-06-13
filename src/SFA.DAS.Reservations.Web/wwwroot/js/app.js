var forms = $('.validate-auto-complete');
var radioInputs = forms.find('.govuk-radios__input');
var idSelectField = 'course-search';

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

forms.attr('novalidate', 'novalidate');

forms.on('submit', function (e) {

    var canSubmit = this.checkValidity(),
        form = this,
        validationMessages = [];

    $('.autocomplete__input').each(function () {
        var that = $(this);
        setTimeout(function () {
            if (!checkField(that)) {
                var fieldId = that.attr('id'),
                    errorMessage = $('#' + fieldId + '-select').data('validation-message');
                validationMessages.unshift({ id: fieldId, message: errorMessage });
                canSubmit = false
            }
        }, 100);
    });

    radioInputs.each(function () {
        hideValidationMessage($(this));
    });

    radioInputs.each(function () {
        var result = this.checkValidity();
        if (!result) {
            var errorMessage = showRadioValidationMessage($(this));
            if (errorMessage !== undefined)
                validationMessages.unshift(errorMessage);
        } else {
            hideValidationMessage($(this));
        }
    });

    setTimeout(function () {
        if (canSubmit) {
            hideErrorSummary();
            form.submit();
        } else {
            hideErrorSummary();
            showErrorSummary(validationMessages);
        }
    }, 300);

    e.preventDefault();

}).keydown(function (e) {
    if (e.which === 13) {
        e.preventDefault();
        return false;
    }
});

var checkField = function ($field) {
    var textInput = $field,
        selectField = $('#' + textInput.attr('id') + '-select');

    if (selectField[0].selectedIndex === 0) {
        showSelectValidationMessage(selectField);
        return false;

    } else {
        hideValidationMessage(selectField);
    }
    return true;
};

var showSelectValidationMessage = function ($field) {
    var fieldGroup = $field.parent(),
        fieldLabel = fieldGroup.find('label'),
        validationMessageText = $field.data('validation-message'),
        validationMessage = $('<span>').addClass('govuk-error-message').text(validationMessageText),
        alreadyShowingError = fieldGroup.hasClass('govuk-form-group--error');

    if (!alreadyShowingError) {
        fieldGroup.addClass('govuk-form-group--error');
        fieldLabel.after(validationMessage);
    }
};

var showRadioValidationMessage = function ($field) {
    var $parent = $field.closest('.govuk-form-group'),
        $radioGroup = $field.closest('.govuk-radios'),
        validationMessageText = $radioGroup.data('validation-message');

    if (!$parent.hasClass('govuk-form-group--error')) {
        var validationMessage = $('<span>')
            .text(validationMessageText)
            .prop('class', 'govuk-error-message')
            .prop('id', 'validation-' + slugify(validationMessageText));
        $radioGroup.before(validationMessage);
        $parent.addClass('govuk-form-group--error');
        $field.attr({
            'aria-describedby': 'validation-' + slugify(validationMessage),
            'aria-invalid': 'true'
        });
        return { id: $field.attr('id'), message: validationMessageText }
    }
};

var hideValidationMessage = function ($field) {
    var $parent = $field.closest('.govuk-form-group');

    $parent.removeClass('govuk-form-group--error');
    $parent.find('.govuk-error-message').remove();
    $field.removeAttr('aria-describedby');
    $field.removeAttr('aria-invalid');
};

var showErrorSummary = function (validationMessages) {
    var errorSummary = $('.govuk-error-summary'),
        errorList = $('.govuk-list.govuk-error-summary__list');
    if (errorSummary.length === 0) {
        errorSummary = $('<div>').addClass('govuk-error-summary').attr('tabindex', -1).data('module', 'error-summary');
        var errorTitle = $('<h2>').addClass('govuk-error-summary__title').text('There is a problem');
        var errorBody = $('<div>').addClass('govuk-error-summary__body');
        errorList = $('<ul>').addClass('govuk-list govuk-error-summary__list');

        errorBody.html(errorList);
        errorSummary.append(errorTitle, errorBody);

        var pageHeading = $('h1.govuk-heading-xl');
        pageHeading.before(errorSummary);
    }

    $.each(validationMessages, function (index, value) {
        var errorLink = $('<a>').html(value.message).attr('href', '#' + value.id);
        var errorListItem = $('<li>').append(errorLink);
        errorList.append(errorListItem);
    });
};

var hideErrorSummary = function () {
    $('.govuk-error-summary').remove();
};

var slugify = function (text) {
    return text.toString().toLowerCase()
        .replace(/\s+/g, '-')
        .replace(/[^\w\-]+/g, '')
        .replace(/\-\-+/g, '-')
        .replace(/^-+/, '')
        .replace(/-+$/, '');
};