using System.Collections.Generic;
using SketchModeller.Infrastructure.Data;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SketchModeller.Modelling.Services.UndoHistory
{
    class SerializingMemoryUndoStack : IUndoStack
    {
        private readonly BinaryFormatter formatter;
        private readonly Stack<byte[]> stack;

        public SerializingMemoryUndoStack()
        {
            formatter = new BinaryFormatter();
            stack = new Stack<byte[]>();
        }

        public void Push(SketchData sketchData)
        {
            var sketchDataWithoutDistanceTransforms = GetSketchDataWithoutDistanceTransforms(sketchData);

            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, sketchDataWithoutDistanceTransforms);
                stream.Flush();
                stack.Push(stream.ToArray());
            }
        }

        public SketchData Pop()
        {
            if (stack.Count == 0)
                return null;

            using (var stream = new MemoryStream(stack.Pop()))
            {
                var sketchData = (SketchData) formatter.Deserialize(stream);
                return sketchData;
            }
        }

        public void Clear()
        {
            stack.Clear();
        }

        private static SketchData GetSketchDataWithoutDistanceTransforms(SketchData sketchData)
        {
            var result = new SketchData()
            {
                Annotations = sketchData.Annotations,
                Curves = sketchData.Curves,
                NewPrimitives = sketchData.NewPrimitives,
                SnappedPrimitives = sketchData.SnappedPrimitives,
            };
            return result;
        }
    }
}
