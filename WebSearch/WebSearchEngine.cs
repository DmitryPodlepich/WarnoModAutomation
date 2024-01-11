using CsQuery;
using JsonDatabase.DTO;
using System.Text.RegularExpressions;

namespace WebSearch
{
    public partial class WebSearchEngine
    {
        public delegate void Outputter(string data);
        public static event Outputter OnOutput;

        public static async Task FillDatabaseWithMilitaryTodayAsync(CancellationTokenSource cancellationTokenSource) 
        {
            string html = string.Empty;

            try
            {
                using (HttpClient httpClient = new())
                {
                    html = await httpClient.GetStringAsync("https://www.militarytoday.com/missiles.htm");

                    var dom = CQ.CreateDocument(html);

                    var unitsHrefs = dom["tbody > tr > td:nth-child(2) > font > a"].Elements.Select(e => e.Attributes["href"]).ToArray();

                    await Parallel.ForEachAsync(unitsHrefs, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount }, async (href, cancellationTokenSource) =>
                    {
                        OnOutput?.Invoke($"Scrapping from: {href}");

                        html = await httpClient.GetStringAsync(href, cancellationTokenSource);

                        var dom = CQ.CreateDocument(html);

                        if (!dom["h1"].Elements.Any())
                        {
                            OnOutput?.Invoke($"Cannot find h1 tag for unit name by href: {href}!");
                            return;
                        }

                        var unitName = dom["h1"].Elements.First().InnerText;

                        var trs = dom["#table52 > tbody tr"].Elements;

                        if (!trs.Any())
                        {
                            OnOutput?.Invoke($"Could not find any elements by selector: #table52 > tbody tr for unit: {unitName}");
                            return;
                        }

                        AmmoRangeDTO ammoRangeDTO = null;

                        foreach (var item in trs)
                        {
                            var innerDom = CQ.Create(item.InnerHTML);

                            if (!innerDom["td:nth-child(1) font"].Elements.Any() || !innerDom["td:nth-child(2) font"].Elements.Any())
                                continue;

                            var key = innerDom["td:nth-child(1) font"].Elements.First().InnerText;

                            var value = innerDom["td:nth-child(2) font"].Elements.First().InnerText;

                            if (key == "Range of fire")
                            {
                                var parserNumber = Numbersregex().Match(value).Value;

                                if (parserNumber.Length > 0)
                                {
                                    ammoRangeDTO = new AmmoRangeDTO
                                    {
                                        //WebName = unitName,
                                        FireRangeInMeters = int.Parse(parserNumber)
                                    };

                                    if (value.Contains("km", StringComparison.OrdinalIgnoreCase))
                                        ammoRangeDTO.FireRangeInMeters *= 1000;

                                    break;
                                }
                            }
                        }

                        if (ammoRangeDTO is not null)
                        {
                            JsonDatabase.JsonDatabase.AddOrUpdateAmmoRangeByWebName(ammoRangeDTO);
                        }
                    });
                }

                await JsonDatabase.JsonDatabase.SaveAsync();
            }
            catch (Exception ex)
            {
                OnOutput?.Invoke(ex.Message);
            }
        }

        [GeneratedRegex(@"\d+", RegexOptions.Compiled)]
        private static partial Regex Numbersregex();
    }
}
