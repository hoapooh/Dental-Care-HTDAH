const ctx = document.getElementById("myChart");

let myChart = new Chart(ctx, {
    data: {
        datasets: [
            {
                type: "line",
                label: "Numbers of Votes",
                data: [12, 19, 5, 10, 2, 3],
                borderWidth: 1,
                backgroundColor: ["rgba(255, 99, 132, 0.2)"],
                borderColor: ["rgb(255, 99, 132)"],
            },
            {
                type: "bar",
                label: "Numbers of Wins",
                data: [32, 19, 25, 30, 42, 33],
                borderWidth: 1,
                backgroundColor: ["rgba(75, 192, 192, 0.2)"],
            },
        ],
        labels: ["Red", "Blue", "Yellow", "Green", "Purple", "Orange"],
    },
});
