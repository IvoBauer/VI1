using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net.Http;
using VI1_TSP.Models;

namespace VI1_TSP.Controllers
{
    public class TspController : Controller
    {
        private readonly ILogger<TspController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public TspController(ILogger<TspController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;

        }
        public  IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetPath()
        {
            List<Location> locations = generateLocations();
            List<Edge> edges = generateEdges(locations);
            edges = await calculateEdgesAsync(edges);

            List<Location> bestRoute = new List<Location>();
            double bestDistance = double.MaxValue;
            if (edges.Count > 2)
            {                                
                List<Location> routeCandidate = new List<Location>();
                
                //routeCandidate = GeneticAlgorithmSolver(locations, 100, 20, 0.01, 500, edges);
                //if (GetRouteDistance(routeCandidate,edges) < bestDistance)
                //{
                //    bestRoute = routeCandidate;
                //    bestDistance = GetRouteDistance(routeCandidate, edges);
                //    Console.WriteLine(bestDistance);
                //}

                routeCandidate = GeneticAlgorithmSolver(locations, 100, 20, 0.01, 1500, edges);
                if (GetRouteDistance(routeCandidate, edges) < bestDistance)
                {
                    bestRoute = routeCandidate;
                    bestDistance = GetRouteDistance(routeCandidate, edges);
                    Console.WriteLine(bestDistance);
                }

                //--------------------------------------------------------------------------------------------
                //routeCandidate = GeneticAlgorithmSolver(locations, 100, 20, 0.05, 1500, edges);
                //if (GetRouteDistance(routeCandidate, edges) < bestDistance)
                //{
                //    bestRoute = routeCandidate;
                //    bestDistance = GetRouteDistance(routeCandidate, edges);
                //    Console.WriteLine(bestDistance);
                //}

                //routeCandidate = GeneticAlgorithmSolver(locations, 100, 20, 0.05, 1500, edges);
                //if (GetRouteDistance(routeCandidate, edges) < bestDistance)
                //{
                //    bestRoute = routeCandidate;
                //    bestDistance = GetRouteDistance(routeCandidate, edges);
                //    Console.WriteLine(bestDistance);
                //}

                ////--------------------------------------------------------------------------------------------
                //routeCandidate = GeneticAlgorithmSolver(locations, 200, 20, 0.01, 1500, edges);
                //if (GetRouteDistance(routeCandidate, edges) < bestDistance)
                //{
                //    bestRoute = routeCandidate;
                //    bestDistance = GetRouteDistance(routeCandidate, edges);
                //    Console.WriteLine(bestDistance);
                //}

                //routeCandidate = GeneticAlgorithmSolver(locations, 200, 20, 0.1, 1500, edges);
                //if (GetRouteDistance(routeCandidate, edges) < bestDistance)
                //{
                //    bestRoute = routeCandidate;
                //    bestDistance = GetRouteDistance(routeCandidate, edges);
                //    Console.WriteLine(bestDistance);
                //}

                ////--------------------------------------------------------------------------------------------
                //routeCandidate = GeneticAlgorithmSolver(locations, 200, 20, 0.05, 1500, edges);
                //if (GetRouteDistance(routeCandidate, edges) < bestDistance)
                //{
                //    bestRoute = routeCandidate;
                //    bestDistance = GetRouteDistance(routeCandidate, edges);
                //    Console.WriteLine(bestDistance);
                //}

                //routeCandidate = GeneticAlgorithmSolver(locations, 200, 20, 0.5, 1500, edges);
                //if (GetRouteDistance(routeCandidate, edges) < bestDistance)
                //{
                //    bestRoute = routeCandidate;
                //    bestDistance = GetRouteDistance(routeCandidate, edges);
                //    Console.WriteLine(bestDistance);
                //}



                Console.WriteLine("Best distance= " + GetRouteDistance(bestRoute, edges));
                string destinaceVypis = "";
                foreach (var location in bestRoute) {
                    destinaceVypis += " " + location.Name;
                }
                Console.WriteLine(destinaceVypis);
            }            
            return View(bestRoute);
        }

        public static double GetRouteDistance (List<Location> route, List<Edge> edges)
        {
            double distance = 0;
            for (int i = 0; i < route.Count-1; i++)
            {
                distance += FindEdge(route[i], route[i+1], edges).Length;
            }

            return distance;
        }
        public static List<Location> GeneticAlgorithmSolver(List<Location> population, int populationSize, int eliteSize, double mutationRate, int generations, List<Edge> edges)
        {
            List<List<Location>> pop = CreateInitialPopulation(populationSize, population);

            double bestDistance = Double.MaxValue;
            List<Location> bestRoute = new List<Location>();

            for (int i = 0; i < generations; i++)
            {
                pop = NextGeneration(pop, eliteSize, mutationRate, edges);
                double actualDistance = ((1 / RankPopulation(pop, edges)[0].Item2)/ 1000);
                
                if (actualDistance < bestDistance)
                {                    
                    bestDistance = actualDistance;
                    bestRoute = pop[RankPopulation(pop, edges)[0].Item1];
                }                
            }

            Console.WriteLine("Distance= " + bestDistance + " | Pop= " + populationSize + " | Elite= " + eliteSize + " | MutationRate= " + mutationRate + " | Generations= " + generations);
            //int bestRouteIndex = RankPopulation(pop, edges)[0].Item1;                 
            
            return bestRoute;
        }


        public static List<Location> CreateRoute(List<Location> locations)
        {
            return locations.OrderBy(x => Guid.NewGuid()).ToList();
        }


        //Vytvoří populaci o dané velikosti. Jedna populace je náhodná trasa mezi lokacemi.
        public static List<List<Location>> CreateInitialPopulation(int populationSize, List<Location> locations)
        {
            List<List<Location>> population = new List<List<Location>>();
            for (int i = 0; i < populationSize; i++)
            {
                population.Add(CreateRoute(locations));
            }

            return population;
        }

        public static Edge FindEdge(Location location1, Location location2, List<Edge> edges)
        {
            List<Edge> foundEdges = edges.Where(x => x.Start == location1).Where(y => y.End == location2).ToList();
            foundEdges.AddRange(edges.Where(x => x.Start == location2).Where(y => y.End == location1).ToList());                       
            
            return foundEdges[0];
        }

        //Vrací seznam ohodnocených tras. 
        public static List<(int, double)> RankPopulation(List<List<Location>> population, List<Edge> edges)
        {
            List<(int, double)> rankedRoutes = new List<(int, double)>();

            for (int i = 0; i < population.Count(); i++)
            {
                List<Edge> popEdges = new List<Edge>();
                for (int j = 0; j < population[i].Count-1; j++)
                {
                    popEdges.Add(FindEdge(population[i][j], population[i][j+1], edges));
                }
                rankedRoutes.Add((i, new Fitness(population[i]).RouteFitnessScore(popEdges)));
            }

            return rankedRoutes.OrderByDescending(x => x.Item2).ToList();
        }


        //Elite size = počet elitních jedinců, kteří přežijí z dané generace
        //Výběr se provádí podle (fitness každého jedince / fitness populace)
        public static List<int> Selection(List<(int, double)> rankedRoutes, int eliteSize)
        {
            List<int> selectionResults = new List<int>();
            double[] cumulativeFitness = new double[rankedRoutes.Count]; // 0, 0.156 0.159 0.206,.. 1
            double totalFitness = rankedRoutes.Sum(x => x.Item2);



            cumulativeFitness[0] = rankedRoutes[0].Item2 / totalFitness;
            for (int i = 1; i < rankedRoutes.Count; i++)
            {
                cumulativeFitness[i] = cumulativeFitness[i - 1] + rankedRoutes[i].Item2 / totalFitness;
            }

            //Výběr nejlepších jedinců, abychom zajistili jejich postup do další generace
            for (int i = 0; i < eliteSize; i++)
            {
                selectionResults.Add(rankedRoutes[i].Item1);
            }

            //Doplní zbytek jedinců
            for (int i = 0; i < rankedRoutes.Count - eliteSize; i++)
            {
                double pick = new Random().NextDouble(); // generuje hodnotu 0.0 =< x < 1
                for (int j = 0; j < rankedRoutes.Count; j++) //projede všechny rankedRoutes (všech 100)
                {
                    if (pick <= cumulativeFitness[j]) //pokud je hodnota pick menší, než na dané úrovni kumulativní fitness
                    {
                        selectionResults.Add(rankedRoutes[j].Item1); //přidá se jedinec do selectionResults
                        break;
                    }
                }
            }

            return selectionResults;
        }

        //Přidání vybraných jedinců do listu, ze kterých se bude vytvářet nová generace
        public static List<List<Location>> MatingPool(List<List<Location>> population, List<int> selectionResults)
        {
            List<List<Location>> matingPool = new List<List<Location>>();

            foreach (int i in selectionResults)
            {
                matingPool.Add(population[i]);
            }


            return matingPool;
        }

        //Z rodiče vrací geny v podobě int. Na 0. místě je menší, na 1. místě větší.
        public static int[] GetGenes(List<Location> parent)
        {
            int[] genes = new int[2];
            Random random = new Random();

            int geneA = random.Next(parent.Count);
            int geneB = random.Next(parent.Count);
            genes[0] = Math.Min(geneA, geneB);
            genes[1] = Math.Max(geneA, geneB);

            return genes;
        }

        //Ze dvou předků vznikne nový potomek 
        public static List<Location> Breed(List<Location> firstParent, List<Location> secondParent)
        {
            List<Location> newChild = new List<Location>();
            List<Location> childFirstPart = new List<Location>();
            List<Location> childSecondPart = new List<Location>();

            int[] genes = GetGenes(firstParent);

            //Daný rozsah z 1. předka přidá do potomka
            for (int i = genes[0]; i < genes[1]; i++)
            {
                childFirstPart.Add(firstParent[i]);
            }

            //Z 2. předka doplní chybějící lokace, které se z 1. předka nepřevzaly
            childSecondPart = secondParent.Where(location => !childFirstPart.Contains(location)).ToList();

            //Spojení obou částí potomka
            newChild = childFirstPart.Concat(childSecondPart).ToList();

            return newChild;
        }

        //Vytvoření populace nových potomků
        public static List<List<Location>> BreedPopulation(List<List<Location>> matingPool, int eliteSize)
        {
            List<List<Location>> children = new List<List<Location>>();


            //Přidání elitních jedinců do další generace
            for (int i = 0; i < eliteSize; i++)
            {
                children.Add(matingPool[i]);
            }

            //Náhodné zamíchání matingPool do randomizedPool
            List<List<Location>> randomizedPool = matingPool.OrderBy(x => Guid.NewGuid()).ToList();

            //Upravení zbytku jedinců do další generace
            int length = matingPool.Count - eliteSize;
            for (int i = 0; i < length; i++)
            {
                List<Location> child = Breed(randomizedPool[i], randomizedPool[randomizedPool.Count - 1]);
                children.Add(child);
            }

            return children;
        }

        //S pravděpodobností mutationRate dojde k prohození jednotlivých lokací u jedince
        public static List<Location> Mutate(List<Location> individual, double mutationRate)
        {
            List<Location> mutatedIndividual = new List<Location>(individual);
            Random random = new Random();

            for (int locationIndex = 0; locationIndex < individual.Count; locationIndex++)
            {
                if (random.NextDouble() < mutationRate)
                {
                    int secondLocationIndex = random.Next(mutatedIndividual.Count);

                    Location firstLocation = mutatedIndividual[locationIndex]; //Dočasná hodnota kvůli prohození lokací TODO
                    mutatedIndividual[locationIndex] = mutatedIndividual[secondLocationIndex];
                    mutatedIndividual[secondLocationIndex] = firstLocation;
                }
            }

            return mutatedIndividual;
        }

        //Provedení Mutate pro všechny členy populace
        public static List<List<Location>> MutatePopulation(List<List<Location>> population, double mutationRate)
        {
            List<List<Location>> mutatedPopulation = new List<List<Location>>();

            foreach (List<Location> individual in population)
            {
                mutatedPopulation.Add(Mutate(individual, mutationRate));
            }

            return mutatedPopulation;
        }

        public static List<List<Location>> NextGeneration(List<List<Location>> currentGeneration, int eliteSize, double mutationRate, List<Edge> edges)
        {
            List<(int, double)> rankedPopulation = RankPopulation(currentGeneration, edges);
            List<int> selectionResults = Selection(rankedPopulation, eliteSize);
            List<List<Location>> matingPool = MatingPool(currentGeneration, selectionResults);
            List<List<Location>> children = BreedPopulation(matingPool, eliteSize);
            List<List<Location>> nextGeneration = MutatePopulation(children, mutationRate);

            return nextGeneration;
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private async Task<List<Edge>> calculateEdgesAsync(List<Edge> edges)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();

                // Nastavení adresy API
                var baseAddress = "https://api.mapy.cz/";
                var apiKey = "Zu0EQK50_ScAQVieAwNxtltpOy8n2-F2dT6S6Zx6Evw";
                client.BaseAddress = new Uri("https://api.mapy.cz/");

                foreach (Edge edge in edges)
                {
                    string startPoint = edge.Start.Longitude.ToString().Replace(",", ".") + "," + edge.Start.Latitude.ToString().Replace(",", ".");
                    string endPoint = edge.End.Longitude.ToString().Replace(",", ".") + "," + edge.End.Latitude.ToString().Replace(",", ".");
                    string requestUri = $"v1/routing/route?start={startPoint}&end={endPoint}&routeType=car_fast&format=polyline&apikey={apiKey}";
                    var response = await client.GetAsync(baseAddress + requestUri);

                    // Zkontrolujeme, zda je odpověď úspěšná
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();


                    var jsonObject = JObject.Parse(responseBody); //Nutné hodit do TRY CATCH

                    string edgeLength = (string)jsonObject["length"];
                    edge.Length = Double.Parse(edgeLength);

                    string edgeDuration = (string)jsonObject["duration"];
                    edge.Duration = Double.Parse(edgeDuration);
                }

            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("Chyba při zpracování požadavku");
            }
            return edges;
        }
        private List<Location> generateLocations()
        {
            List<Location> locations = new List<Location>();
            //locations.Add(new Location(50.21836276112238, 15.83443262778331)); //1
            //locations.Add(new Location(50.2174016528686, 15.84185698188508)); //2
            //locations.Add(new Location(50.21542445500339, 15.846792246172384)); //3
            //locations.Add(new Location(50.21199162520274, 15.85498907642347)); //4
            //locations.Add(new Location(50.20397157242685, 15.858293557728881)); //5
            //locations.Add(new Location(50.197323794966444, 15.847779299029844)); //6
            //locations.Add(new Location(50.20062032925635, 15.836063410765203)); //7
            //locations.Add(new Location(50.20839375452244, 15.820785549145375)); //8
            //locations.Add(new Location(50.21306269460837, 15.819541004238143)); //9                                                      
            //locations.Add(new Location(50.21245850456618, 15.814004925168039)); //10


            //locations.Add(new Location(50.20031223574316, 15.859390540552772)); //11
            //locations.Add(new Location(50.194158481218494, 15.843426033466889)); //12
            //locations.Add(new Location(50.19794972736611, 15.831838891227132)); //13
            //locations.Add(new Location(50.18695977971852, 15.838791176570984)); //14
            //locations.Add(new Location(50.1866300422092, 15.851665779059603)); //15



            //locations.Add(new Location(50.201255875094674, 15.994424465509029)); //1
            //locations.Add(new Location(50.22785272438098, 16.000940718639157)); //2
            //locations.Add(new Location(50.24959251818832, 16.012916535202635)); //3
            //locations.Add(new Location(50.24103295369522, 16.02313120227149)); //4
            //locations.Add(new Location(50.259051301025366, 16.066631594788827)); //5
            //locations.Add(new Location(50.238216971466535, 16.10608134346852)); //5


            locations.Add(new Location(50.29142923994021, 16.161622244880324, "Dobruška")); //DOBRUŠKA
            locations.Add(new Location(50.302613306912086, 16.35207204626744, "Deštné v orlických horách")); //Deštné v orlických horách
            locations.Add(new Location(50.23820073112768, 16.410146425165756, "Zdobnice")); //Zdobnice
            locations.Add(new Location(50.16276020987021, 16.46309659416128, "Rokytnice v Orlických horách")); //Rokytnice v Orlických horách
            locations.Add(new Location(50.15263774299893, 16.422956949922735, "Pečín")); //Pečín
            locations.Add(new Location(50.117876754594526, 16.288446439974436, "Vamberk")); //Vamberk
            locations.Add(new Location(50.1230788239319, 16.21115627394065, "Kostelec nad Orlicí")); //Kostelec nad Orlicí
            locations.Add(new Location(50.14798616124803, 16.071948571581455, "Týniště nad Orlicí")); //Týniště nad Orlicí
            locations.Add(new Location(50.213067137393644, 16.075364711516652, "Bolehošť")); //Bolehošť
            locations.Add(new Location(50.25813426771307, 16.066397344186765, "Mokré")); //Mokré
            locations.Add(new Location(50.26714258208283, 16.1103801458524, "Opočno")); //Opočno
            return locations;
        }
        private List<Edge> generateEdges(List<Location> locations)
        {
            int locIndex = 0;
            List<Edge> edges = new List<Edge>();

            foreach (Location location in locations)
            {
                for (int i = locIndex + 1; i < locations.Count; i++)
                {
                    edges.Add(new Edge(locations[locIndex], locations[i], 0, 0));
                }
                locIndex++;
            }

            return edges;
        }
    }
}
