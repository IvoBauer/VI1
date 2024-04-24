namespace VI1_TSP.Models
{
    public class Fitness
    {
        public List<Location> Route { get; set; }
        public double Distance = 0;
        public double FitnessScore = 0.0;
        public double y { get; set; }

        public Fitness(List<Location> route)
        {
            this.Route = route;
            this.Distance = 0.0;
            this.FitnessScore = 0.0;
        }

        public double RouteDistance(List<Edge> edges)
        {
            double routeDistance = 0;
            foreach (Edge edge in edges)
            {
                routeDistance += edge.Length;
            }

            Distance = routeDistance;
            return Distance;
        }

        public double RouteFitnessScore(List<Edge> edges)
        {
            if (this.FitnessScore == 0)
            {
                this.FitnessScore = 1 / (float)this.RouteDistance(edges);
            }
            return this.FitnessScore;
        }
    }
}
