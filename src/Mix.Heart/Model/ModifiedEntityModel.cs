using Mix.Heart.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mix.Heart.Model
{
    public sealed class ModifiedEntityModel
    {
        public ModifiedEntityModel()
        {

        }

        public ModifiedEntityModel(Type type, object id, ViewModelAction action)
        {
            EntityType = type;
            Id = id;
            Action = action;
            CacheFolder = EntityType.FullName;
        }
        public ModifiedEntityModel(Type type, object id, ViewModelAction action, string cacheFolder)
        {
            EntityType = type;
            Id = id;
            Action = action;
            CacheFolder = cacheFolder;
        }

        public object Id { get; set; }
        public Type EntityType { get; set; }
        public ViewModelAction Action { get; set; }
        public string CacheFolder { get; set; }
    }
}
