using System;
using Mix.Heart.Entity;

namespace Mix.Example.Infrastructure.Entities
{
    public class ProductEntity : Entity
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Producer { get; set; }

        public Guid CategoryId { get; set; }
    }
}
