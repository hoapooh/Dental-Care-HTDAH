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

document.addEventListener("DOMContentLoaded", function () {
    console.log("DOM fully loaded and parsed");
    // Check if mobiscroll is loaded
    if (typeof mobiscroll !== 'undefined') {
        console.log("Mobiscroll is loaded");

        mobiscroll.setOptions({
            theme: "ios",
            themeVariant: "light",
            locale: mobiscroll.localeVi,
            firstDay: 1 // Set Monday as the first day of the week
        });

        mobiscroll.datepicker('#demo-counter', {
            controls: ['calendar'],
            display: 'inline',
            selectMultiple: false, // Disable multiple selection
            selectCounter: false, // Disable the count of selected dates
            valid: [{
                start: formattedDate,
                end: futureFormattedDate
            }],

            onInit: function (event, inst) {
                console.log("onInit event fired"); // Debug log
                setTimeout(addWeekButtons, 2000); // Ensure buttons are added after rendering
            },
            onPageChange: function (event, inst) {
                console.log("onPageChange event fired"); // Debug log
                setTimeout(addWeekButtons, 2000); // Ensure buttons are added after page change
            }
        });
    } else {
        console.error("Mobiscroll is not loaded");
    }
});

function addText_FirstRow() {
    const calendarContainer = document.querySelector('#demo-counter');
    console.log("Calendar container: ", calendarContainer);
    const dayElements = calendarContainer.querySelectorAll('.mbsc-calendar-week-day');
    console.log("WDay elements found: ", dayElements.length);

    if (dayElements.length === 0) {
        console.error("No Weekday elements found.");
        return;
    }
    if (dayElements.length != 0) {
        dayElements.forEach((day, index) => {
            console.log(day);
        });
    }
    //------------------------------------------
    let weeks = [];
    let currentWeek = [];

    dayElements.forEach((day, index) => {
        currentWeek.push(day);

        // Assuming a week row has 7 days
        if ((index + 1) % 7 === 0) {
            weeks.push(currentWeek);
            currentWeek = [];
        }
    });

    if (currentWeek.length > 0) {
        weeks.push(currentWeek); // Add the last week if there are remaining days
    }

    console.log("Weeks found: ", weeks.length);

    // Clear existing buttons
    const existingButtons = document.querySelectorAll('.week-buttonTitile');
    existingButtons.forEach(button => button.remove());

    // Add a button for each week
    weeks.forEach((week, index) => {
        console.log(`Processing week ${index + 1}`);

        const weekButton = document.createElement('button');
        weekButton.className = 'week-buttonTitile';
        weekButton.innerText = `Tạo lịch khám`;
        weekButton.onclick = function () {
            submitWeek(week);
        };

        // Create a container div for the button and append it next to the first day of the week
        const buttonContainer = document.createElement('div');
        buttonContainer.className = 'week-button-container';
        buttonContainer.style.marginTop = '2px'; // Adjust as necessary
        buttonContainer.style.visibility = 'hidden';
        buttonContainer.appendChild(weekButton);

        // Append the button container after the first day of the week
        week[0].parentNode.appendChild(buttonContainer);
    });
}

function addWeekButtons() {
    addText_FirstRow();
    const calendarContainer = document.querySelector('#demo-counter');
    console.log("Calendar container: ", calendarContainer);
    //--------------------------------
    const dayElements = calendarContainer.querySelectorAll('.mbsc-calendar-day');
    console.log("Day elements found: ", dayElements.length);

    if (dayElements.length === 0) {
        console.error("No day elements found.");
        return;
    }

    let weeks = [];
    let currentWeek = [];

    dayElements.forEach((day, index) => {
        currentWeek.push(day);

        // Assuming a week row has 7 days
        if ((index + 1) % 7 === 0) {
            weeks.push(currentWeek);
            currentWeek = [];
        }
    });

    if (currentWeek.length > 0) {
        weeks.push(currentWeek); // Add the last week if there are remaining days
    }

    console.log("Weeks found: ", weeks.length);

    // Clear existing buttons
    const existingButtons = document.querySelectorAll('.week-button');
    existingButtons.forEach(button => button.remove());

    // Add a button for each week
    weeks.forEach((week, index) => {
        const weekDates = week.map(dayElement => {
            const ariaLabel = dayElement.querySelector('[aria-label]').getAttribute('aria-label');
            return parseDateFromAriaLabel(ariaLabel);
        }).filter(date => date !== null);
        // Log the weekDates for debugging
        console.log(`Week ${index + 1} dates:`, weekDates);
        // Check if at least one date in the week falls within the specified range
        const isWithinRange = weekDates.some(date => new Date(date) >= new Date(formattedDate) && new Date(date) <= new Date(futureFormattedDate));

        console.log(`Processing week ${index + 1} - Within range: ${isWithinRange}`);

        const weekButton = document.createElement('button');
        weekButton.className = 'week-button';
        weekButton.innerText = `Tạo lịch khám`;
        weekButton.onclick = function () {
            submitWeek(week);
        };

        // Create a container div for the button and append it next to the first day of the week
        const buttonContainer = document.createElement('div');
        buttonContainer.className = 'week-button-container';
        buttonContainer.style.marginTop = '2px'; // Adjust as necessary
        buttonContainer.style.visibility = (isWithinRange == true) ? 'visible' : 'hidden';
        buttonContainer.appendChild(weekButton);

        // Append the button container after the first day of the week
        week[0].parentNode.appendChild(buttonContainer);
    });
}

function submitWeek(week) {
    const dates = week.map(dayElement => {
        const ariaLabel = dayElement.querySelector('[aria-label]').getAttribute('aria-label');
        console.log('Aria-label: ', ariaLabel);
        return parseDateFromAriaLabel(ariaLabel);
    }).filter(date => date !== null);

    if (dates.length === 0) {
        console.error("No valid dates found in the week.");
        return;
    }

    console.log('Submitting week:', dates);

    // Set the hidden input value
    document.getElementById('selectedDates').value = dates.join(', ');

    // Submit the form
    document.getElementById('weekScheduleForm').submit();
}

function parseDateFromAriaLabel(ariaLabel) {
    const monthMap = {
        "Tháng Một": "01",
        "Tháng Hai": "02",
        "Tháng Ba": "03",
        "Tháng Tư": "04",
        "Tháng Năm": "05",
        "Tháng Sáu": "06",
        "Tháng Bảy": "07",
        "Tháng Tám": "08",
        "Tháng Chín": "09",
        "Tháng Mười": "10",
        "Tháng Mười Một": "11",
        "Tháng Mười Hai": "12"
    };

    // Adjusted regex to match the format "Chủ Nhật, Tháng Bảy 7, 2024"
    const datePattern = /Tháng (Một|Hai|Ba|Tư|Năm|Sáu|Bảy|Tám|Chín|Mười|Mười Một|Mười Hai) (\d+), (\d{4})/;
    const match = ariaLabel.match(datePattern);

    if (match) {
        const monthString = `Tháng ${match[1]}`;
        const day = String(match[2]).padStart(2, '0');
        const year = match[3];
        const month = monthMap[monthString];

        if (month) {
            return `${year}-${month}-${day}`;
        } else {
            console.error("Failed to parse month from aria-label:", ariaLabel);
            return null;
        }
    } else {
        console.error("Failed to parse date from aria-label:", ariaLabel);
        return null;
    }
}

// Test the function
const ariaLabels = [
    "Thứ Hai, Tháng Bảy 1, 2024",
    "Thứ Năm, Tháng Bảy 4, 2024",
    "Thứ Sáu, Tháng Bảy 5, 2024",
    "Thứ Bảy, Tháng Bảy 6, 2024",
    "Chủ Nhật, Tháng Bảy 7, 2024",
    "Thứ Năm, Tháng Bảy 12, 2024",
    "Thứ Sáu, Tháng Bảy 13, 2024",
    "Chủ Nhật, Tháng Bảy 14, 2024"
];

ariaLabels.forEach(label => {
    const formattedDate = parseDateFromAriaLabel(label);
    console.log(`${label} => ${formattedDate}`);
});
