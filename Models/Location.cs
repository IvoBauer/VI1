namespace VI1_TSP.Models
{
    public class Location
    {
        public Location(double latitude, double longitude, string? name)
        {
            Latitude = latitude;
            Longitude = longitude;
            Name = name;
        }

        public Location(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public Location() { }


        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Name {get; set;}

        public double Distance(Location distanceTo, List<Edge> edges)
        {
            double distance = 0;
            if (edges.Count > 0)
            {

                try
                {
                    Edge testEdge = edges.Find(x => x.Start == distanceTo || x.End == distanceTo);
                    if (testEdge is not null)
                    {
                        distance = testEdge.Length;
                    }
                }
                catch (ArgumentNullException)
                {
                    distance = 0;
                }
            }

            return distance;
        }
    }
}
