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
            var options = this.selectElement.options;
            var requestedOption = null;
            var searchString = '';

            if (opt && typeof opt === 'object' && opt.nodeName === 'OPTION') {
                requestedOption = opt;
                requestedOption.selected = true;
            } else {
                var txtInput = document.querySelector('#' + idSelectField);
                searchString = (typeof opt === 'string' ? opt : (txtInput && txtInput.value) || '').trim().replace(/\s+/g, ' ');
                requestedOption = [].filter.call(options, function (option) {
                    var text = (option.textContent || option.innerText || '').trim().replace(/\s+/g, ' ');
                    return text === searchString;
                })[0];
                if (!requestedOption && searchString.length > 0) {
                    requestedOption = [].filter.call(options, function (option) {
                        return option.value === searchString;
                    })[0];
                }
                if (requestedOption) {
                    requestedOption.selected = true;
                } else {
                    this.selectElement.selectedIndex = 0;
                }
            }

            this.selectElement.dispatchEvent(new Event('change', { bubbles: true }));
            var selectedOption = options[this.selectElement.selectedIndex];
            var allowPreviousDate = selectedOption && selectedOption.getAttribute('data-allow-previous-date') === 'true';
            document.dispatchEvent(new CustomEvent('courseSelectionConfirmed', { detail: { allowPreviousDate: !!allowPreviousDate } }));
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


var pageTitle = document.querySelector('h1.govuk-heading-xl');
var panelTitle = document.querySelector('h1.govuk-panel__title')
var pageTitleText = null

if (pageTitle !== null) {
    pageTitleText = pageTitle.innerHTML.trim()
} else if (panelTitle !== null) {
    pageTitleText = panelTitle.innerHTML.trim()
}

// Radio button selection - dataLayer pushes
var radioWrapper = document.querySelector('.govuk-radios');
if (radioWrapper !== null) {
    var radios = radioWrapper.querySelectorAll('input[type=radio]');
    var labelText;
    var dataLayerObj;
    nodeListForEach(radios, function(radio) {
        radio.addEventListener('change', function() {
        labelText = this.nextElementSibling.innerText;
        dataLayerObj = {
            event: 'radio button selected',
            page: pageTitleText,
            radio: labelText
        }
        window.dataLayer.push(dataLayerObj)
        })
    })
}

function nodeListForEach(nodes, callback) {
    if (window.NodeList.prototype.forEach) {
        return nodes.forEach(callback)
    }
    for (var i = 0; i < nodes.length; i++) {
        callback.call(window, nodes[i], i, nodes);
    }
}