var autocompleteInputs = document.querySelectorAll(".app-autocomplete");

if (autocompleteInputs.length > 0) {

    for (var i = 0; i < autocompleteInputs.length; i++) {

        var input = autocompleteInputs[i];
        var container = document.createElement('div');
        var apiUrl = input.dataset.autocompleteUrl;

        container.className = "das-autocomplete-wrap";
        input.parentNode.replaceChild(container, input);

        var getSuggestions = function (query, updateResults) {
            var results = [];
            var xhr = new XMLHttpRequest();
            xhr.onreadystatechange = function () {
                if (xhr.readyState === 4) {
                    var jsonResponse = JSON.parse(xhr.responseText);
                    results = jsonResponse.map(function (r) {
                        return r;
                    });
                    updateResults(results);
                }
            }
            xhr.open("GET", apiUrl + '?searchTerm=' + query, true);
            xhr.send();
        };

        accessibleAutocomplete({
            element: container,
            id: input.id,
            name: input.name,
            defaultValue: input.value,
            displayMenu: 'overlay',
            showNoOptionsFound: false,
            minLength: 3,
            source: getSuggestions,
            placeholder: "",
            confirmOnBlur: false,
            autoselect: true
        });
    }
}