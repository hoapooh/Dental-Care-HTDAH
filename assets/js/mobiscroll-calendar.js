let date = new Date();
let formattedDate =
	date.getFullYear() +
	"-" +
	String(date.getMonth() + 1).padStart(2, "0") +
	"-" +
	String(date.getDate()).padStart(2, "0");

let futureDate = new Date(date.getTime()); // Create a copy of the original date
futureDate.setMonth(futureDate.getMonth() + 1);
let futureFormattedDate =
	futureDate.getFullYear() +
	"-" +
	String(futureDate.getMonth() + 1).padStart(2, "0") +
	"-" +
	String(futureDate.getDate()).padStart(2, "0");

mobiscroll.setOptions({
	theme: "ios",
	themeVariant: "light",
});

mobiscroll.datepicker("#demo-single-select-date", {
	controls: ["calendar"],
	selectMultiple: false,
});

mobiscroll.datepicker("#demo-single-select-datetime", {
	controls: ["calendar", "time"],
	selectMultiple: false,
});

mobiscroll.datepicker("#demo-single-select-timegrid", {
	controls: ["calendar", "timegrid"],
	selectMultiple: false,
});

// Tập trung vào đây
mobiscroll.datepicker("#demo-single-day", {
	controls: ["calendar", "timegrid"],
	display: "inline",
	touchUi: true,
	selectMultiple: false,
	valid: [
		{ start: "2024-06-14T02:00", end: "2024-06-14T02:00" },
		{ start: "2024-06-14T09:00", end: "2024-06-14T09:00" },
		{ start: "2024-06-19T02:00", end: "2024-06-19T02:00" },
		{ start: "2024-06-19T09:00", end: "2024-06-19T09:00" },
	],
	// {
	// 	start: "02:00",
	// 	end: "06:30",
	// 	recurring: {
	// 		repeat: "weekly",
	// 		weekDays: "MO, WE",
	// 		interval: 1,
	// 	},
	// },
	// {
	// 	start: "13:00",
	// 	end: "14:00",
	// 	recurring: {
	// 		repeat: "daily",
	// 	},
	// },
});

mobiscroll.datepicker("#demo-max-days", {
	controls: ["calendar"],
	display: "inline",
	selectMultiple: true,
	selectMax: 5,
	headerText: "Pick up to 5 days",
});

// Tâp trung vào đây
mobiscroll.datepicker("#demo-counter", {
	controls: ["calendar"],
	display: "inline",
	selectMultiple: true,
	selectCounter: true,
	valid: [
		{
			start: formattedDate,
			end: futureFormattedDate,
		},
	],
});
