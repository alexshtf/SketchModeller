using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoDiff
{
    public class IntPower : Term
    {
        public IntPower(Term baseTerm, int exponent)
        {
            Base = baseTerm;
            Exponent = exponent;
        }

        public Term Base { get; private set; }
        public int Exponent { get; private set; }

        public override void Accept(ITermVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
