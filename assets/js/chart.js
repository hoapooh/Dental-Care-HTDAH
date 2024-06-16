const ctx = document.getElementById("myChart");

let myChart = new Chart(ctx, {
	data: {
		datasets: [
			{
				type: "line",
				label: "Numbers of Votes",
				data: [12, 19, 5, 10, 2, 3, 43],
				borderWidth: 1,
				backgroundColor: ["rgba(255, 99, 132, 0.2)"],
				borderColor: ["rgb(255, 99, 132)"],
			},
		],
		labels: ["Red", "Blue", "Yellow", "Green", "Purple", "Orange", "Black"],
	},
});
