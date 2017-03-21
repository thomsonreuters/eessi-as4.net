namespace ExponentialIntervalReceiver
{
    public class ExecuteResult
    {
        public string Name { get; }
        public bool Results { get; }

        public ExecuteResult(string name, bool result)
        {
            this.Name = name;
            this.Results = result;
        }
    }
}