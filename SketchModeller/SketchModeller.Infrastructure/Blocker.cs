using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchModeller.Infrastructure
{
    public class Blocker
    {
        public bool IsBlocked { get; private set; }

        public void Do(Action action)
        {
            if (!IsBlocked)
            {
                try
                {
                    IsBlocked = true;
                    action();
                }
                finally
                {
                    IsBlocked = false;
                }
            }
        }
    }
}
