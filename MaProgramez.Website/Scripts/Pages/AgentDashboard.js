$(document).ready(function () {
    if ($(document).find("#myChart").length > 0) {
        var data = {
            labels: graphicLabels,
            datasets: [
                {
                    fillColor: "rgba(151,187,205,0.2)",
                    strokeColor: "rgba(151,187,205,1)",
                    pointColor: "rgba(151,187,205,1)",
                    pointStrokeColor: "#fff",
                    pointHighlightFill: "#fff",
                    pointHighlightStroke: "rgba(151,187,205,1)",
                    data: graphicResult
                }
            ]
        };

        var ctx = document.getElementById("myChart").getContext("2d");
        var myLineChart = new Chart(ctx).Line(data, {
            responsive: true
        });
    }

    if ($(document).find("#myChart2").length > 0) {
        var data = {
            labels: graphicLabels,
            datasets: [
                {
                    fillColor: "rgba(220,220,220,0.2)",
                    strokeColor: "rgba(220,220,220,1)",
                    pointColor: "rgba(220,220,220,1)",
                    pointStrokeColor: "#fff",
                    pointHighlightFill: "#fff",
                    pointHighlightStroke: "rgba(220,220,220,1)",
                    data: graphicUserResult
                }
            ]
        };

        var ctx = document.getElementById("myChart2").getContext("2d");
        var myLineChart = new Chart(ctx).Line(data, {
            responsive: true
        });
    }

    if ($(document).find("#myPie").length > 0) {
        var pieData = [
                {
                    value: (typeof statesChartResult.Suppliers === 'undefined') ? 0 : statesChartResult.Suppliers,
                    color: "#F7464A",
                    highlight: "#FF5A5E",
                    label: suppliersText
                },
                {
                    value: (typeof statesChartResult.Services === 'undefined') ? 0 : statesChartResult.Services,
                    color: "#46BFBD",
                    highlight: "#5AD3D1",
                    label: servicesText
                }];

        var ctxPie = document.getElementById("myPie").getContext("2d");
        var myPieChart = new Chart(ctxPie).Doughnut(pieData, {
            responsive: true
        });
    }
});