using Microsoft.WindowsAzure.Storage.Table;

namespace WebApplication.models
{
    public class Customer : TableEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }

        public Customer()
        {
        }

        public Customer(Customer sample)
        {
            Id = sample.Id;
            Name = sample.Name;
            Age = sample.Age;

            PartitionKey = "Customer";
            RowKey = Id;
        }
    }
}
