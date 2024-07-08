document.addEventListener("DOMContentLoaded", function () {
    console.log("DOM fully loaded and parsed");

    let date = new Date();
    let formattedDate =
        date.getFullYear() +
        "-" +
        String(date.getMonth() + 1).padStart(2, "0") +
        "-" +
        String(date.getDate()).padStart(2, "0");

    let futureDate = new Date(date.getTime()); // Create a copy of the original date
    futureDate.setMonth(futureDate.getMonth() + 2);
    let futureFormattedDate =
        futureDate.getFullYear() +
        "-" +
        String(futureDate.getMonth() + 1).padStart(2, "0") +
        "-" +
        String(futureDate.getDate()).padStart(2, "0");

    console.log("Formatted Date: ", formattedDate);
    console.log("Future Formatted Date: ", futureFormattedDate);

    // Get the dates from the hidden input field
    let preSelectedDatesString = document.getElementById('Dates').value;
    let preSelectedDates = [];
    if (preSelectedDatesString) {
        preSelectedDates = preSelectedDatesString.split(', ').map(function (dateStr) {
            return new Date(dateStr);
        });
    }

    console.log("Pre-selected Dates: ", preSelectedDates);

    // Check if mobiscroll is loaded
    if (typeof mobiscroll !== 'undefined') {
        console.log("Mobiscroll is loaded");

        mobiscroll.setOptions({
            theme: "ios",
            themeVariant: "light",
            locale: mobiscroll.localeVi,
            firstDay: 1 // Set Monday as the first day of the week
        });

        let mobiscrollInstance = mobiscroll.datepicker('#demo-counter', {
            controls: ['calendar'],
            display: 'inline',
            selectMultiple: true,
            selectCounter: true, // Shows the count of selected dates
            valid: [{
                start: formattedDate,
                end: futureFormattedDate
            }],
            onChange: function (event, inst) {
                console.log("onChange event fired"); // Debug log

                // Get the list of selected dates
                var selectedDates = event.value.map(function (dateObj) {
                    let year = dateObj.getFullYear();
                    let month = String(dateObj.getMonth() + 1).padStart(2, "0");
                    let day = String(dateObj.getDate()).padStart(2, "0");
                    return `${year}-${month}-${day}`;
                });

                // Set the value to the hidden input field
                document.getElementById('Dates').value = selectedDates.join(', ');
                console.log("Selected dates set in hidden input: " + selectedDates.join(', ')); // Debug log
            }
        });

        // Set pre-selected dates after initialization
        mobiscrollInstance.setVal(preSelectedDates, true);

    } else {
        console.error("Mobiscroll is not loaded");
    }
});
