using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mix.Heart.Entities
{
    public abstract class SimpleEntityBase<TPrimaryKey> : IEntity<TPrimaryKey> where TPrimaryKey : IComparable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public TPrimaryKey Id { get; set; }
    }
}
