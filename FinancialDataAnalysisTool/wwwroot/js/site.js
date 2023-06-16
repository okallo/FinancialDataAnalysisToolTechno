// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Get the data from the server
var graphData = Html.Raw(Json.Serialize(Model));

// Create the graph
var graphContainer = document.getElementById('graphContainer');
var graph = new ScottPlot.Plot(graphContainer);
graph.PlotSignal(graphData.data, graphData.sampleRate);

// Optional: Customize the graph properties
graph.Title = 'My Graph';
graph.XLabel = 'X-Axis';
graph.YLabel = 'Y-Axis';

// Render the graph
graph.Render();
