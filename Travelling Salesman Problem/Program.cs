using System;
using System.Collections.Generic;
using SharpPDDL;

namespace Travelling_Salesman_Problem
{
    class Program
    {
        public class City
        {
            public readonly string Name;
            public readonly int PostalCode;
            public bool Visited = false;
            public bool SalesmanHere = false;

            public City (string Name)
            {
                this.Name = Name;
                this.PostalCode = CitiesAPI.PostalCodeAPI(Name);
            }
        }

        static void PrintPlan(List<List<string>> plan)
        {
            for (int i = 0; i != plan.Count; i++)
                Console.WriteLine(plan[i][0] + plan[i][1] + plan[i][2]);
        }

        static void Main(string[] args)
        {
            List<City> Cities = new List<City>
            {
                new City("Koszalin"),
                new City("Gniezno"),
                new City("Kraków"),
                new City("Płock"),
                new City("Poznań"),
                new City("Warszawa"),
                new City("Lublin")
            };
            Cities[0].SalesmanHere = true;

            DomeinPDDL TSP = new DomeinPDDL("TSP");

            ActionPDDL Travel = new ActionPDDL("Travel");
            City From = null; //Salesman leaves "From" city,
            City To = null; //and goes to "To" city.

            Travel.AddPartOfActionSententia(ref To, "Go to {0}.", T => T.Name);

            Travel.AddPrecondiction( // From.SalesmanHere == true
                "Salesnam is in FROM city now",
                ref From,
                F => F.SalesmanHere);

            //Salesman visit city only one time
            Travel.AddPrecondiction( // To.Visiting == false
                "Salesnam havent been in TO city",
                ref To,
                F => !F.Visited);

            Travel.AddEffect( // From.SalesmanHere = false
                "Salesman leaves city",
                ref From,
                F => F.SalesmanHere,
                false);

            Travel.AddEffect( // To.SalesmanHere = true
                "Salesman arrives new city",
                ref To,
                T => T.SalesmanHere,
                true);

            Travel.AddEffect( // To.Visited = true
                "Salesman visit new city",
                ref To,
                T => T.Visited,
                true);

            Travel.DefineActionCost(ref From, ref To, (F, T) => CitiesAPI.DistanceAPI(F.PostalCode, T.PostalCode));

            TSP.AddAction(Travel);

            GoalPDDL VisitAll = new GoalPDDL("Visit all cities");
            foreach (City city in Cities)
            {
                TSP.domainObjects.Add(city);
                if (city.Name == "Koszalin")
                    VisitAll.AddExpectedObjectState(city, c => c.Visited && c.SalesmanHere);
                else
                    VisitAll.AddExpectedObjectState(city, c => c.Visited);
            }

            TSP.AddGoal(VisitAll);
            TSP.PlanGenerated += PrintPlan;

            TSP.Start();
            Console.ReadKey();
        }
    }
}