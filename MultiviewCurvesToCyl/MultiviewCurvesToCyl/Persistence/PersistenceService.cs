using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;

namespace MultiviewCurvesToCyl.Persistence
{
    class PersistenceService
    {
        public void Save(string fileName, State state)
        {
            using (var fileStream = File.Create(fileName))
            {
                var dataContractSerializer = new DataContractSerializer(typeof(State));
                dataContractSerializer.WriteObject(fileStream, state);
            }
        }

        public State Load(string fileName)
        {
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            {
                var ser = new DataContractSerializer(typeof(State));
                var state = (State)ser.ReadObject(fileStream);
                return state;
            }
        }
    }
}
