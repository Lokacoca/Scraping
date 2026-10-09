# Bus Display – Live Transit Board

<img width="1554" height="941" alt="image" src="https://github.com/user-attachments/assets/7ffd3cb5-0926-416a-995d-4cdd2dc9d41d" />


A live departure board for a high school's bus stop in Örebro, built in C# with **HTML Agility Pack**. It scrapes departure data from Länstrafiken's public stop page and turns it into a polished full-screen display, which was used on the school's screens.

## Why I built it

The main goal was to **experiment with web scraping**. I tried two different tools:

| Tool | Experience |
|---|---|
| **Selenium** | Works, but drives a full browser. Consumes unnecessary compute and takes longer for simple tasks. not fit for this project |
| **HTML Agility Pack** | Much better for my purpose: it parses the page's HTML directly, is lightweight, and the departure data is already in the HTML |

HTML Agility Pack was the clear winner, so the final version uses it.

## How it works

1. Opening the page sends a request to a small ASP.NET Core server.
2. The server downloads the stop page and parses it with HTML Agility Pack, selecting the departure rows with XPath.
3. For each departure it extracts the line, destination, platform, departure time, minutes left, delay and whether the data is real-time.
4. The next departure goes into a large "Next bus" card and up to eight more are listed below it.
5. The values are filled into an HTML template (`index.html`) and sent to the browser.

### Keeping it live

The page **reloads itself every 30 seconds**, which makes the server run the scraping again. I chose this simple solution on purpose: instead of building extra code that fetches and updates the data in the background every second, the browser just refreshes and the existing code runs again. It keeps the code short and is fast enough for a bus board.

## Design

Since it ran on the school's screens, I focused on making it visually pleasing and easy to read from a distance:

- A large "Next bus" card to display the most relevant time, with the other departures in a list underneath
- A live clock and date that update every second
- A bus that drives along a road line at the top of the screen
- A real-time indicator on departures that use live data
- Delayed departures show the original time crossed out
- Departures leaving within 3 minutes are highlighted

## Tech stack

| | |
|---|---|
| Language | C# |
| Framework | .NET 9, ASP.NET Core (minimal API) |
| Scraping | HTML Agility Pack |
| Front end | HTML, CSS, a little JavaScript |

## Project structure

| File | Purpose |
|---|---|
| `BusDisplay/Program.cs` | Scrapes and parses the stop page, fills in the template, serves the page |
| `BusDisplay/index.html` | Page template with placeholders, clock and 30 second refresh |
| `BusDisplay/style.css` | Layout, colours and animations |

## Getting started

1. Install the [.NET 9 SDK](https://dotnet.microsoft.com/download).
2. Clone this repository.
3. Run the project:

```bash
cd BusDisplay
dotnet run
```

4. Open http://localhost:5080 in a browser.

## Limitations

- The scraper depends on the structure of Länstrafiken's page, so it needs updating if they change their HTML.
- The stop is set directly in the code.
- The data is scraped on every page load, so each open screen triggers a new request every 30 seconds.

## Credits

Departure data comes from the public stop page of Länstrafiken Örebro. This is a personal learning project and is not affiliated with Länstrafiken.
