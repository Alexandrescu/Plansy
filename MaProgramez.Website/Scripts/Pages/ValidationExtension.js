$(document).ready(function () {

});

var currentCulture = $("meta[name='accept-language']").prop("content"),
        language;

// Set Globalize to the current culture driven by the meta tag (if any)
if (currentCulture) {
    language = (currentCulture in $.fn.datepicker.dates)
        ? currentCulture //a language exists which looks like "zh-CN" so we'll use it
        : currentCulture.split("-")[0]; //we'll try for a language that looks like "de" and use it if it exists (otherwise it will fall back to the default)
}

//Initialise any date pickers
try {
    $('.datepicker').datepicker({ language: language });
} catch (err) { }

jQuery.validator.addMethod('requiredif',
    function (value, element, parameters) {
        var id = '#' + parameters['dependentproperty'];
        // get the target value (as a string, 
        // as that's what actual value will be)
        var targetvalue = parameters['targetvalue'];
        targetvalue = (targetvalue == null ? '' : targetvalue).toString();

        var targetvaluearray = targetvalue.split('|');

        for (var i = 0; i < targetvaluearray.length; i++) {

            // get the actual value of the target control
            // note - this probably needs to cater for more 
            // control types, e.g. radios
            var control = $(id);
            var controltype = control.attr('type');
            var actualvalue =
            controltype === 'checkbox' ?
            control.attr('checked') ? "true" : "false" :
            control.val();

            // if the condition is true, reuse the existing 
            // required field validator functionality
            if (targetvaluearray[i] === actualvalue) {
                return $.validator.methods.required.call(this, value, element, parameters);
            }
        }

        return true;
    }
);

jQuery.validator.unobtrusive.adapters.add(
    'requiredif',
    ['dependentproperty', 'targetvalue'],
    function (options) {
        options.rules['requiredif'] = {
            dependentproperty: options.params['dependentproperty'],
            targetvalue: options.params['targetvalue']
        };
        options.messages['requiredif'] = options.message;
    });


jQuery.validator.addMethod("enforcetrue", function (value, element, param) {
    return element.checked;
});
jQuery.validator.unobtrusive.adapters.addBool("enforcetrue");