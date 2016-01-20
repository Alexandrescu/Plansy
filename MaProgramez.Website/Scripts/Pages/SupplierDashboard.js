$(document).ready(function () {
    var data = {
        labels: graphicLabels,
        datasets: [
            {
                label: activeRequestsText,
                fillColor: "rgba(220,220,220,0.2)",
                strokeColor: "rgba(220,220,220,1)",
                pointColor: "rgba(220,220,220,1)",
                pointStrokeColor: "#fff",
                pointHighlightFill: "#fff",
                pointHighlightStroke: "rgba(220,220,220,1)",
                data: graphicResult
            },
            {
                label: myOffersText,
                fillColor: "rgba(151,187,205,0.2)",
                strokeColor: "rgba(151,187,205,1)",
                pointColor: "rgba(151,187,205,1)",
                pointStrokeColor: "#fff",
                pointHighlightFill: "#fff",
                pointHighlightStroke: "rgba(151,187,205,1)",
                data: graphicUserResult
            }
        ]
    };

    var pieData = [
    {
        value: (typeof statesChartResult.Offerted === 'undefined') ? 0 : statesChartResult.Offerted,
        color: "#F7464A",
        highlight: "#FF5A5E",
        label: offeredText
    },
    {
        value: (typeof statesChartResult.OfferAccepted === 'undefined') ? 0 : statesChartResult.OfferAccepted,
        color: "#46BFBD",
        highlight: "#5AD3D1",
        label: offerAcceptedText
    },
    {
        value: (typeof statesChartResult.Completed === 'undefined') ? 0 : statesChartResult.Completed,
        color: "#FDB45C",
        highlight: "#FFC870",
        label: completedText
    }
    ]

    var ctx = document.getElementById("myChart").getContext("2d");
    var ctxPie = document.getElementById("myPie").getContext("2d");
    var myLineChart = new Chart(ctx).Line(data, {
        responsive: true
    });

    var myPieChart = new Chart(ctxPie).Doughnut(pieData, {
        responsive: true
    });
});