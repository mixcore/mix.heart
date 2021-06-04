using System;
using Mix.Heart.Entity;

namespace Mix.Example.Infrastructure.Entities
{
    public class ProductDetailEntity : Entity
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public int Quantity { get; set; }

        public int InventoryNumber { get; set; }

        public Guid ProductId { get; set; }
    }
}
