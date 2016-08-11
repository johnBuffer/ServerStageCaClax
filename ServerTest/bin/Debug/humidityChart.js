"use strict";

var dataHumidity = [];

var width = 800;
var height = 450;

var parseDate = d3.timeParse('%Y-%m-%d %H:%M:%S');
var dateFormat = d3.timeFormat('%m-%d %H:%M:%S');

var svgHumidity = d3.select("#chartHumidity").append("svg")
	.attr("width",  100 +"%")
	.attr("height", height+"px")
	.attr("id", "humidityChart")
	.attr('viewBox','0 0 '+Math.min(width,height) +' '+Math.min(width,height) )
	.attr('preserveAspectRatio','xMinYMin');
	
var margin = {top: 30, right: 30, bottom: 30, left: 30};    

var xHumidity = d3.scaleTime();
   /*.domain([parseDate(data[0].Date), parseDate(data[data.length - 1].Date)])
   .range([0, width-margin.left*2]);*/

var yHumidity = d3.scaleLinear()
	.domain([0, d3.max(dataHumidity, function(d) { return d.Value; })])
	.range([height-margin.top-margin.bottom, 0]);

/*var xAxis = d3.axisBottom(x)
    .tickFormat(d3.timeFormat("%Y-%m"))
	.ticks(5);

var yAxis = d3.axisLeft(yHumidity)
    .ticks(10);*/
	
var lineHumidity = d3.line()
    .x(function(d) { return xHumidity(parseDate(d.Date)); })
    .y(function(d) { return yHumidity(d.Value); });

var areaHumidity = d3.area()
    .x(function(d) { return xHumidity(parseDate(d.Date)); })
    .y1(function(d) { return yHumidity(d.Value); })
	.y0(height-margin.top-margin.bottom);
	
svgHumidity.selectAll("circle.nodes")
	.data(dataHumidity)
	.enter()
	.append("svg:circle")
	.attr("cx", function(d) { return xHumidity(parseDate(d.date));})
    .attr("cy", function(d) { return yHumidity(d.temp);})
    .attr("r", "5px")
    .attr("fill", "rgb(153, 204, 51)")
	.attr("transform", "translate(" + margin.left + "," + margin.top + ")");
	
svgHumidity.append("path")
	.datum(dataHumidity)
	.attr("class", "lineHum")
	.attr("stroke", "#83B4EE")
	.attr("stroke-width", 5)
	.attr("fill", "none")
	.attr("d", lineHumidity)
	.attr("transform", "translate(" + margin.left + "," + margin.top + ")");
	
svgHumidity.append("path")
	.datum(dataHumidity)
	.attr("class", "areaHumidity")
	.attr("stroke", "none")
	.attr("fill", "#B5D3F8")
	.attr("d", areaHumidity)
	.attr("transform", "translate(" + margin.left + "," + margin.top + ")");
	
/*var label = svgHumidity.selectAll(".label")
    .data(dataHumidity)
    .enter()
	.append("g")
    .attr("class", "labelHumidity")
    .attr("transform", function(d, i) { return "translate(" + xHumidity(parseDate(d.Date)) + "," + yHumidity(0) + ")"; });*/
	
svgHumidity.append("g")
  .attr("class", "axis--x_humidity")
  
svgHumidity.append("g")
  .attr("class", "axis--y_humidity")

label.append("text")
    .text(function(d) { return d.Date; });
	
function addHumidity(str) {
	var jsonObject = JSON.parse(str);
		
	var ValuePoint = jsonObject.Payload;
	
	if (dataHumidity.length != 0)
	{
		if (ValuePoint.Date != dataHumidity[dataHumidity.length-1].Date)
		{
			dataHumidity.push(ValuePoint);
			
			document.getElementById("currentHumidity").innerHTML = "current : "+ValuePoint.Value+"%";
		}
	}
	else
	{
		dataHumidity.push(ValuePoint);
	}
	
	if (dataHumidity.length > 250)
		dataHumidity.shift();
	
	xHumidity.domain([parseDate(dataHumidity[0].Date), parseDate(ValuePoint.Date)])
		.range([0, width-margin.left*2]);

	yHumidity.domain([0, d3.max(dataHumidity, function(d) { return d.Value; })]);
		
	var line = d3.line()
		.x(function(d) { return xHumidity(parseDate(d.Date)); })
		.y(function(d) { return yHumidity(d.Value); });
	
	//d3.select("path").transition().duration(750);
		
	d3.select(".lineHum")
		//.datum(data)
		.attr("d", lineHumidity.curve(d3.curveCardinal.tension(0.5)));
		
	d3.select(".areaHumidity")
		//.datum(data)
		.attr("d", areaHumidity.curve(d3.curveCardinal.tension(0.5)));
		
	svgHumidity.select(".axis--x_humidity")
		.transition()
		.call(d3.axisBottom(xHumidity).ticks(d3.timeMinute, 5).tickFormat(d3.timeFormat("%H:%M:%S")))
		.attr("transform", "translate(" + margin.left + "," + (height-margin.bottom) + ")");
	
	svgHumidity.select(".axis--y_humidity")
		.transition()
		.call(d3.axisLeft(yHumidity))
		.attr("transform", "translate(" + margin.left + "," + margin.top + ")");
}
	