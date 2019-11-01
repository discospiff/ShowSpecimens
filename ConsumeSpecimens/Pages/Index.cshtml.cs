using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using QuickType;

namespace ConsumeSpecimens.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            using (var webClient = new WebClient())
            {

                String key = System.IO.File.ReadAllText("WeatherAPIKey.txt");
                String weatherString = webClient.DownloadString("https://api.weatherbit.io/v2.0/current?&city=Cincinnati&country=USA&key=" + key);
                QuickTypeWeather.Welcome welcomeWeather = QuickTypeWeather.Welcome.FromJson(weatherString);
                long precip = 0;
                foreach(QuickTypeWeather.Datum weather in welcomeWeather.Data)
                {
                    precip = weather.Precip;
                }
                if (precip < 1)
                {
                    ViewData["WeatherMessage"] = "Water your plants!";
                }
                IDictionary<long, QuickTypePlants.Welcome> allPlants = new Dictionary<long, QuickTypePlants.Welcome>();
                String plantsJSON = webClient.DownloadString("http://plantplaces.com/perl/mobile/viewplantsjsonarray.pl?WetTolerant=on");
                QuickTypePlants.Welcome[] welcomePlants = QuickTypePlants.Welcome.FromJson(plantsJSON);
                foreach(QuickTypePlants.Welcome plant in welcomePlants)
                {
                    allPlants.Add(plant.Id, plant);
                }
                String jsonString = webClient.DownloadString("https://www.plantplaces.com/perl/mobile/viewspecimenlocations.pl?Lat=39.14455075&Lng=-84.5093939666667&Range=0.5&Source=location&Version=2");
                JSchema schema = JSchema.Parse(System.IO.File.ReadAllText("SpecimenSchema.json"));
                JObject jsonObject = JObject.Parse(jsonString);
                IList<string> validationEvents = new List<string>();
                if (jsonObject.IsValid(schema, out validationEvents))
                {
                    Welcome welcome = Welcome.FromJson(jsonString);
                    List<Specimen> specimens = welcome.Specimens;
                    List<Specimen> waterMeSpecimens = new List<Specimen>();
                    foreach(Specimen specimen in specimens)
                    {
                        if (allPlants.ContainsKey(specimen.PlantId)) 
                        {
                            waterMeSpecimens.Add(specimen);
                        }
                     }
                    ViewData["Specimens"] = waterMeSpecimens;
                }
                else
                {
                    foreach (string evt in validationEvents)
                    {
                        Console.WriteLine(evt);
                    }
                    ViewData["Specimens"] = new List<Specimen>();
                }
            }
        }
    }
}
