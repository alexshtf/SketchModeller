using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Services;
using SketchModeller.Infrastructure.Data;

namespace SketchModeller.Modelling.Services.Duplicator
{
    class PrimitivesDuplicator : IPrimitivesDuplicator
    {
        private Dictionary<Tuple<Type, Type>, IDuplicator> typesToDuplicator;

        public PrimitivesDuplicator()
        {
            typesToDuplicator = new Dictionary<Tuple<Type, Type>, IDuplicator>();
            RegisterDuplicator(new ConeConeDuplicator());
            RegisterDuplicator(new ConeCylinderDuplicator());
            RegisterDuplicator(new CylinderCylinderDuplicator());
            RegisterDuplicator(new CylinderConeDuplicator());
        }

        public NewPrimitive Duplicate(SnappedPrimitive snapped, Type targetType)
        {
            var sourceType = snapped.GetType();
            var key = Tuple.Create(sourceType, targetType);
            var duplicator = typesToDuplicator[key];
            return duplicator.Duplicate(snapped);
        }

        public Type[] GetValidDuplicateTypes(SnappedPrimitive snapped)
        {
            var sourceType = snapped.GetType();
            var query =
                from key in typesToDuplicator.Keys
                where key.Item1 == sourceType
                select key.Item2;
            return query.ToArray();
        }

        public Type GetNaturalDuplicateType(SnappedPrimitive snapped)
        {
            var sourceType = snapped.GetType();
            var query =
                from kvp in typesToDuplicator
                let key = kvp.Key
                let duplicator = kvp.Value
                where key.Item1 == sourceType && duplicator.IsNatural
                select key.Item2;
            return query.First();
        }

        private void RegisterDuplicator(IDuplicator duplicator)
        {
            var key = Tuple.Create(duplicator.SnappedType, duplicator.NewType);
            typesToDuplicator[key] = duplicator;
        }
    }
}
