using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using System.Collections.ObjectModel;

namespace SketchModeller.Infrastructure.Data
{
    /// <summary>
    /// Abstraction over a complex curve. A sequence of polylines and joints.
    /// </summary>
    /// <remarks>
    /// <para>
    /// We assume that the joints join different polylines, and that the end-point of one
    /// polyline is the start point of the next. That is, the sequence looks like this "j-p-j-p-j-p-j" where j
    /// is a joint and p is a polyline. We have a joint before the first polyline, a joint after the last and 
    /// </para>
    /// <para>
    /// Formally - let <b>m</b> be the number of polylines and <b>n</b> be the number of joints. We assume
    /// that:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// If <b>m</b> is 0 then <b>n</b> is 0. Otherwise, <b>n = m + 1</b>. That is, the number of joints
    /// is one more than the number of polylines, unless we have no polylines.
    /// </item>
    /// <item>
    /// <c>Joints[0]</c> is a "joint" before the first polyline. <c>Joints[n]</c> is a "joint" after the 
    /// last polyline.
    /// </item>
    /// <item>
    /// For all <b>i</b> in <b>[1..m-2]</b> we have that the joint <c>Joints[i]</c> connects the polylines
    /// <c>Polylines[i]</c> to <c>Polylines[i + 1]</c>.
    /// </item>
    /// <item>
    /// For all <b>i</b> in <b>[0..m-2]</b> we have that the last point of <c>Polylines[i]</c> is the first point
    /// of <c>Polylines[i + 1]</c>.
    /// </item>
    /// <item>
    /// For all <b>i</b> in <b>[0..m-1]</b> we have that <c>Polylines[i] != null</c>.
    /// </item>
    /// <item>
    /// For all <b>j</b> in <b>[0..n-1]</b> we have that <c>Joints[j] != null</c>.
    /// </item>
    /// </list>
    /// </remarks>
    public class ComplexCurve : NotificationObject
    {
        public ComplexCurve()
        {
            Polylines = new ObservableCollection<Polyline>();
            Joints = new ObservableCollection<Joint>();
        }

        public ObservableCollection<Polyline> Polylines { get; private set;}
        public ObservableCollection<Joint> Joints { get; private set; }
    }
}
