document.getElementById("timeInterval").addEventListener("change", function () {
    var interval = this.value;
    var startHour = 7;
    var endHour = 18;  // Extend to 18:00 for the 1-hour intervals
    var checkboxContainer = document.getElementById("checkboxContainer");
    checkboxContainer.innerHTML = "";
    var prefix = interval === "30" ? "short" : "long";
    var count = 1;

    for (var i = startHour; i < endHour; i += interval / 60) {
        var startMinute = i % 1 ? 30 : 0;
        var nextIntervalHour = Math.floor(i + interval / 60);
        var nextIntervalMinute = (i + interval / 60) % 1 ? 30 : 0;

        // Calculate the integer value based on the time slot
        var value;
        if (interval === "30") {
            value = (Math.floor(i) - startHour) * 2 + (startMinute === 30 ? 1 : 0) + 1;
        } else if (interval === "60") {
            value = (Math.floor(i) - startHour) + 23;
        }

        var checkbox = document.createElement("input");
        checkbox.type = "checkbox";
        checkbox.name = "TimeSlots";  // Use the name TimeSlots for all checkboxes
        checkbox.id = "time-" + prefix + "-" + pad(count);
        checkbox.value = value;

        var label = document.createElement("label");
        label.htmlFor = checkbox.id;
        label.appendChild(checkbox);
        label.appendChild(
            document.createTextNode(
                `${pad(Math.floor(i))}:${pad(startMinute)} - ${pad(nextIntervalHour)}:${pad(nextIntervalMinute)}`
            )
        );

        checkboxContainer.appendChild(label);
        checkboxContainer.appendChild(document.createElement("br"));

        // Add event listener to each checkbox to log the selected values
        checkbox.addEventListener('change', logSelectedValues);

        count++;
    }
});

function pad(n) {
    return n < 10 ? "0" + n : n;
}

function logSelectedValues() {
    var selectedValues = [];
    var checkboxes = document.querySelectorAll('input[name="TimeSlots"]:checked');
    checkboxes.forEach(function (checkbox) {
        selectedValues.push(parseInt(checkbox.value, 10));
    });
    console.log("Selected TimeSlots:", selectedValues);
}
