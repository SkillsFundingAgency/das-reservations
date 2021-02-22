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