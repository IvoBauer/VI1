namespace VI1_TSP.Models
{
    public class Edge
    {
        public Edge(Location start, Location end, double length, double duration)
        {
            Start = start;
            End = end;
            Length = length;
            Duration = duration;
        }

        public Location Start { get; set; }
        public Location End { get; set; }
        public double Length { get; set; }
        public double Duration { get; set; }
    }
}
