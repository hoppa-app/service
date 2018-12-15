namespace hoppa.Service.Model
{
    public class Condition
    {
        public string Type { get; set; }
    }

    public class Tigger : Condition
    {
        public string When { get; set; }
    }

    public class Mutation : Condition
    {
        public string Description { get; set; }
    }
}
