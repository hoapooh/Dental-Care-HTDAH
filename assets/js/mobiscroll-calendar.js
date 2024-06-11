let date = new Date();
let formattedDate =
	date.getFullYear() +
	"-" +
	String(date.getMonth() + 1).padStart(2, "0") +
	"-" +
	String(date.getDate()).padStart(2, "0");

let futureDate = new Date(date.getTime()); // Create a copy of the original date
futureDate.setFullYear(futureDate.getFullYear() + 20);
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

mobiscroll.datepicker("#demo-multi-day", {
	controls: ["calendar"],
	display: "inline",
	selectMultiple: true,
});

mobiscroll.datepicker("#demo-max-days", {
	controls: ["calendar"],
	display: "inline",
	selectMultiple: true,
	selectMax: 5,
	headerText: "Pick up to 5 days",
});

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
