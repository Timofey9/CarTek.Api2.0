namespace CarTek.Api.Model.Orders
{
    public class Material
    {
        public long Id { get; set; }

        public string Name { get; set; }
        
        public ICollection<Order> Orders { get;set; }
    }

    public class CreateMaterialModel
    {
        public string Name { get; set; }
    }
}
