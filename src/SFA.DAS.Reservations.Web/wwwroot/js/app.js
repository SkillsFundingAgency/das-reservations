var forms = $('.validate-auto-complete');
var radioInputs = forms.find('.govuk-radios__input');
var idSelectField = 'SelectedCourseId';

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
    forms.on('submit',
        function(e) {

            $('.autocomplete__input').each(function() {
                var that = $(this);
                    if (that.val().length === 0) {
                        var fieldId = that.attr('id'),
                            selectField = $('#' + fieldId + '-select');
                        selectField[0].selectedIndex = 0;
                    }
            });

        });


}

forms.attr('novalidate', 'novalidate');


var slugify = function (text) {
    return text.toString().toLowerCase()
        .replace(/\s+/g, '-')
        .replace(/[^\w\-]+/g, '')
        .replace(/\-\-+/g, '-')
        .replace(/^-+/, '')
        .replace(/-+$/, '');
};