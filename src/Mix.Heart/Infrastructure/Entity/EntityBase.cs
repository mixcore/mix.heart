using Mix.Heart.Enums;
using System;

namespace Mix.Heart.Entity {
  public abstract class EntityBase<TPrimaryKey>
      : IEntity<TPrimaryKey> where TPrimaryKey : IComparable {
    public TPrimaryKey Id {
      get;
      set;
    }
    public DateTime CreatedDateTime {
      get;
      set;
    }
    public DateTime? LastModified {
      get;
      set;
    }
    public Guid CreatedBy {
      get;
      set;
    }
    public Guid? ModifiedBy {
      get;
      set;
    }
    public int Priority {
      get;
      set;
    }
    public MixContentStatus Status {
      get;
      set;
    }
  }
}
