using Microsoft.EntityFrameworkCore;
using Mix.Heart.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mix.Heart.Helpers
{
    public static class ViewModelHelper<TDbContext, TEntity, TPrimaryKey>
        where TPrimaryKey : IComparable
        where TEntity : class, IEntity<TPrimaryKey>
        where TDbContext : DbContext
    {

    }
}
