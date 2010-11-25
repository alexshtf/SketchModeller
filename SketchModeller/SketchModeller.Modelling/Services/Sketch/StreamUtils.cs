using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace SketchModeller.Modelling.Services.Sketch
{
    class StreamUtils
    {
        public Task<byte[]> ReadToEndAsync(Stream stream, int length)
        {
            byte[] buffer = new byte[length];

            Func<int, Task<int>> createContinuation = null;
            createContinuation = offset =>
                 {
                     var task = Task<int>.Factory.FromAsync(
                        beginMethod: stream.BeginRead, 
                        endMethod: stream.EndRead, 
                        arg1: buffer, 
                        arg2: offset, 
                        arg3: length - offset, 
                        state: null);

                     var continuation = task.ContinueWith(completedTask =>
                         {
                             var bytesRead = completedTask.Result;
                             if (bytesRead > 0)
                                 return createContinuation(offset + bytesRead);
                             else
                                 return Task<int>.Factory.StartNew(() => 0);
                         }, TaskContinuationOptions.OnlyOnRanToCompletion);

                     return continuation.Unwrap();
                 };

            var resultingTask = createContinuation(0).ContinueWith(completedTask =>
                {
                    if (completedTask.IsFaulted)
                        throw completedTask.Exception;
                    else
                        return buffer;
                });

            return resultingTask;
        }
    }
}
