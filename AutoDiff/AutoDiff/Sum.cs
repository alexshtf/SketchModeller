using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace AutoDiff
{
    public class Sum : Term
    {
        public Sum(Term first, Term second, params Term[] rest)
        {
            var allTerms = 
                (new Term[] { first, second}).Concat(rest);

            Terms = allTerms.ToList().AsReadOnly();
        }

        internal Sum(IEnumerable<Term> terms)
        {
            Terms = terms.ToList().AsReadOnly();
        }

        public ReadOnlyCollection<Term> Terms { get; private set; }
        
        public override void Accept(ITermVisitor visitor)
        {
            visitor.Visit(this);
        }

    }
}
