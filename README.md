
This project is a Windows desktop app that downloads the latest image from EUMETSAT, decorates it a little
(with a calendar and weather forcast), and sets it as the Windows Desktop Background.

## Licence

This project is released under the [ISC Licence](LICENCE)

## Use

The app runs in the notification tray. Start it up and let it run. The backgroud is 
updated once an hour.

## Met Office API

The weather data comes from the UK Met Office data API. You will need to 
[register](https://register.metoffice.gov.uk/WaveRegistrationClient/public/register.do?service=datapoint)
for their "DataPoint" service, then create a file called "secrets.config" that contains:

	<?xml version="1.0" encoding="utf-8" ?>
	<appSettings>
		<add key="MetOfficeApiKey" value="[The API key from your registration]"/>
	</appSettings>

## Other Credits

I'm using weather icons from https://erikflowers.github.io/weather-icons/