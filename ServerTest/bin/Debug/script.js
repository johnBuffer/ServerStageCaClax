"use strict";

window.onload = function() {
	getTargetTemperature();
}

function checkDeviceSatus() {
	getRelayStatus();
	getLastPing();
}

function checkData() {
	getTemperature();
	getHumidity();
}

setInterval(checkDeviceSatus, 2000);
setInterval(checkData, 10000);

var currentPingStatus = "--";

function updateToggle()
{
	var toggle = document.getElementById("toggleCheckBox");
	if (toggle.checked)
	{
		toggle.checked = false;
		document.getElementById("relayToggle").style.background = "url('toggleOFF.png')";
	}
	else
	{
		toggle.checked = true;
		document.getElementById("relayToggle").style.background = "url('toggleON.png')";
	}
}

function toggleRelay() {
	var toggle = document.getElementById("toggleCheckBox");
	if (toggle.checked)
	{
		addAction('RelayOFF');
	}
	else
	{
		addAction('RelayON');
	}
}

function toggleThermostat() {
	var toggle = document.getElementById("toogleThermostat");
	
	if (!toggle.checked)
	{
		setTargetTemperature(10);
		document.getElementById("temperatureRange").disabled = true;
	}
	else
	{
		setTargetTemperature(30);
		document.getElementById("temperatureRange").disabled = false;
	}
}

function updatePing(json) {
	var jsonObject = JSON.parse(json);
	document.getElementById("lastPingDisplay").innerHTML = jsonObject.Payload;
	
	var elapsed = d3.timeSecond.count(d3.timeSecond(parseDate(jsonObject.Payload)), new Date);
	
	if (elapsed < 10)
	{
		if (currentPingStatus != "online")
		{
			document.getElementById("pingStatus").style.background = "url('ledgreen.png')";
			document.getElementById("statusDisplay").innerHTML = "online";
			currentPingStatus = "online";
			var text = document.getElementById("logArea");
			text.value = dateFormat(new Date) + " online\n"+text.value;
		}
	}
	else if (elapsed < 20)
	{
		if (currentPingStatus != "waiting")
		{
			document.getElementById("pingStatus").style.background = "url('ledorange.png')";
			document.getElementById("statusDisplay").innerHTML = "waiting module";
			currentPingStatus = "waiting";
			//var text = document.getElementById("logArea");
			//text.value = dateFormat(new Date) + " waiting\n"+text.value;
		}
	}
	else
	{
		if (currentPingStatus != "error")
		{
			document.getElementById("pingStatus").style.background = "url('ledred.png')";
			document.getElementById("statusDisplay").innerHTML = "connection error";
			currentPingStatus = "error";
			var text = document.getElementById("logArea");
			text.value = dateFormat(new Date) + " error\n"+text.value;
		}
	}
		
}

function showSVG() {
	var chart = document.getElementById("temperatureChart");
	var isShown = chart.style.display;
	
	if (isShown == "none")
	{
		chart.style.display = "inline-block";
	}
	else
	{
		chart.style.display = "none";
	}
}

function showSVG_h() {
	var chart = document.getElementById("humidityChart");
	var isShown = chart.style.display;
	
	if (isShown == "none")
	{
		chart.style.display = "inline-block";
	}
	else
	{
		chart.style.display = "none";
	}
}

function addAction(type) {
	var url = "http://79.142.244.131:65201/AddAction?unitid=1&action="+type;
	httpGet(url, getCallback);
}

function setTargetTemperature(value) {
	var url = "http://79.142.244.131:65201/SetTargetTemperature?unitid=1&value="+value;
	httpGet(url, getCallback);
}

function getRelayStatus() {
	var url = "http://79.142.244.131:65201/GetRelayStatus?unitid=1";
	httpGet(url, setRelayToggle);
}

function getTemperature() {
	var url = "http://79.142.244.131:65201/GetLastTemperature?unitid=1";
	httpGet(url, addTemperature);
}

function getHumidity() {
	var url = "http://79.142.244.131:65201/GetLastHumidity?unitid=1";
	httpGet(url, addHumidity);
}

function getTargetTemperature() {
	var url = "http://79.142.244.131:65201/GetTargetTemperature?unitid=1";
	httpGet(url, jsonToValue);
}

function jsonToValue(json) {
	var jsonObject = JSON.parse(json);
	updateTemperature(jsonObject.Payload);
}

function getLastPing() {
	var url = "http://79.142.244.131:65201/GetLastPing?unitid=1";
	httpGet(url, updatePing);
}

function setRelayToggle(json) {
	var jsonObject = JSON.parse(json);
	
	var toggle = document.getElementById("toggleCheckBox");
	
	if (jsonObject.Payload == "ON")
		toggle.checked = false;
	else if (jsonObject.Payload == "OFF")
		toggle.checked = true;
	
	updateToggle();
}

function updateTemperature(value) {
	
	if (value > 10)
	{
		document.getElementById("temperatureDisplay").innerHTML = value+"&degC";
		document.getElementById("temperatureRange").value = value;
	}
	else
	{
		document.getElementById("toogleThermostat").checked  = false;
		document.getElementById("temperatureRange").disabled = true;
	}
	setTargetTemperature(value);
}

function getCallback(response) {
	var jsonObject = JSON.parse(response);
	console.log(jsonObject.Payload);
}

function httpGet(url, getCallback) {
	var request = new XMLHttpRequest();
	request.onreadystatechange = function() { 
        if (request.readyState == 4 && request.status == 200)
            getCallback(request.responseText);
    }
	request.open("GET", url, true);
	request.send(null);
}