using HtmlAgilityPack;

var app = WebApplication.Create(args);

// Startsidan: scrapar hållplatsen och fyller i index.html med avgångarna.
// Sidan laddar om sig själv var 30:e sekund, så scrapingen körs om varje gång.
app.MapGet("/", () =>
{
    var doc = new HtmlWeb().Load("https://www.lanstrafiken.se/hallplats/18993380/");
    var stopName = Text(doc.DocumentNode, "//h1");

    var hero = "";
    var rows = "";

    // Varje avgång är en <tr> med klassen "result-row" i Länstrafikens tabell.
    var departures = doc.DocumentNode.SelectNodes("//tr[contains(@class,'result-row')]")?.Take(9) ?? [];

    foreach (var row in departures)
    {
        var line = Text(row, ".//span[@class='info-container']/span").Replace("linje", "").Trim();
        var destination = Text(row, ".//span[@class='line-container']/span[not(@class)]");
        var platform = Text(row, ".//td[contains(@class,'stop-point')]/text()[normalize-space()]");
        var time = row.SelectSingleNode(".//div[@data-departure-time]").GetAttributeValue("data-departure-time", "");
        var originalTime = Text(row, ".//td[@class='departure']//s/span");
        var countdown = Text(row, ".//span[@class='departure-time']").Replace("Kl. ", "");
        var isLive = row.SelectSingleNode(".//i[contains(@class,'live-icon')]") != null;

        var live = isLive ? "<span class=\"live-dot\"></span>" : "";
        var late = originalTime != "" ? $"<s>{originalTime}</s>" : "";
        var soon = countdown == "Nu" || countdown.StartsWith('<') || (int.TryParse(countdown.Replace(" min", ""), out var min) && min <= 3) ? "soon" : "";

        // Första avgången visas i det stora kortet, resten i listan.
        if (hero == "")
        {
            hero = $"""
                <div class="hero-main">
                  <div class="hero-line">{line}</div>
                  <div>
                    <div class="hero-dest">{destination}</div>
                    <div class="hero-meta"><span>Läge {platform}</span><span>Avgår {time}</span>{(isLive ? "<span class=\"live-pill\"><span class=\"live-dot\"></span>Realtid</span>" : "")}</div>
                  </div>
                </div>
                <div class="hero-count {soon}">{countdown}</div>
                """;
        }
        else
        {
            rows += $"""
                <div class="row">
                  <span class="badge">{line}</span>
                  <span class="dest">{live}{destination}</span>
                  <span class="platform">{platform}</span>
                  <span class="time">{time}{late}</span>
                  <span class="count {soon}">{countdown}</span>
                </div>
                """;
        }
    }

    var html = File.ReadAllText("index.html")
        .Replace("{{HÅLLPLATS}}", stopName)
        .Replace("{{NÄSTA}}", hero)
        .Replace("{{AVGÅNGAR}}", rows);

    return Results.Content(html, "text/html; charset=utf-8");
});

// Skickar CSS-filen till webbläsaren.
app.MapGet("/style.css", () => Results.File(Path.GetFullPath("style.css"), "text/css"));

app.Run("http://localhost:5080");

// Hämtar texten från första elementet som matchar XPath och gör om t.ex. &#229; till å.
static string Text(HtmlNode node, string xpath) =>
    HtmlEntity.DeEntitize(node.SelectSingleNode(xpath)?.InnerText ?? "").Trim();
