namespace hoppa.Service.Model
{
    public class Action
    {
        public string Type { get; set; }
    }

    public class Mail : Action
    {
        public string EmailAddress { get; set; }
    }

    public class Payment : Action
    {
        public string Account { get; set; }

        public Amount Amount { get; set; }
        public Pointer Recipient { get; set; }

        public string Description { get; set; }
    }

    public class Amount
    {
        public string Value { get; set; }
        public string Currency { get; set; }

        public Amount(string value, string currency)
        {
            Value = value;
            Currency = currency;
        }
    }

    public class Pointer
    {
        public string Type { get; set; }
        public string Value { get; set; }
        public string Name { get; set; }

        public Pointer(string type, string value, string name)
        {
            Type = type;
            Value = value;
            Name = name;
        }
    }
}