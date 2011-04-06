using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using SketchModeller.Infrastructure.Data;
using System.Windows;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Collections.Specialized;
using System.ComponentModel;
using Utils;
using ContinuousLinq;

namespace SketchModeller.Infrastructure.Shared
{
    public class SessionData : NotificationObject
    {
        private readonly SelectionListener<NewPrimitive> newPrimitivesSelectionListener;
        private readonly SelectionListener<PointsSequence> sketchObjectsSelectionListener;
        private readonly SelectionListener<FeatureCurve> featureCurvesSelectionListener;
        private readonly SelectionListener<SelectablePrimitive> allPrimitivesSelectionListener;
        private readonly ReadOnlyContinuousCollection<SelectablePrimitive> allPrimitives;

        public SessionData()
        {
            NewPrimitives = new ObservableCollection<NewPrimitive>();
            SnappedPrimitives = new ObservableCollection<SnappedPrimitive>();
            Annotations = new ObservableCollection<Annotation>();
            sketchObjects = new PointsSequence[0];
            FeatureCurves = new ObservableCollection<FeatureCurve>();

            newPrimitivesSelectionListener = new SelectionListener<NewPrimitive>(NewPrimitives, p => p.IsSelected);
            sketchObjectsSelectionListener = new SelectionListener<PointsSequence>(sketchObjects, o => o.IsSelected);
            featureCurvesSelectionListener = new SelectionListener<FeatureCurve>(FeatureCurves, c => c.IsSelected);

            allPrimitives = 
                NewPrimitives
                .Select(x => (SelectablePrimitive)x)
                .Concat(SnappedPrimitives.Select(x => (SelectablePrimitive)x));
            allPrimitivesSelectionListener = new SelectionListener<SelectablePrimitive>(allPrimitives, p => p.IsSelected);

            SelectedNewPrimitives = newPrimitivesSelectionListener.SelectedItems;
            SelectedSketchObjects = sketchObjectsSelectionListener.SelectedItems;
            SelectedFeatureCurves = featureCurvesSelectionListener.SelectedItems;
            SelectedPrimitives = allPrimitivesSelectionListener.SelectedItems;
        }

        #region SketchData property

        private SketchData sketchData;

        /// <summary>
        /// The last loaded/saved sketch data.
        /// </summary>
        public SketchData SketchData
        {
            get { return sketchData; }
            set
            {
                sketchData = value;
                RaisePropertyChanged(() => SketchData);
            }
        }

        #endregion

        #region SketchName property

        private string sketchName;

        /// <summary>
        /// The currently loaded name.
        /// </summary>
        public string SketchName
        {
            get { return sketchName; }
            set
            {
                sketchName = value;
                RaisePropertyChanged(() => SketchName);
            }
        }

        #endregion

        public ObservableCollection<NewPrimitive> NewPrimitives { get; private set; }

        public ObservableCollection<SnappedPrimitive> SnappedPrimitives { get; private set; }

        public ObservableCollection<Annotation> Annotations { get; private set; }

        public ObservableCollection<FeatureCurve> FeatureCurves { get; private set; }

        #region SketchObjects property

        private PointsSequence[] sketchObjects;

        public PointsSequence[] SketchObjects
        {
            get { return sketchObjects; }
            set
            {
                sketchObjects = value;
                RaisePropertyChanged(() => SketchObjects);
                sketchObjectsSelectionListener.Reset(value);
            }
        }

        #endregion

        #region DistanceTransforms property

        private int[][,] distanceTransforms;

        public int[][,] DistanceTransforms
        {
            get { return distanceTransforms; }
            set
            {
                distanceTransforms = value;
                RaisePropertyChanged(() => DistanceTransforms);
            }
        }

        #endregion

        public ReadOnlyObservableCollection<SelectablePrimitive> SelectedPrimitives { get; private set; }
        public ReadOnlyObservableCollection<NewPrimitive> SelectedNewPrimitives { get; private set; }
        public ReadOnlyObservableCollection<PointsSequence> SelectedSketchObjects { get; private set; }
        public ReadOnlyObservableCollection<FeatureCurve> SelectedFeatureCurves { get; private set; }

    }
}
