using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Services;
using SketchModeller.Infrastructure.Data;

using NewConverterKey = System.Tuple<System.Type, System.Type>;
using System.Windows.Media.Media3D;

namespace SketchModeller.Modelling.Services.PrimitivesConverter
{
    class PrimitivesConverter : IPrimitivesConverter
    {
        private Dictionary<NewConverterKey, INewConverter> newConvertersRegistry;
        private Dictionary<Type, ISnappedConverter> snappedConvertersRegistry;

        public PrimitivesConverter()
        {
            newConvertersRegistry = new Dictionary<NewConverterKey, INewConverter>();
            snappedConvertersRegistry = new Dictionary<Type, ISnappedConverter>();
        }

        public NewPrimitive SnappedToNew(SnappedPrimitive source)
        {
            return snappedConvertersRegistry[GetKey(source)].Convert(source);
        }

        public NewPrimitive NewToNew(NewPrimitive source, Type targetType, Vector3D moveVector)
        {
            return newConvertersRegistry[GetKey(source, targetType)].Convert(source, moveVector);
        }

        public Type[] GetTargetTypes(NewPrimitive source)
        {
            var query =
                from key in newConvertersRegistry.Keys
                where key.Item1 == source.GetType()
                select key.Item2;
            return query.ToArray();
        }

        private void RegisterConverter(INewConverter newConverter)
        {
            newConvertersRegistry.Add(GetKey(newConverter), newConverter);
        }

        private void RegisterConverter(ISnappedConverter snappedConverter)
        {
            snappedConvertersRegistry.Add(GetKey(snappedConverter), snappedConverter);
        }

        #region GetKey methods

        private static NewConverterKey GetKey(INewConverter converter)
        {
            return Tuple.Create(converter.SourceType, converter.TargetType);
        }

        private static NewConverterKey GetKey(NewPrimitive source, Type targetType)
        {
            return Tuple.Create(source.GetType(), targetType);
        }

        private static Type GetKey(ISnappedConverter converter)
        {
            return converter.SnappedType;
        }

        private static Type GetKey(SnappedPrimitive source)
        {
            return source.GetType();
        }

        #endregion
    }
}
