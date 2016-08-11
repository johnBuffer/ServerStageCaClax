"use strict";

var data = [];

console.log("OK D3js");

var width = 800;
var height = 450;

var parseDate = d3.timeParse('%Y-%m-%d %H:%M:%S');
var dateFormat = d3.timeFormat('%m-%d %H:%M:%S');

var svg = d3.select("#chart").append("svg")
	.attr("width",  100 +"%")
	.attr("height", height+"px")
	.attr("id", "temperatureChart")
	.attr('viewBox','0 0 '+Math.min(width,height) +' '+Math.min(width,height) )
	.attr('preserveAspectRatio','xMinYMin');
	
var margin = {top: 30, right: 30, bottom: 30, left: 30};    

var x = d3.scaleTime();
   /*.domain([parseDate(data[0].Date), parseDate(data[data.length - 1].Date)])
   .range([0, width-margin.left*2]);*/

var y = d3.scaleLinear()
	.domain([0, d3.max(data, function(d) { return d.Value; })])
	.range([height-margin.top-margin.bottom, 0]);

var xAxis = d3.axisBottom(x)
    .tickFormat(d3.timeFormat("%Y-%m"))
	.ticks(5);

var yAxis = d3.axisLeft(y)
    .ticks(10);
	
var line = d3.line()
    .x(function(d) { return x(parseDate(d.Date)); })
    .y(function(d) { return y(d.Value); });

var area1 = d3.area()
    .x(function(d) { return x(parseDate(d.Date)); })
    .y1(function(d) { return y(d.Value); })
	.y0(height-margin.top-margin.bottom);
	
svg.selectAll("circle.nodes")
	.data(data)
	.enter()
	.append("svg:circle")
	.attr("cx", function(d) { return x(parseDate(d.date));})
    .attr("cy", function(d) { return y(d.temp);})
    .attr("r", "5px")
    .attr("fill", "rgb(153, 204, 51)")
	.attr("transform", "translate(" + margin.left + "," + margin.top + ")");
	
svg.append("path")
	.datum(data)
	.attr("class", "line")
	.attr("stroke", "rgb(153, 204, 51)")
	.attr("stroke-width", 5)
	.attr("fill", "none")
	.attr("d", line)
	.attr("transform", "translate(" + margin.left + "," + margin.top + ")");
	
svg.append("path")
	.datum(data)
	.attr("class", "area")
	.attr("stroke", "none")
	.attr("fill", "rgb(221, 239, 187)")
	.attr("d", area1)
	.attr("transform", "translate(" + margin.left + "," + margin.top + ")");
	
var label = svg.selectAll(".label")
    .data(data)
    .enter()
	.append("g")
    .attr("class", "label")
    .attr("transform", function(d, i) { return "translate(" + x(parseDate(d.Date)) + "," + y(0) + ")"; });
	
svg.append("g")
  .attr("class", "axis--x");
  
svg.append("g")
  .attr("class", "axis--y");

label.append("text")
    .text(function(d) { return d.Date; });
	
function addTemperature(str) {
	var jsonObject = JSON.parse(str);
		
	var ValuePoint = jsonObject.Payload;
	
	if (data.length != 0)
	{
		if (ValuePoint.Date != data[data.length-1].Date)
		{
			data.push(ValuePoint);
			
			document.getElementById("currentTemp").innerHTML = "current : "+ValuePoint.Value+"&degC";
		}
	}
	else
	{
		data.push(ValuePoint);
	}
	
	if (data.length > 250)
		data.shift();
	
	x.domain([parseDate(data[0].Date), parseDate(ValuePoint.Date)])
		.range([0, width-margin.left*2]);

	y.domain([0, d3.max(data, function(d) { return d.Value; })]);
		
	var line = d3.line()
		.x(function(d) { return x(parseDate(d.Date)); })
		.y(function(d) { return y(d.Value); });
	
	//d3.select("path").transition().duration(750);
		
	d3.select(".line")
		//.datum(data)
		.attr("d", line.curve(d3.curveCardinal.tension(0.5)));
		
	d3.select(".area")
		//.datum(data)
		.attr("d", area1.curve(d3.curveCardinal.tension(0.5)));
		
	svg.select(".axis--x")
		.transition()
		.call(d3.axisBottom(x).ticks(d3.timeMinute, 5).tickFormat(d3.timeFormat("%H:%M")))
		.attr("transform", "translate(" + margin.left + "," + (height-margin.bottom) + ")");
	
	svg.select(".axis--y")
		.transition()
		.call(d3.axisLeft(y))
		.attr("transform", "translate(" + margin.left + "," + margin.top + ")");
}
	